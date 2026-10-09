using System.Collections.Frozen;
using System.Collections.Immutable;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using Migration.Tool.Common;
using Migration.Tool.Common.Helpers;
using Migration.Tool.Common.Services;
using Migration.Tool.Source.Model;

namespace Migration.Tool.Source.Services;

/// <summary>
///     in cases where consumer exported site with documents and created new site with that particular export, GUID
///     conflicts will probably happen. This class aims to query and solve those conflicts
/// </summary>
/// <param name="modelFacade"></param>
/// <param name="logger"></param>
public class SpoiledGuidContext(ModelFacade modelFacade, ILogger<SpoiledGuidContext> logger) : ISpoiledGuidContext
{
    private IDictionary<Guid, ImmutableList<SpoiledDocumentGuidInfo>>? spoiledDocumentGuids;
    private IDictionary<Guid, ImmutableList<SpoiledNodeGuidInfo>>? spoiledNodeGuids;

    internal IDictionary<Guid, ImmutableList<SpoiledDocumentGuidInfo>> SpoiledDocumentGuids =>
        spoiledDocumentGuids ??= modelFacade.Select("""
                                                     WITH DuplicateDocumentGuids AS (
                                                         SELECT DocumentGUID
                                                         FROM CMS_Document
                                                         GROUP BY DocumentGUID
                                                         HAVING COUNT(DocumentID) > 1
                                                     )
                                                     SELECT document.DocumentGUID, tree.NodeSiteID, document.DocumentNodeID AS NodeID
                                                     FROM CMS_Document AS document
                                                     INNER JOIN DuplicateDocumentGuids AS duplicate ON duplicate.DocumentGUID = document.DocumentGUID
                                                     INNER JOIN CMS_Tree AS tree ON tree.NodeID = document.DocumentNodeID
                                                     """,
                (reader, version) => new
                {
                    DocumentGUID = reader.Unbox<Guid>("DocumentGUID"),
                    NodeID = reader.Unbox<int>("NodeID"),
                    NodeSiteID = reader.Unbox<int>("NodeSiteID"),
                })
            .GroupBy(x => x.DocumentGUID)
            .ToFrozenDictionary(
                x => x.Key,
                x => x.Select(i => new SpoiledDocumentGuidInfo(i.NodeSiteID, i.NodeID)).ToImmutableList()
            );

    internal IDictionary<Guid, ImmutableList<SpoiledNodeGuidInfo>> SpoiledNodeGuids =>
        spoiledNodeGuids ??= modelFacade.Select("""
                                                 WITH DuplicateNodeGuids AS (
                                                     SELECT NodeGUID
                                                     FROM CMS_Tree
                                                     GROUP BY NodeGUID
                                                     HAVING COUNT(NodeGUID) > 1
                                                 )
                                                 SELECT tree.NodeGUID, tree.NodeSiteID
                                                 FROM CMS_Tree AS tree
                                                 INNER JOIN DuplicateNodeGuids AS duplicate ON duplicate.NodeGUID = tree.NodeGUID
                                                 """,
                (reader, version) => new
                {
                    NodeGUID = reader.Unbox<Guid>("NodeGUID"),
                    NodeSiteID = reader.Unbox<int>("NodeSiteID")
                })
            .GroupBy(x => x.NodeGUID)
            .ToFrozenDictionary(
                x => x.Key,
                x => x.Select(i => new SpoiledNodeGuidInfo(i.NodeSiteID)).ToImmutableList()
            );

    public Guid EnsureDocumentGuid(Guid documentGuid, int siteId, int nodeId, int documentId)
    {
        if (!SpoiledDocumentGuids.TryGetValue(documentGuid, out _))
        {
            return documentGuid;
        }

        var newGuid = GuidHelper.CreateDocumentGuid($"{documentId}|{nodeId}|{siteId}");
        logger.LogTrace("Spoiled document guid encountered '{OriginalGuid}', replaced by {NewGuid} {Details}", documentGuid, newGuid, new { siteId, nodeId, documentId });
        return newGuid;
    }

    public Guid EnsureNodeGuid(Guid nodeGuid, int siteId, int nodeId)
    {
        if (!SpoiledNodeGuids.TryGetValue(nodeGuid, out _))
        {
            return nodeGuid;
        }

        var newGuid = GuidHelper.CreateNodeGuid($"{nodeId}|{siteId}");
        logger.LogTrace("Spoiled node guid encountered '{OriginalGuid}', replaced by {NewGuid} {Details}", nodeGuid, newGuid, new { siteId, nodeId });
        return newGuid;
    }

    public Guid EnsureNodeGuid(Guid nodeGuid, int siteId)
    {
        if (!SpoiledNodeGuids.TryGetValue(nodeGuid, out _))
        {
            return nodeGuid;
        }

        int nodeId = modelFacade.Select("""
                                        SELECT NodeID FROM CMS_Tree WHERE NodeSiteID = @siteId AND NodeGUID = @nodeGuid
                                        """, (reader, version) => reader.Unbox<int>("NodeID"),
            new SqlParameter("siteId", siteId), new SqlParameter("nodeGuid", nodeGuid)
        ).Single();

        var newGuid = GuidHelper.CreateNodeGuid($"{nodeId}|{siteId}");
        logger.LogTrace("Spoiled node guid encountered '{OriginalGuid}', replaced by {NewGuid} {Details}", nodeGuid, newGuid, new { siteId, nodeId });
        return newGuid;
    }

    public Guid? GetNodeGuid(int nodeId, int siteId)
    {
        var nodeGuid = modelFacade.Select("""
                                          SELECT NodeGUID FROM CMS_Tree WHERE NodeSiteID = @siteId AND NodeId = @nodeId
                                          """, (reader, version) => reader.Unbox<Guid?>("NodeGUID"),
            new SqlParameter("siteId", siteId), new SqlParameter("nodeId", nodeId)
        ).FirstOrDefault();

        if (nodeGuid is not { } sNodeGuid)
        {
            return null;
        }

        if (!SpoiledNodeGuids.TryGetValue(sNodeGuid, out _))
        {
            return sNodeGuid;
        }

        var newGuid = GuidHelper.CreateNodeGuid($"{nodeId}|{siteId}");
        logger.LogTrace("Spoiled node guid encountered '{OriginalGuid}', replaced by {NewGuid} {Details}", nodeGuid, newGuid, new { siteId, nodeId });
        return newGuid;
    }

    public Guid EnsureNodeGuid(ICmsTree node) => EnsureNodeGuid(node.NodeGUID, node.NodeSiteID, node.NodeID);

    internal record SpoiledDocumentGuidInfo(int SiteId, int NodeId);

    internal record SpoiledNodeGuidInfo(int SiteId);
}
