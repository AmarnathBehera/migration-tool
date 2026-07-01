namespace BDO.GWT.Domain.Core.Enums;

public enum UserPrivilegeLevel
{
    None = 0,
    /// <summary>
    /// Represents a user with basic privileges, typically having limited access to certain features or functionalities within the application. This level is often assigned to regular users who can view and interact with content but may not have the ability to modify or manage it.
    /// </summary>
    Editor = 1,
    Administrator = 2,
    /// <summary>
    /// Represents a user with the highest level of privileges, typically having full access to all features and settings within the application. This level is usually reserved for system administrators or superusers who can manage other users and perform critical system operations.
    /// </summary>
    GlobalAdministrator = 3,
}
