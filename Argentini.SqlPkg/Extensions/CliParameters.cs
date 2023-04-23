namespace Argentini.SqlPkg.Extensions;

public static class CliParameters
{
    public static string GetArgumentValue(this IEnumerable<string>? arguments, string startsWith, string startsWithAbbrev, char delimiter)
    {
        if (arguments == null)
            return string.Empty;

        var args = arguments.ToList();

        if (args.Any() == false)
            return string.Empty;
        
        if (args.Any(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)))
        {
            var splits = args.First(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                        
            if (splits.Length == 2)
            {
                return splits[1];
            }
        }

        return string.Empty;
    }

    public static string[] SetDefault(this string[] args, string argumentPrefix, string appendValue)
    {
        if (args.Any(a => a.StartsWith(argumentPrefix, StringComparison.CurrentCultureIgnoreCase)))
            return args;
        
        var tempArgs = args.ToList();
            
        tempArgs.Add($"{argumentPrefix}{appendValue}");
            
        return tempArgs.ToArray();
    }
}