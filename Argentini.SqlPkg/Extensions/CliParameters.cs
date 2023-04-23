namespace Argentini.SqlPkg.Extensions;

public static class CliParameters
{
    /// <summary>
    /// Get a CLI argument value, or an emtpy string if not found.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="startsWith"></param>
    /// <param name="startsWithAbbrev"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Set a default CLI argument value if it doesn't exist.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="argumentPrefix"></param>
    /// <param name="appendValue"></param>
    /// <returns></returns>
    public static string[] SetDefault(this string[] args, string argumentPrefix, string appendValue)
    {
        if (args.Any(a => a.StartsWith(argumentPrefix, StringComparison.CurrentCultureIgnoreCase)))
            return args;
        
        var tempArgs = args.ToList();
            
        tempArgs.Add($"{argumentPrefix}{appendValue}");
            
        return tempArgs.ToArray();
    }
}