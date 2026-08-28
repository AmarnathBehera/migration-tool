namespace BDO.GWT.GLOBAL.API.Data
{
    public interface IPageChannelResolver
    {
        Task<string?> GetChannelNameByPageGuidAsync(Guid pageGuid, CancellationToken cancellationToken = default);
    }

}
