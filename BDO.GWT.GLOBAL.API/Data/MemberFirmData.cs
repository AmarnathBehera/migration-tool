namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Represents the data structure for a member firm as retrieved from the Master Data API. This class contains properties that correspond to the fields returned by the API for a member firm, including its unique identifier, name, and website.
/// </summary>
public class MemberFirmData
{
    /// <summary>
    /// Gets or sets the unique identifier for the member firm. This ID is used to identify the firm in the Master Data API.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the member firm. This is the official name of the firm as registered in the Master Data API.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of the member firm. This indicates whether the firm is a 'TestFirm' or a regular 'Firm'.
    /// </summary>
    public string Website { get; set; } = null!;
}
