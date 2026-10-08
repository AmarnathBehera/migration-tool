using System.Collections.Immutable;
using System.Diagnostics;
using System.Xml.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.OnlineForms;
using MediatR;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using Migration.Tool.Common;
using Migration.Tool.Common.Abstractions;
using Migration.Tool.Common.MigrationProtocol;
using Migration.Tool.Common.Services.BulkCopy;
using Migration.Tool.KXP.Api;
using Migration.Tool.Source.Contexts;
using Migration.Tool.Source.Model;

namespace Migration.Tool.Source.Handlers;

public class MigrateFormsCommandHandler(
    ILogger<MigrateFormsCommandHandler> logger,
    IEntityMapper<ICmsClass, DataClassInfo> dataClassMapper,
    IEntityMapper<ICmsForm, BizFormInfo> cmsFormMapper,
    KxpClassFacade kxpClassFacade,
    BulkDataCopyService bulkDataCopyService,
    PrimaryKeyMappingContext primaryKeyMappingContext,
    IProtocol protocol,
    ModelFacade modelFacade,
    ToolConfiguration configuration
)
    : IRequestHandler<MigrateFormsCommand, CommandResult>
{
    public async Task<CommandResult> Handle(MigrateFormsCommand request, CancellationToken cancellationToken)
    {
        var migratedSiteIds = modelFacade.GetMigratedSites().Select(site => site.SiteID).ToHashSet();
        var allForms = modelFacade.SelectAll<ICmsForm>(" ORDER BY FormID").ToList();
        var selectedForms = allForms.Where(form => migratedSiteIds.Contains(form.FormSiteID)).ToList();
        var selectedFormsByClass = selectedForms.GroupBy(form => form.FormClassID).ToDictionary(group => group.Key, group => group.ToList());
        var cmsClassForms = modelFacade.Select<ICmsClass>("ClassIsForm = 1", "ClassID");
        foreach (var ksClass in cmsClassForms)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!selectedFormsByClass.TryGetValue(ksClass.ClassID, out var cmsForms))
            {
                continue;
            }

            protocol.FetchedSource(ksClass);

            var kxoDataClass = kxpClassFacade.GetClass(ksClass.ClassGUID);
            protocol.FetchedTarget(kxoDataClass);

            bool classSuccessFullySaved = MapAndSaveUsingKxoApi(ksClass, kxoDataClass);
            if (!classSuccessFullySaved)
            {
                continue;
            }

            foreach (var ksCmsForm in cmsForms)
            {
                cancellationToken.ThrowIfCancellationRequested();
                protocol.FetchedSource(ksCmsForm);

                var kxoCmsForm = BizFormInfo.Provider.Get(ksCmsForm.FormGUID);

                protocol.FetchedTarget(kxoCmsForm);

                var mapped = cmsFormMapper.Map(ksCmsForm, kxoCmsForm);
                protocol.MappedTarget(mapped);

                if (mapped is { Success: true } result)
                {
                    (var cmsForm, bool newInstance) = result;
                    ArgumentNullException.ThrowIfNull(cmsForm, nameof(cmsForm));


                    try
                    {
                        int formItems = cmsForm.FormItems;
                        BizFormInfo.Provider.Set(cmsForm!);

                        // Update FormItems manually, as BizFormInfo provider always resets the value to 0
                        using (var conn = ConnectionHelper.GetConnection())
                        {
                            conn.DataConnection.ExecuteNonQuery("UPDATE [CMS_Form] SET FormItems=@formItems WHERE FormID=@formID", new QueryDataParameters() { { "formItems", formItems }, { "formID", cmsForm.FormID } }, QueryTypeEnum.SQLQuery, true);
                        }

                        logger.LogEntitySetAction(newInstance, cmsForm);

                        primaryKeyMappingContext.SetMapping<BizFormInfo>(
                            r => r.FormID,
                            ksClass.ClassID,
                            cmsForm.FormID
                        );
                    }
                    catch (Exception ex)
                    {
                        protocol.Append(HandbookReferences
                            .ErrorCreatingTargetInstance<BizFormInfo>(ex)
                            .NeedsManualAction()
                            .WithIdentityPrint(cmsForm)
                        );
                        logger.LogEntitySetError(ex, newInstance, cmsForm);

                        continue;
                    }
                }

            }

            if (ksClass.ClassTableName is null)
            {
                logger.LogWarning("Form class '{ClassName}' has no coupled table; skipping its form data", ksClass.ClassName);
                continue;
            }

            XNamespace nsSchema = "http://www.w3.org/2001/XMLSchema";
            XNamespace msSchema = "urn:schemas-microsoft-com:xml-msdata";
            var xDoc = XDocument.Parse(ksClass.ClassXmlSchema);
            var autoIncrementColumns = xDoc.Descendants(nsSchema + "element")
                .Where(x => x.Attribute(msSchema + "AutoIncrement")?.Value == "true")
                .Select(x => x.Attribute("name")?.Value).ToImmutableHashSet();

            if (autoIncrementColumns.Count != 1)
            {
                logger.LogWarning("Form class '{ClassName}' has {AutoIncrementColumnCount} auto-increment columns; skipping its form data", ksClass.ClassName, autoIncrementColumns.Count);
                continue;
            }

            if (bulkDataCopyService.CheckIfDataExistsInTargetTable(ksClass.ClassTableName))
            {
                logger.LogWarning("Data exists in target coupled data table '{TableName}' - cannot migrate, skipping form data migration", ksClass.ClassTableName);
                protocol.Append(HandbookReferences.DataMustNotExistInTargetInstanceTable(ksClass.ClassTableName));
                continue;
            }

            bool classUsedByUnselectedSites = allForms.Any(form =>
                form.FormClassID == ksClass.ClassID && !migratedSiteIds.Contains(form.FormSiteID));
            bool tableHasSiteColumn = bulkDataCopyService.GetSqlTableColumns(ksClass.ClassTableName, configuration.KxConnectionString)
                .Any(column => column.ColumnName.Equals("FormSiteID", StringComparison.OrdinalIgnoreCase));
            if (classUsedByUnselectedSites && !tableHasSiteColumn)
            {
                logger.LogWarning("Form class '{ClassName}' is shared by selected and unselected sites, but table '{TableName}' has no FormSiteID column; skipping its shared submissions to avoid cross-site migration", ksClass.ClassName, ksClass.ClassTableName);
                continue;
            }

            Func<System.Data.IDataReader, bool> dataFilter = tableHasSiteColumn
                ? reader => !reader.IsDBNull(reader.GetOrdinal("FormSiteID")) && migratedSiteIds.Contains(reader.GetInt32(reader.GetOrdinal("FormSiteID")))
                : _ => true;
            var bulkCopyRequest = new BulkCopyRequest(
                ksClass.ClassTableName, column => !autoIncrementColumns.Contains(column), dataFilter, 20000);

            logger.LogInformation("Copying submissions for form class '{ClassName}' associated with {SelectedSiteCount} selected site(s)", ksClass.ClassName, migratedSiteIds.Count);
            bulkDataCopyService.CopyTableToTable(bulkCopyRequest);
        }

        await GlobalizeBizFormFiles();

        return new GenericCommandResult();
    }

    private bool MapAndSaveUsingKxoApi(ICmsClass ksClass, DataClassInfo kxoDataClass)
    {
        var mapped = dataClassMapper.Map(ksClass, kxoDataClass);
        protocol.MappedTarget(mapped);

        if (mapped is { Success: true })
        {
            (var dataClassInfo, bool newInstance) = mapped;
            ArgumentNullException.ThrowIfNull(dataClassInfo, nameof(dataClassInfo));

            try
            {
                kxpClassFacade.SetClass(dataClassInfo);

                protocol.Success(ksClass, dataClassInfo, mapped);
                logger.LogEntitySetAction(newInstance, dataClassInfo);

                primaryKeyMappingContext.SetMapping<DataClassInfo>(
                    r => r.ClassID,
                    ksClass.ClassID,
                    dataClassInfo.ClassID
                );

                return true;
            }
            catch (Exception ex)
            {
                protocol.Append(HandbookReferences
                    .ErrorCreatingTargetInstance<DataClassInfo>(ex)
                    .NeedsManualAction()
                    .WithIdentityPrint(dataClassInfo)
                );
                logger.LogEntitySetError(ex, newInstance, dataClassInfo);
            }
        }

        return false;
    }

    #region Directory globalization

    private Task GlobalizeBizFormFiles()
    {
        foreach (var cmsSite in modelFacade.GetMigratedSites())
        {
            string globalBizformFiles = CMS.IO.Path.Combine(SystemContext.WebApplicationPhysicalPath, "BizFormFiles");
            string siteBizFormFiles = CMS.IO.Path.Combine(configuration.KxCmsDirPath, cmsSite.SiteName, "bizformfiles");
            if (CMS.IO.Directory.Exists(siteBizFormFiles))
            {
                Debug.WriteLine($"Copying site bizformfiles from '{siteBizFormFiles}' to global bizformfiles '{globalBizformFiles}'");
                try
                {
                    var source = CMS.IO.DirectoryInfo.New(siteBizFormFiles);
                    var target = CMS.IO.DirectoryInfo.New(globalBizformFiles);
                    CopyAll(source, target);
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Moving site bizformfiles failed with {ex}");
                }
            }
            else
            {
                Debug.WriteLine($"Directory '{siteBizFormFiles}' not exists");
            }
        }
        return Task.CompletedTask;
    }

    internal static void CopyAll(CMS.IO.DirectoryInfo source, CMS.IO.DirectoryInfo target)
    {
        var stack = new Stack<(CMS.IO.DirectoryInfo source, CMS.IO.DirectoryInfo target)>();
        stack.Push((source, target));

        while (stack.Count > 0)
        {
            var (s, t) = stack.Pop();
            if (string.Equals(s.FullName, t.FullName, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            // Check if the target directory exists, if not, create it.
            if (!CMS.IO.Directory.Exists(t.FullName))
            {
                CMS.IO.Directory.CreateDirectory(t.FullName);
            }

            // Copy each file into it's new directory.
            foreach (var fi in s.GetFiles())
            {
                Debug.WriteLine($@"Moving {t.FullName}\{fi.Name}");
                fi.CopyTo(CMS.IO.Path.Combine(t.FullName, fi.Name), true);
                // fi.Delete();
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in s.GetDirectories())
            {
                var nextTargetSubDir = t.CreateSubdirectory(diSourceSubDir.Name);
                stack.Push((diSourceSubDir, nextTargetSubDir));
            }
        }

        // missing overload in source.Delete(true);, replaced with:
        // CMS.IO.Directory.Delete(source.FullName, true);
    }

    #endregion
}
