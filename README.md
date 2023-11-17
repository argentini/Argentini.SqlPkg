# SqlPkg for Microsoft SqlPackage

SqlPkg is a 64-bit .NET 8.0 command line (CLI) wrapper for the Microsoft SqlPackage CLI tool with the goal of making common backup and restore operations easier and more powerful. It does this through new `Backup` and `Restore` actions that provide additional features like the exclusion of specific table data in backups and destination prep prior to restore.

## Features

The following SqlPackage action modes provide enhanced features.

### /Action:Backup

This mode is equivalent to `Action:Export` to create a `.bacpac` file, with the following differences.

- Specify one or more `/p:ExcludeTableData=` properties to exclude specific table data from the bacpac file. The table name format is the same as the `/p:TableData=` property.
- `/SourceTrustServerCertificate:` defaults to `true`.
- `/SourceTimeout:` defaults to `30`.
- `/CommandTimeout:` defaults to `120`.
- `/p:VerifyExtraction=` defaults to `false`.
- Destination file paths will be created if they do not exist.

Here's a *Backup* example for Bash:

```bash
sqlpkg /Action:Backup /TargetFile:'Backups/Local/MyBackup.bacpac' /SourceServerName:'mydatabase.net,1433' /SourceDatabaseName:'MyDatabase' /SourceUser:'sa' /SourcePassword:'MyP@ssw0rd' /p:ExcludeTableData='[dbo].[Log]' /p:ExcludeTableData='[dbo].[IpAddresses]'
```

Here's a *Backup* example for PowerShell:

```powershell
sqlpkg /Action:Backup /TargetFile:"Backups/Local/MyBackup.bacpac" /SourceServerName:"mydatabase.net,1433" /SourceDatabaseName:MyDatabase /SourceUser:sa /SourcePassword:MyP@ssw0rd /p:ExcludeTableData=[dbo].[Log] /p:ExcludeTableData=[dbo].[IpAddresses]
```

### /Action:Restore

This mode is equivalent to `Action:Import` to restore a `.bacpac` file, with the following differences.

- The destination database will be purged of all user objects (tables, views, etc.) before the restoration.
- If the destination database doesn't exist it will be created.
- `/TargetTrustServerCertificate:` defaults to `true`.
- `/TargetTimeout:` defaults to `30`.
- `/CommandTimeout:` defaults to `120`.
- Destination file paths will be created if they do not exist.

Here's a *Restore* example for Bash:

```bash
sqlpkg /Action:Restore /SourceFile:'Backups/Local/MyBackup.bacpac' /TargetServerName:'mydatabase.net,1433' /TargetDatabaseName:'MyDatabase' /TargetUser:'sa' /TargetPassword:'MyP@ssw0rd'
```

Here's a *Restore* example for PowerShell:

```powershell
sqlpkg /Action:Restore /SourceFile:"Backups/Local/MyBackup.bacpac" /TargetServerName:"mydatabase.net,1433" /TargetDatabaseName:MyDatabase /TargetUser:sa /TargetPassword:MyP@ssw0rd
```

### /Action:Backup-All

This mode will back up all user databases on a server.

- Provide a source connection to the master database.
- Provide a target file path ending with 'master.bacpac'. The path will be used as the destination for each database backup file, ignoring 'master.bacpac'.
- Optionally provide a log file path ending with 'master.log'. The path will be used as the destination for each database backup log file, ignoring 'master.log'.
- Accepts all arguments that the Backup action mode accepts.

Here's a *Backup-All* example for Bash:

```bash
sqlpkg /Action:Backup-All /TargetFile:'Backups/Local/master.bacpac' /SourceServerName:'mydatabase.net,1433' /SourceDatabaseName:'master' /SourceUser:'sa' /SourcePassword:'MyP@ssw0rd' /p:ExcludeTableData='[dbo].[Log]' /p:ExcludeTableData='[dbo].[IpAddresses]'
```

Here's a *Backup-All* example for PowerShell:

```powershell
sqlpkg /Action:Backup-All /TargetFile:"Backups/Local/master.bacpac" /SourceServerName:"mydatabase.net,1433" /SourceDatabaseName:master /SourceUser:sa /SourcePassword:MyP@ssw0rd /p:ExcludeTableData=[dbo].[Log] /p:ExcludeTableData=[dbo].[IpAddresses]
```

### /Action:Restore-All

This mode will restore all \*.bacpac files in a given path to databases with the same names as the filenames.

- Provide a source file path to 'master.bacpac' in the location of the bacpac files. The path will be used as the source location for each database backup file to restore, ignoring 'master.bacpac'.
- Provide a target connection to the master database.
- Optionally provide a log file path ending with 'master.log'. The path will be used as the destination for each database backup log file, ignoring 'master.log'.
- Accepts all arguments that the Restore action mode accepts.

Here's a *Restore-All* example for Bash:

```bash
sqlpkg /Action:Restore-All /SourceFile:'Backups/Local/master.bacpac' /TargetServerName:'mydatabase.net,1433' /TargetDatabaseName:'master' /TargetUser:'sa' /TargetPassword:'MyP@ssw0rd'
```

Here's a *Restore-All* example for PowerShell:

```powershell
sqlpkg /Action:Restore-All /SourceFile:"Backups/Local/master.bacpac" /TargetServerName:"mydatabase.net,1433" /TargetDatabaseName:master /TargetUser:sa /TargetPassword:MyP@ssw0rd
```

## Additional Usage

When not using SqlPkg special action modes, the entire argument list is simply piped to SqlPackage and will run normally. So you can use `sqlpkg` everywhere `SqlPackage` is used.

## Installation

### 1. Install Microsoft .NET

SqlPkg requires that you already have the .NET 8.0 runtime installed, which you can get at [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download).

**Note:** Microsoft SqlPackage uses .NET 6, so you need to install that in addition to .NET 8.

### 2. Install SqlPkg

Run the following command in your command line interface (e.g. cmd, PowerShell, Terminal, bash, etc.)

```dotnet tool install --global argentini.sqlpkg```

Note: this process will also install Microsoft SqlPackage.

### Update SqlPkg

Run the following command in your command line interface (e.g. cmd, PowerShell, Terminal, bash, etc.) to update to the latest version of SqlPkg:

```dotnet tool update --global argentini.sqlpkg```

## Project Status

This application is under active development so check back for updates.
