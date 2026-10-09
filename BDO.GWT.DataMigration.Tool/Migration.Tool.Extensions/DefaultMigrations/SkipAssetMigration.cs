using System.Xml.Linq;
using Migration.Tool.KXP.Api.Auxiliary;
using Migration.Tool.Common;
using Migration.Tool.Common.Enumerations;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Migration.Tool.Source.Contexts;
using Migration.Tool.Source.Mappers;
using Migration.Tool.Source.Model;

namespace Migration.Tool.Extensions.DefaultMigrations;

public sealed class SkipAssetMigration : IFieldMigration
{
    public int Rank => 100_000;

    public void MigrateFieldDefinition(FormDefinitionPatcher formDefinitionPatcher, XElement field, XAttribute? columnTypeAttr, string fieldDescriptor)
    {
    }

    public bool ShallMigrate(FieldMigrationContext context) =>
        (
            context.SourceDataType is KsFieldDataType.DocAttachments or KsFieldDataType.File ||
            Kx13FormControls.UserControlForText.MediaSelectionControl.Equals(context.SourceFormControl, StringComparison.InvariantCultureIgnoreCase)
        ) &&
        context.SourceObjectContext
            is DocumentSourceObjectContext or CustomTableSourceObjectContext or EmptySourceObjectContext;

    public Task<FieldMigrationResult> MigrateValue(object? sourceValue, FieldMigrationContext context) =>
        Task.FromResult(new FieldMigrationResult(true, null));
}
