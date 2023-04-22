# SqlPkg for Microsoft SqlPackage

SqlPkg is a 64-bit .NET 7.0 command line (CLI) wrapper for the Microsoft SqlPackage CLI tool, providing additional features, like the exclusion of specific database objects or table data.

## Install Microsoft SqlPackage

According to the SqlPackage [website](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage), the recommended way to install SqlPackage is as a dotnet tool.

```dotnet tool install -g microsoft.sqlpackage
```

This requires that you already have the Microsoft dotnet CLI tool installed, which you can get at [https://dotnet.microsoft.com](https://dotnet.microsoft.com/).
