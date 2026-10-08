using MediatR;

using Migration.Tool.Common;
using Migration.Tool.Common.Abstractions;
using Migration.Tool.Source.Model;
using Migration.Tool.Source.Services;

namespace Migration.Tool.Source.Handlers;

// ReSharper disable once UnusedMember.Global [implicit use]
public class MigrateAttachmentsCommandHandler(
    ModelFacade modelFacade,
    IAttachmentMigrator attachmentMigrator
) : IRequestHandler<MigrateAttachmentsCommand, CommandResult>
{
    public async Task<CommandResult> Handle(MigrateAttachmentsCommand request, CancellationToken cancellationToken)
    {
        var migratedSiteIds = modelFacade.GetMigratedSites().Select(site => site.SiteID).ToHashSet();
        var ksCmsAttachments = modelFacade.SelectWhere<ICmsAttachment>("AttachmentIsUnsorted = 1 AND AttachmentGroupGUID IS NULL")
            .Where(attachment => migratedSiteIds.Contains(attachment.AttachmentSiteID));

        foreach (var ksCmsAttachment in ksCmsAttachments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await attachmentMigrator.MigrateAttachment(ksCmsAttachment);
        }

        return new GenericCommandResult();
    }
}
