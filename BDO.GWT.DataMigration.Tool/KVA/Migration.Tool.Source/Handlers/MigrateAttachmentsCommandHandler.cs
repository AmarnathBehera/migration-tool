using MediatR;

using Migration.Tool.Common;
using Migration.Tool.Common.Abstractions;
using Migration.Tool.Source.Model;
using Migration.Tool.Source.Services;

namespace Migration.Tool.Source.Handlers;

// ReSharper disable once UnusedMember.Global [implicit use]
public class MigrateAttachmentsCommandHandler(
    ModelFacade modelFacade,
    IAttachmentMigrator attachmentMigrator,
    ToolConfiguration toolConfiguration
) : IRequestHandler<MigrateAttachmentsCommand, CommandResult>
{
    public async Task<CommandResult> Handle(MigrateAttachmentsCommand request, CancellationToken cancellationToken)
    {
        var targetSiteNames = toolConfiguration.TargetWebsiteChannelMappings.Keys;
        var targetSiteIds = targetSiteNames.Count == 0
            ? null
            : modelFacade.SelectAll<ICmsSite>()
                .Where(site => targetSiteNames.Contains(site.SiteName, StringComparer.OrdinalIgnoreCase))
                .Select(site => site.SiteID)
                .ToHashSet();

        if (targetSiteIds is { Count: 0 })
        {
            throw new InvalidOperationException("No source sites matched TargetWebsiteChannelMappings; attachments cannot be scoped safely.");
        }

        var ksCmsAttachments = modelFacade.SelectAll<ICmsAttachment>();

        foreach (var ksCmsAttachment in ksCmsAttachments)
        {
            if (targetSiteIds is not null && !targetSiteIds.Contains(ksCmsAttachment.AttachmentSiteID))
            {
                continue;
            }

            if (ksCmsAttachment.AttachmentIsUnsorted != true || ksCmsAttachment.AttachmentGroupGUID != null)
            {
                // those must be migrated with pages
                continue;
            }

            await attachmentMigrator.MigrateAttachment(ksCmsAttachment);
        }

        return new GenericCommandResult();
    }
}
