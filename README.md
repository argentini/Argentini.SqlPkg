# SqlPkg for Microsoft SqlPackage

SqlPkg is a 64-bit .NET 7.0 command line (CLI) wrapper for the Microsoft SqlPackage CLI tool with the goal of making common backup and restore operations easier and more powerful. It does this through new `Backup` and `Restore` actions that provide additional features like the exclusion of specific table data in backups and destination prep prior to restore.

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

## Usage

You can use SqlPkg as you would use the Microsoft SqlPackage CLI application. When not using *Backup* or *Restore* modes, the entire argument list is simply piped to SqlPackage and will run normally. So you can use `sqlpkg` everywhere SqlPackage is used.

Here's a *Backup* example for Bash:

```bash
sqlpkg /Action:Backup /TargetFile:'Backups/Local/MyBackup.bacpac' /SourceServerName:'mydatabase.net,1433' /SourceDatabaseName:'MyDatabase' /SourceUser:'sa' /SourcePassword:'MyP@ssw0rd' /p:ExcludeTableData='[dbo].[Log]' /p:ExcludeTableData='[dbo].[IpAddresses]'
```

Here's a *Backup* example for PowerShell:

```powershell
sqlpkg /Action:Backup /TargetFile:"Backups/Local/MyBackup.bacpac" /SourceServerName:"mydatabase.net,1433" /SourceDatabaseName:MyDatabase /SourceUser:sa /SourcePassword:MyP@ssw0rd /p:ExcludeTableData=[dbo].[Log] /p:ExcludeTableData=[dbo].[IpAddresses]
```

Here's a *Restore* example for Bash:

```bash
sqlpkg /Action:Restore /SourceFile:'Backups/Local/MyBackup.bacpac' /TargetServerName:'mydatabase.net,1433' /TargetDatabaseName:'MyDatabase' /TargetUser:'sa' /TargetPassword:'MyP@ssw0rd'
```

Here's a *Restore* example for PowerShell:

```powershell
sqlpkg /Action:Restore /SourceFile:"Backups/Local/MyBackup.bacpac" /TargetServerName:"mydatabase.net,1433" /TargetDatabaseName:MyDatabase /TargetUser:sa /TargetPassword:MyP@ssw0rd
```

## Installation

### 1. Install Microsoft SqlPackage

According to the SqlPackage [website](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage), the recommended way to install SqlPackage is as a dotnet tool.

```
dotnet tool install -g microsoft.sqlpackage
```

This requires that you already have dotnet installed, which you can get at [https://dotnet.microsoft.com](https://dotnet.microsoft.com/).

### 2. Install SqlPkg

Download the [latest release](https://github.com/argentini/Argentini.SqlPkg/releases) (zip file) for your operating system and CPU architecture and follow the instructions in the README file.

## Project Status

This application is under active development so check back for updates.
