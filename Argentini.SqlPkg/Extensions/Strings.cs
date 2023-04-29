using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Argentini.SqlPkg.Extensions;

/// <summary>
/// Characters for use in string functions.
/// </summary>
public static class Characters
{
	public const string CrLf = "\r\n";

	/// <summary>
	/// Characters considered to be whitespace
	/// </summary>
	public static readonly char[] Whitespace = new char[]
	{
		'\u0009',  // CHARACTER TABULATION
	    '\u000A',  // LINE FEED
	    '\u000B',  // LINE TABULATION
	    '\u000C',  // FORM FEED
	    '\u000D',  // CARRIAGE RETURN
	    '\u0020',  // SPACE
	    '\u00A0',  // NO-BREAK SPACE
	    '\u2000',  // EN QUAD
	    '\u2001',  // EM QUAD
	    '\u2002',  // EN SPACE
	    '\u2003',  // EM SPACE
	    '\u2004',  // THREE-PER-EM SPACE
	    '\u2005',  // FOUR-PER-EM SPACE
	    '\u2006',  // SIX-PER-EM SPACE
	    '\u2007',  // FIGURE SPACE
	    '\u2008',  // PUNCTUATION SPACE
	    '\u2009',  // THIN SPACE
	    '\u200A',  // HAIR SPACE
	    '\u200B',  // ZERO WIDTH SPACE
	    '\u3000',  // IDEOGRAPHIC SPACE
	    '\uFEFF'  // ZERO WIDTH NO-BREAK SPACE
    };

	/// <summary>
	/// Characters considered to be delimiters for whole-word text searches
	/// </summary>
	public static readonly char[] WordDelimiters = new char[]
	{
		'\u0009',  // CHARACTER TABULATION
	    '\u000A',  // LINE FEED
	    '\u000B',  // LINE TABULATION
	    '\u000C',  // FORM FEED
	    '\u000D',  // CARRIAGE RETURN
	    '\u0020',  // SPACE
	    '\u00A0',  // NO-BREAK SPACE
	    '\u2000',  // EN QUAD
	    '\u2001',  // EM QUAD
	    '\u2002',  // EN SPACE
	    '\u2003',  // EM SPACE
	    '\u2004',  // THREE-PER-EM SPACE
	    '\u2005',  // FOUR-PER-EM SPACE
	    '\u2006',  // SIX-PER-EM SPACE
	    '\u2007',  // FIGURE SPACE
	    '\u2008',  // PUNCTUATION SPACE
	    '\u2009',  // THIN SPACE
	    '\u200A',  // HAIR SPACE
	    '\u200B',  // ZERO WIDTH SPACE
	    '\u3000',  // IDEOGRAPHIC SPACE
	    '\uFEFF',  // ZERO WIDTH NO-BREAK SPACE
		'!',
		'@',
		'#',
		'$',
		'%',
		'^',
		'&',
		'*',
		'(',
		')',
		'-',
		'=',
		',',
		'.',
		'/',
		'[',
		']',
		'\\',
		'`',
		'<',
		'>',
		'?',
		'{',
		'}',
		'|',
		'~'
	};
}

/// <summary>
/// Various tools for working with strings. 
/// </summary>
public static class Strings
{
	#region Trimming
	
	/// <summary>
	/// Converts two or more consecutive spaces into a single space.
	/// </summary>
	/// <param name="value">String to process</param>
	/// <returns>String with only single spaces</returns>
	public static string ConsolidateSpaces(this string value)
	{
		var regEx = new Regex(@"[\s]+");
		
		return regEx.Replace(value, " ");
	}
	
	/// <summary>
	/// Trim leading and trailing whitespace, which includes space, non-breaking space, carriage returns, line feeds, 
	/// tabs, en space, em space, and other ASCII and Unicode whitespace characters.
	/// </summary>
	/// <param name="value">String to evaluate</param>
	/// <returns>String with leading and trailing whitespace removed.</returns>
	public static string TrimWhitespace(this string? value)
	{
		return value?.Trim(Characters.Whitespace) ?? string.Empty;
	}

	/// <summary>
    /// Remove a specified number of characters from the beginning of a string
    /// </summary>
    /// <param name="value">String to trim</param>
    /// <param name="count">Number of characters to remove</param>
    /// <returns>Trimmed string</returns>
	public static string? TrimStart(this string? value, int count)
	{
		if (value is not null && value.Length >= count)
			return value.Right(value.Length - count);

		return value;
	}

	/// <summary>
    /// Remove a specified number of characters from the end of a string
    /// </summary>
    /// <param name="value">String to trim</param>
    /// <param name="count">Number of characters to remove</param>
    /// <returns>Trimmed string</returns>
	public static string? TrimEnd(this string? value, int count)
	{
		if (value is not null && value.Length >= count)
			return value.Left(value.Length - count);

		return value;
	}

	/// <summary>
	/// Remove a string from the beginning of a string
	/// </summary>
	/// <param name="source">The string to search</param>
	/// <param name="substring">The substring to remove</param>
	/// <param name="stringComparison"></param>
	/// <returns>Trimmed source</returns>
	public static string? TrimStart(this string? source, string? substring = " ", StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source == null || source.IsEmpty() || substring is null or "")
			return null;

		var result = new StringBuilder(source);
		
		result.TrimStart(substring, stringComparison);

		return result.ToString();
	}

	/// <summary>
	/// Remove a string from the end of a string
	/// </summary>
	/// <param name="source">The string to search</param>
	/// <param name="substring">The substring to remove</param>
	/// <param name="stringComparison"></param>
	/// <returns>Trimmed source</returns>
	public static string? TrimEnd(this string? source, string? substring = " ", StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source == null || source.IsEmpty() || substring is null or "")
			return null;

		var result = new StringBuilder(source);
		
		result.TrimEnd(substring, stringComparison);

		return result.ToString();
	}
	
	#endregion
	
	#region Case
	
    /// <summary>
    /// Convert a string to AP style title case, which makes all words use an upper case first character,
    /// except a core set of small words, unless one of those small words is the first or last one in the string.
    /// </summary>
    /// <param name="value">String to make AP title case</param>
    /// <param name="alwaysLowerIgnoreWords">Always use lower case for ignored words (not true AP title case)</param>
    /// <returns>String in AP title case</returns>
    public static string ApTitleCase(this string? value, bool alwaysLowerIgnoreWords = false)
    {
	    if (value == null) return string.Empty;

	    value = value.Trim();

	    if (value.Length == 0) return string.Empty;

	    var ignoreWords = new[]
	    {
		    "a", "an", "and", "at", "but", "by", "for", "in",
		    "nor", "of", "on", "or", "so", "the", "to", "up", "yet"
	    };

	    var space = new[] { ' ' };
	    var cultureInfo = CultureInfo.InvariantCulture;
	    var textInfo = cultureInfo.TextInfo;
	    var tokens = value.Split(space, StringSplitOptions.None).ToList();

	    if (tokens.Count <= 2)
		    return textInfo.ToTitleCase(value.ToLowerInvariant());

	    // Extract and process the first word
	    string firstWord = tokens[0].All(char.IsUpper) ? tokens[0] : textInfo.ToTitleCase(tokens[0].ToLowerInvariant());
	    tokens.RemoveAt(0);

	    // Remove any trailing spaces
	    while (tokens.Count > 0 && string.IsNullOrWhiteSpace(tokens[^1]))
		    tokens.RemoveAt(tokens.Count - 1);

	    if (tokens.Count <= (alwaysLowerIgnoreWords ? 1 : 2))
		    return textInfo.ToTitleCase(value.ToLowerInvariant());

	    // Extract and process the last word
	    string lastWord = tokens[^1].All(char.IsUpper) ? tokens[^1] : textInfo.ToTitleCase(tokens[^1].ToLowerInvariant());
	    tokens.RemoveAt(tokens.Count - 1);

	    var newTitle = new StringBuilder(firstWord);

	    foreach (var token in tokens)
	    {
		    newTitle.Append(' ');

		    if (ignoreWords.Contains(token.ToLowerInvariant()))
		    {
			    newTitle.Append(token.All(char.IsUpper) ? token : token.ToLowerInvariant());
		    }
		    else
		    {
			    newTitle.Append(token.All(char.IsUpper) ? token : textInfo.ToTitleCase(token.ToLowerInvariant()));
		    }
	    }

	    newTitle.Append(' ').Append(lastWord);

	    return newTitle.ToString();
    }
    
	#endregion
	
	#region Transform

    private static IEnumerable<string> WrapTextAtMaxWidth(string input, int maxLength)
    {
	    var result = new List<string>();
	    var indentation = Regex.Match(input, @"^\s+").Value;
		var words = input.TrimStart().Split(' ');
	    var currentLineLength = indentation.Length;
	    var currentLine = new StringBuilder(indentation);

	    foreach (var word in words)
	    {
		    if (currentLineLength + word.Length > maxLength)
		    {
			    result.Add(currentLine.ToString());
			    currentLine.Clear();
			    currentLine.Append(indentation);
			    currentLineLength = indentation.Length;
		    }

		    currentLine.Append(word);
		    currentLine.Append(' ');
		    currentLineLength += word.Length + 1;
	    }

	    if (currentLine.Length > indentation.Length)
		    result.Add(currentLine.ToString());

	    return result;
    }
	
	public static void WriteToConsole(this string text, int maxCharacters)
	{
		if (string.IsNullOrEmpty(text))
			return;

		var result = new List<string>();
		var lines = text.Trim().NormalizeLinebreaks().Split('\n');

		foreach (var line in lines)
			result.AddRange(WrapTextAtMaxWidth(line, maxCharacters));

		foreach (var line in result)
		{
			Console.WriteLine(line.NormalizeLinebreaks(Environment.NewLine));
		}
	}
	
	/// <summary>
	/// Fixes folder paths that have duplicate separator characters, replaces "~" with the
	/// user's home path, and ensures a trailing path separator.
	/// </summary>
	/// <param name="folderPath">Folder path to process</param>
	/// <returns>A processed folder path</returns>
	public static string ProcessFolderPath(this string folderPath)
	{
		var result = folderPath.Trim(new[] { '\"' });

		result = result.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		result = result.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
        result = result.Replace($"{Path.AltDirectorySeparatorChar}{Path.AltDirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
        result = Path.GetFullPath(result).TrimEnd(new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) + Path.DirectorySeparatorChar;

		return result;
	}
	
	public static string RemoveWrappedQuotes(this string value)
	{
		var result = value;
        
		if (result.StartsWith('\"') && result.EndsWith('\"'))
			result = result.Trim('\"');

		if (result.StartsWith('\'') && result.EndsWith('\''))
			result = result.Trim('\'');
        
		return result;        
	}
	
	/// <summary>
	/// Repeat a string a specified number of times.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="n"></param>
	/// <returns></returns>
	public static string Repeat(this string text, int n)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;
		
		var textAsSpan = text.AsSpan();
		var span = new Span<char>(new char[textAsSpan.Length * n]);
	
		for (var i = 0; i < n; i++)
		{
			textAsSpan.CopyTo(span.Slice(i * textAsSpan.Length, textAsSpan.Length));
		}

		return span.ToString();
	}
	
	/// <summary>
	/// Remove the query string from a URL.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string RemoveQueryString(this string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		var index = value.IndexOf('?');

		return index == -1 ? value : value[..index];
	}
	
	/// <summary>
	/// Convert an object into a quoted SQL string value.
	/// Sanitizes strings to prevent SQL injection.
	/// Formats dates and times to ISO 8601-1:2019.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string SqlQuotedValue(this object value)
	{
		return value switch
		{
			string s => $"'{s.SqlSanitize()}'",
			Guid g => $"'{g.ToString()}'",
			DateOnly d => $"'{d:O}'",
			TimeOnly t => $"'{t:O}'",
			DateTime d => $"'{d:yyyy-MM-dd}T{d:HH:mm:ss}{d:zzz}'", // ISO 8601-1:2019
			DateTimeOffset d => $"'{d:yyyy-MM-dd}T{d:HH:mm:ss}{d:zzz}'", // ISO 8601-1:2019
			bool b => $"{(b ? "1" : "0")}",
			short s => $"{s}",
			int i => $"{i}",
			long l => $"{l}",
			double d => $"{d}",
			float f => $"{f}",
			decimal d => $"{d}",
			char c => $"'{c.ToString().SqlSanitize()}'",
			_ => $"'{(value.ToString() ?? string.Empty).SqlSanitize()}'"
		};
	}
	
	/// <summary>
	/// Add spaces at regular intervals within a string.
	/// Useful for making a more readable code sequence, like "ABCDEFGHIJKLMNO" as "ABC DEF GHI JKL MNO")
	/// </summary>
	/// <param name="value"></param>
	/// <param name="charactersPerChunk">Defaults to 4</param>
	/// <returns></returns>
	public static string InsertSpacesInSequence(this string? value, int charactersPerChunk = 4)
	{
		if (value == null || value.IsEmpty()) return string.Empty;

		return Regex.Replace(value, $".{{{charactersPerChunk}}}", "$0 ").Trim();
	}
	
    /// <summary>
    /// Convert unicode characters with diacritic marks into English equivalents.
    /// </summary>
    /// <param name="value">String to evaluate</param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string RemoveDiacritics(this string? value)
    {
        if (value == null || value.IsEmpty()) return string.Empty;

        value = value.Normalize(NormalizationForm.FormD);

        var chars = value.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();

        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Remove punctuation from a string. Uses regex to keep whitespace and words.
    /// </summary>
    /// <param name="value">String to filter</param>
    /// <param name="except">Regex list of characters to keep (e.g. "'")</param>
    /// <returns></returns>
    public static string RemovePunctuation(this string? value, string except = "")
    {
	    if (value == null || value.IsEmpty())
		    return string.Empty;

        return Regex.Replace(value, @"[^\w\s" + except + "]" , string.Empty);
    }

	/// <summary>
	/// Convert an object to a string. If null an empty string is returned.
	/// </summary>
	/// <param name="obj">Object to convert to a string</param>
	/// <returns>String value or an empty string if null</returns>
	public static string SafeToString(this object? obj)
	{
		return obj?.ToString() ?? string.Empty;
	}

	/// <summary>
	/// <![CDATA[
	/// Sanitize a string so that it resists SQL injection attacks;
	/// replaces single apostrophes with two apostrophes.
	/// ]]>
	/// </summary>
	/// <param name="value">String to sanitize</param>
	/// <returns>A sanitized string.</returns>
	public static string SqlSanitize(this string value)
	{
		return value.Replace("'", "''");
	}

    /// <summary>
    /// Normalize line breaks.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="linebreak">Line break to use (default: "\n")</param>
    public static string NormalizeLinebreaks(this string? value, string? linebreak = "\n")
    {
	    if (value == null || value.IsEmpty()) return string.Empty;

	    if (linebreak == null || linebreak.IsEmpty()) return value;
        
        if (value.Contains("\r\n") && linebreak != "\r\n")
            return value.Replace("\r\n", linebreak);

        if (value.Contains('\r') && linebreak != "\r")
            return value.Replace("\r", linebreak);

        if (value.Contains('\n') && linebreak != "\n")
            return value.Replace("\n", linebreak);

        return value;
    }
	
	/// <summary>
	/// Returns a default value is the string is null, empty, or only contains whitespace
	/// </summary>
	/// <param name="source"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string SafeValue(this string? source, string defaultValue = "")
	{
		if (source == null || source.IsEmpty()) return defaultValue;

		return source;
	}
	
	/// <summary>
	/// Indent text with given whitespace based on line breaks
	/// </summary>
	/// <param name="block"></param>
	/// <param name="whitespace"></param>
	/// <param name="includeLeading"></param>
	/// <returns></returns>
	public static string Indent(this string block, string whitespace, bool includeLeading = false)
	{
		var result = block.Trim().NormalizeLinebreaks("\r\n");
        
		if (result.HasValue())
			result = result.Replace("\r\n", "\r\n" + whitespace);

		return (includeLeading ? whitespace : string.Empty) + result.Trim();
	}

	/// <summary>
	/// Format a double using culture-specific formatting and specific decimal places without rounding.
	/// Uses current culture to format the number.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="decimalPlaces"></param>
	/// <param name="isFractionalPercentage"></param>
	/// <returns></returns>
	public static string FormatNoRounding(this double value, int decimalPlaces = 2, bool isFractionalPercentage = false)
	{
		if (isFractionalPercentage)
			value *= 100;

		var valueStr = value.ToString(CultureInfo.InvariantCulture);
		var segments = valueStr.Split('.', StringSplitOptions.RemoveEmptyEntries);
			
		if (decimalPlaces < 1)
		{
			valueStr = segments[0];
		}

		else
		{
			if (segments.Length == 2)
			{
				if (segments[1].Length > decimalPlaces)
					segments[1] = segments[1][..decimalPlaces];

				valueStr = string.Join('.', segments);
			}
		}

		return double.Parse(valueStr).ToString($"N{decimalPlaces}", CultureInfo.CurrentCulture);
	}
	
	#endregion
	
	#region Comparison	

	/// <summary>
	/// Determines if a string has a value (is not null and not empty).
	/// </summary>
	/// <param name="value">String to evaluate</param>
	public static bool HasValue(this string? value)
	{
		return string.IsNullOrEmpty(value?.Trim()) == false;
	}

	/// <summary>
	/// Determines if a string is empty or null.
	/// </summary>
	/// <param name="value">String to evaluate</param>
	public static bool IsEmpty(this string? value)
	{
		return string.IsNullOrEmpty(value);
	}
	
	/// <summary>
	/// Determine if two strings are not equal.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="value"></param>
	/// <param name="comparisonType"></param>
	/// <returns></returns>
	public static bool NotEquals(this string? source, string? value, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source == null && value == null) return false;
		if (source is not null && value == null) return true;
		if (source == null && value is not null) return true;
		
		return source?.Equals(value, comparisonType) == false;
	}

	/// <summary>
	/// Determine if a string starts with any value from a string array.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="substrings"></param>
	/// <param name="stringComparison"></param>
	/// <returns></returns>
	public static bool StartsWith(this string value, string[]? substrings, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (substrings is not {Length: > 0}) return false;

		var result = false;
        
		for (var x = 0; x < substrings.Length; x++)
		{
			if (value.StartsWith(substrings[x], stringComparison) == false)
				continue;
            
			result = true;
			x = substrings.Length;
		}

		return result;
	}
	
	#endregion
	
	#region Conversion

	/// <summary>
	/// Convert a GUID to a Javascript-compatible id value.
	/// </summary>
	/// <param name="guid">GUID to convert</param>
	// ReSharper disable once MemberCanBePrivate.Global
	public static string GuidToId(this string guid)
	{
		return "w" + guid.Replace("-" , string.Empty);
	}

	/// <summary>
	/// Convert a GUID to a Javascript-compatible id value.
	/// </summary>
	/// <param name="guid">GUID to convert</param>
	public static string GuidToId(this Guid guid)
	{
		return GuidToId(guid.ToString());
	}

	/// <summary>
	/// Convert a GUID to a Javascript-compatible id value.
	/// </summary>
	/// <param name="guid">GUID to convert</param>
	public static string GuidToId(this Guid? guid)
	{
		return GuidToId(guid.HasValue ? guid.Value.ToString() : Guid.NewGuid().ToString());
	}

	/// <summary>
	/// Convert a string to a byte array.
	/// </summary>
	/// <param name="value">String to evaluate</param>
	/// <returns>Byte array</returns>
	public static byte[] ToByteArray(this string? value)
	{
		if (value == null || value.IsEmpty()) return Array.Empty<byte>();
		
		var encoding = new UTF8Encoding();

		return encoding.GetBytes(value);
	}
	
	/// <summary>
	/// Take first, middle, and last name and makes a sortable string as Last, First Middle
	/// </summary>
	/// <param name="firstName">First name</param>
	/// <param name="middleName">Middle name</param>
	/// <param name="lastName">Last name</param>
	/// <returns>Sortable name</returns>
	public static string SortableName(string firstName, string middleName, string lastName)
	{
		var result = string.Empty;

		if (firstName.HasValue() || middleName.HasValue() || lastName.HasValue())
			result = ((lastName.HasValue() ? lastName.Trim() + "," : string.Empty) + (firstName.HasValue() ? " " + firstName.Trim() : string.Empty) + (middleName.HasValue() ? " " + middleName.Trim() : string.Empty)).Trim(new char[] { ' ', ',' });

		return result;
	}
	
	/// <summary>
	/// Creates a string from the sequence by concatenating the result
	/// of the specified string selector function for each element.
	/// Concatenates the strings with no delimiter.
	/// </summary>
	/// <param name="source">The source IEnumerable object</param>
	/// <param name="stringSelector">Abstraction for the individual string objects</param>
	public static string ToConcatenatedString<T>(this List<T> source, Func<T, string> stringSelector)
	{
		return ToConcatenatedString(source, stringSelector, string.Empty);
	}

	/// <summary>
	/// Creates a string from the sequence by concatenating the result
	/// of the specified string selector function for each element.
	/// Concatenates the string with a specified delimiter.
	/// </summary>
	/// <param name="source">The source IEnumerable object</param>
	/// <param name="stringSelector">Abstraction for the individual string objects</param>
	/// <param name="delimiter">The string which separates each concatenated item</param>
	public static string ToConcatenatedString<T>(this List<T> source, Func<T, string> stringSelector, string delimiter)
	{
		var b = new StringBuilder();
		var needsSeparator = false;

		foreach (var item in CollectionsMarshal.AsSpan(source))
		{
			if (needsSeparator)
				b.Append(delimiter);

			b.Append(stringSelector(item));
			needsSeparator = true;
		}

		return b.ToString();
	}

	#endregion
	
	#region Parsing

	/// <summary>
	/// Determine if a string contains any one of a string of individual characters.
	/// Useful for determining if a string has any upper case characters, as one example.
	/// </summary>
	/// <param name="source">String to check</param>
	/// <param name="characters">String of characters for which to check</param>
	// ReSharper disable once MemberCanBePrivate.Global
	public static bool ContainsCharacters(this string? source, string characters)
	{
		var result = false;

		if (!source.HasValue() || !characters.HasValue()) return result;
		
		for (var x = 0; x < characters.Length; x++)
		{
			if (source is not null && source.IndexOf(characters.Substring(x, 1), StringComparison.Ordinal) < 0)
				continue;
			
			result = true;
			x = characters.Length;
		}

		return result;
	}
	
	/// <summary>
	/// Get the left "length" characters of a string.
	/// </summary>
	/// <param name="value">String value</param>
	/// <param name="length">Number of characters</param>
	/// <returns>Left portion of a string</returns>
	public static string Left(this string? value, int length)
	{
		if (value == null || value.IsEmpty() || length < 1) return string.Empty;

		if (length > value.Length) return value;
		
		return value[..length];
	}

	/// <summary>
	/// Get the left characters of a string up to but not including the first instance of "marker".
	/// If marker is not found the original value is returned.
	/// </summary>
	/// <param name="value">String value</param>
	/// <param name="marker">Delimiter to denote the cut off point</param>
	/// <returns>Left portion of a string</returns>
	public static string Left(this string? value, string? marker)
	{
		if (value == null || value.IsEmpty()) return string.Empty;

		if (marker == null || marker.IsEmpty()) return value;

		if (value.Length <= marker.Length) return string.Empty;

		if (value.Contains(marker))
			return value[..value.IndexOf(marker, StringComparison.Ordinal)];

		return value;
	}
	
	/// <summary>
	/// Get the right "length" characters of a string.
	/// </summary>
	/// <param name="value">String value</param>
	/// <param name="length">Number of characters</param>
	/// <returns>Right portion of a string</returns>
	public static string Right(this string? value, int length)
	{
		if (value == null || value.IsEmpty() || length < 1) return string.Empty;

		if (length > value.Length) return value;

		return value[^length..];
	}

	/// <summary>
	/// Get the right characters of a string up to but not including the last instance of "marker" (right to left).
	/// If marker is not found the original value is returned.
	/// </summary>
	/// <param name="value">String value</param>
	/// <param name="marker">Delimiter to denote the cut off point</param>
	/// <returns>Right portion of a string</returns>
	public static string Right(this string? value, string? marker)
	{
		if (value == null || value.IsEmpty()) return string.Empty;

		if (marker == null || marker.IsEmpty()) return value;

		if (value.Length <= marker.Length) return string.Empty;
		
		if (value.Contains(marker))
			return value[(value.LastIndexOf(marker, StringComparison.Ordinal) + marker.Length)..];

		return value;
	}
	
	/// <summary>
	/// Retrieve a filename from a path.
	/// </summary>
	/// <example>
	/// <code>
	/// string filename = GetFilename(filepath);
	/// </code>
	/// </example>
	/// <param name="filePath">File path to parse</param>
	/// <returns>Filename as a string.</returns>
	public static string GetFilename(this string? filePath)
	{
		if (filePath == null || filePath.IsEmpty()) return string.Empty;
		
		var separator = filePath.Contains(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar.ToString() : Path.AltDirectorySeparatorChar.ToString();

        if (filePath.EndsWith(separator))
			return string.Empty;

		var x = filePath.LastIndexOf(separator, StringComparison.Ordinal);

		if (x < 0)
			return filePath;

		return filePath[(x + 1)..];
	}

	/// <summary>
	/// Get the last few characters after the final period in a string,
	/// typically the file extension.
	/// </summary>
	/// <param name="filename"></param>
	// ReSharper disable once MemberCanBePrivate.Global
	public static string FileExtension(this string? filename)
	{
		if (filename == null || filename.IsEmpty()) return string.Empty;

		if (filename.EndsWith('.'))
			return string.Empty;
		
		var dotLocation = filename.LastIndexOf('.');

		if (dotLocation < 0)
			return filename;
		
		return filename[(dotLocation + 1)..];
	}

	#endregion
	
	#region Timers

	/// <summary>
	/// Format the elapsed time as a more friendly time span string.
	/// Example: 3 days 10h:37m:15.123s
	/// </summary>
	/// <param name="msecs"></param>
	/// <returns>Formatted timespan</returns>
	public static string FormatTimer(this long msecs)
	{
		var timespan = TimeSpan.FromMilliseconds(msecs);

		return $"{(timespan.Days > 0 ? timespan.Days.ToString("#,##0") + " days " : "")}{timespan.Hours:00}h:{timespan.Minutes:00}m:{timespan.Seconds:00}.{timespan.Milliseconds:D}s";
	}

	/// <summary>
	/// Returns a string with the time in seconds as well as the performance per second
	/// (e.g. "100.2 sec (10,435.1/sec)")
	/// </summary>
	/// <param name="numberProcessed">Number of items processed in the elapsed time</param>
	/// <param name="msecs">Number milliseconds to output (overrides ElapsedMs)</param>
	/// <param name="decimalPlaces">Number of decimal places to show</param>
	/// <returns></returns>
	public static string PerformanceTimeString(int numberProcessed, long msecs, int decimalPlaces = 1)
	{
		return $"{FormatTimer(msecs)} ({PerformanceString(numberProcessed, msecs, decimalPlaces)})";
	}

	/// <summary>
	/// Returns a string with the performance per second
	/// (e.g. "10,435.1/sec")
	/// </summary>
	/// <param name="numberProcessed">Number of items processed in the elapsed time</param>
	/// <param name="msecs">Number milliseconds to output (overrides ElapsedMs)</param>
	/// <param name="decimalPlaces">Number of decimal places to show</param>
	/// <returns></returns>
	public static string PerformanceString(int numberProcessed, double msecs, int decimalPlaces = 1)
	{
		var secs = msecs / 1000;

		return $"{Math.Round(numberProcessed / secs, decimalPlaces).ToString($"N{decimalPlaces}")}/sec";
	}
	
	#endregion
	
	#region RegEx
	
	/// <summary>
	/// Find and replace based on position and length.
	/// </summary>
	/// <param name="match"></param>
	/// <param name="source"></param>
	/// <param name="replacement"></param>
	/// <returns></returns>
	public static string Replace(this Match match, string source, string replacement)
	{
		if (string.IsNullOrEmpty(source))
			return source;

		return source[..match.Index] + replacement + source[(match.Index + match.Length)..];
	}	
	
	#endregion
}
