namespace Argentini.SqlPkg;

public class Settings
{
    public string Action { get; set; } = string.Empty;

    public string SourceConnectionString { get; set; } = string.Empty;
    public string SourceServerName { get; set; } = string.Empty;
    public string SourceDatabaseName { get; set; } = string.Empty;
    public string SourceUserName { get; set; } = string.Empty;
    public string SourcePassword { get; set; } = string.Empty;

    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetServerName { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetPassword { get; set; } = string.Empty;
}