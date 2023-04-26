namespace Argentini.SqlPkg;

public class Settings
{
    public string Action { get; set; } = string.Empty;

    public string SourceConnectionString { get; set; } = string.Empty;
    public string SourceServerName { get; set; } = string.Empty;
    public string SourceDatabaseName { get; set; } = string.Empty;
    public string SourceUserName { get; set; } = string.Empty;
    public string SourcePassword { get; set; } = string.Empty;
    public int SourceConnectionTimeout { get; set; } = 30;
    public int SourceCommandTimeout { get; set; } = 120;
    public bool SourceTrustServerCertificate { get; set; } = true;

    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetServerName { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetPassword { get; set; } = string.Empty;
    public int TargetConnectionTimeout { get; set; } = 30;
    public int TargetCommandTimeout { get; set; } = 120;
    public bool TargetTrustServerCertificate { get; set; } = true;
}