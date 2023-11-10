namespace PugSharp.Api.G5Api;

public enum PauseType
{
    None,      // Not paused
    Tech,      // Technical pause
    Tactical,  // Tactical Pause
    Admin,     // Admin/RCON Pause
    Backup     // Special type for match pausing during backups.
};