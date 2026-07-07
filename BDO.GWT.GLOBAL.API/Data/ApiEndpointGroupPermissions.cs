namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Defines API endpoint group permissions for access control.
/// </summary>
public class ApiEndpointGroupPermissions
{
    /// <summary>
    /// Gets or sets a value indicating whether read permission is granted.
    /// </summary>
    public bool Read { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manage permission is granted.
    /// </summary>
    public bool Manage { get; set; }
}
