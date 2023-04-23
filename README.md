# SqlPkg for Microsoft SqlPackage

SqlPkg is a 64-bit .NET 7.0 command line (CLI) wrapper for the Microsoft SqlPackage CLI tool, providing additional features, like the exclusion of specific database objects or table data.

## Install Microsoft SqlPackage

According to the SqlPackage [website](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage), the recommended way to install SqlPackage is as a dotnet tool.

```
dotnet tool install -g microsoft.sqlpackage
```

This requires that you already have the Microsoft dotnet CLI tool installed, which you can get at [https://dotnet.microsoft.com](https://dotnet.microsoft.com/).

## Installation SqlPkg

TBD

## Usage

You use SqlPkg as you would use the Microsoft SqlPackage CLI application. All arguments are passed to SqlPackage as-is, but some default values have been changed to make using it easier, like defaulting to ignore permissions and to accept server certificates.

Here's an example:

```
SqlPackage /Action:Extract /TargetFile:MyDatabaseBackup.dacpac /DiagnosticsFile:MyDatabaseBackup.log /p:ExtractAllTableData=false /p:VerifyExtraction=true /SourceServerName:mydatabase.net,1433 /SourceDatabaseName:MyDatabase /SourceUser:sa /SourcePassword:MyP@ssw0rd /p:ExcludeTableData=[dbo].[Log]
```

There are additional features as well, as are listed below.

## Status

This application is under active development so check back for updates.

## Features

The following SqlPackage action modes have new features when using SqlPkg.

### Action:Extract

You can specify a `/p:ExcludeTableData=` property for each table to exclude its data from the dacpac file. The table name format is the same as the `/p:TableData=` property. To exclude table data you must also use `/p:TableData=` properties to include specific tables, or `/p:ExtractAllTableData=true` to include all table data, otherwise there is nothing to exclude.

### Action:Export

You can specify a `/p:ExcludeTableData=` property for each table to exclude its data from the dacpac file. See *Action:Extract* for more details.
