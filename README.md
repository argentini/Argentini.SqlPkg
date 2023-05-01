# SqlPkg for Microsoft SqlPackage

SqlPkg is a 64-bit .NET 7.0 command line (CLI) wrapper for the Microsoft SqlPackage CLI tool, providing additional features, like the exclusion of specific table data and destination purging prior to import.

## Features

The following SqlPackage action modes provide enhanced features.

### /Action:Backup

This mode is equivalent to `Action:Export` to create a `.bacpac` file, with the following differences.

- Specify one or more `/p:ExcludeTableData=` properties to exclude sepcific table data from the bacpac file. The table name format is the same as the `/p:TableData=` property.
- `/SourceTrustServerCertificate:` defaults to `true`.
- `/SourceTimeout:` defaults to `30`.
- `/CommandTimeout:` defaults to `120`.
- `/p:VerifyExtraction=` defaults to `false`.
- Destination file paths will be created if they do not exist.

### /Action:Restore

This mode is equivalent to `Action:Import` to restore a `.bacpac` file, with the following differences.

- The destination database will be purged of all user objects (tables, views, etc.) before the restoration.
- If the destination database doesn't exist it will be created.
- `/TargetTrustServerCertificate:` defaults to `true`.
- `/TargetTimeout:` defaults to `30`.
- `/CommandTimeout:` defaults to `120`.
- Destination file paths will be created if they do not exist.

### Other Actions

When not using Backup or Restore modes, the entire argument list is simply piped to SqlPackage and will run normally. So you can use `sqlpkg` everywhere SqlPackage is used.

## Usage

You use SqlPkg as you would use the Microsoft SqlPackage CLI application. All arguments are passed to SqlPackage as-is, but some default values have been changed to make using it easier, like defaulting to ignore permissions and to accept server certificates.

Here's a backup example for Bash:

```bash
sqlpkg /Action:Backup /TargetFile:'Backups/Local/MyBackup.bacpac' /SourceServerName:'mydatabase.net,1433' /SourceDatabaseName:'MyDatabase' /SourceUser:'sa' /SourcePassword:'MyP@ssw0rd' /p:ExcludeTableData='[dbo].[Log]' /p:ExcludeTableData='[dbo].[IpAddresses]'
```

Here's a backup example for PowerShell:

```powershell
sqlpkg /Action:Backup /TargetFile:"Backups/Local/MyBackup.bacpac" /SourceServerName:"mydatabase.net,1433" /SourceDatabaseName:MyDatabase /SourceUser:sa /SourcePassword:MyP@ssw0rd /p:ExcludeTableData=[dbo].[Log] /p:ExcludeTableData=[dbo].[IpAddresses]
```

Here's a restore example for Bash:

```bash
sqlpkg /Action:Restore /SourceFile:'Backups/Local/MyBackup.bacpac' /TargetServerName:'mydatabase.net,1433' /TargetDatabaseName:'MyDatabase' /TargetUser:'sa' /TargetPassword:'MyP@ssw0rd'
```

Here's a restore example for PowerShell:

```bash
sqlpkg /Action:Restore /SourceFile:"Backups/Local/MyBackup.bacpac" /TargetServerName:"mydatabase.net,1433" /TargetDatabaseName:MyDatabase /TargetUser:sa /TargetPassword:MyP@ssw0rd
```

## Installation

### 1. Install Microsoft SqlPackage

According to the SqlPackage [website](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage), the recommended way to install SqlPackage is as a dotnet tool.

```
dotnet tool install -g microsoft.sqlpackage
```

This requires that you already have the Microsoft dotnet CLI tool installed, which you can get at [https://dotnet.microsoft.com](https://dotnet.microsoft.com/).

### 2. Install SqlPkg

Download the latest release and follow the instructions in the release for your operating system and platform.

## Project Status

This application is under active development so check back for updates.
