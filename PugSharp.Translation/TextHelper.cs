using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using PugSharp.Translation.Properties;

namespace PugSharp.Translation;

public partial class TextHelper : ITextHelper
{
    private readonly ILogger<TextHelper> _Logger;

    private const char _DefaultColor = '\u0001';
    private const int _RegexTimeout = 1000;
    private readonly char _HighlightColor;
    private readonly char _CommandColor;
    private readonly char _ErrorColor;

    public TextHelper(ILogger<TextHelper> logger, char highlightColor, char commandColor, char errorColor)
    {
        _Logger = logger;
        _HighlightColor = highlightColor;
        _CommandColor = commandColor;
        _ErrorColor = errorColor;
    }

    [GeneratedRegex(@"(\*\*(?<highlight>.*?)\*\*)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex HighlightRegex();

    [GeneratedRegex(@"(`(?<command>.*?)`)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex CommandRegex();


    [GeneratedRegex(@"(!!(?<error>.*?)!!)", RegexOptions.ExplicitCapture, _RegexTimeout)]
    private static partial Regex ErrorRegex();

    public string GetTranslatedText(string key, CultureInfo cultureInfo, params object?[] arguments)
    {
        return GetTranslatedText(key, cultureInfo, true, arguments);
    }

    public string GetTranslatedText(string key, CultureInfo cultureInfo, bool withColors, params object?[] arguments)
    {
        var text = Resources.ResourceManager.GetString(key.Replace('_', '.'), cultureInfo);
        if (text == null)
        {
            return $"?key?";
        }

        if (arguments.Length > 0)
        {
            try
            {
                var stringArguments = arguments.Select(GetArgumentString).ToArray<object>();
                text = string.Format(CultureInfo.CurrentCulture, text, stringArguments);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error formatting text {text}", text);
            }
        }

        if (withColors)
        {
            if (HighlightRegex().IsMatch(text)
                || CommandRegex().IsMatch(text)
                || ErrorRegex().IsMatch(text))
            {
                text = $" {_DefaultColor}{text}";
            }

            text = HighlightRegex().Replace(text, match => $"{_HighlightColor}{match.Groups["highlight"].Value}{_DefaultColor}");
            text = CommandRegex().Replace(text, match => $"{_CommandColor}{match.Groups["command"].Value}{_DefaultColor}");
            text = ErrorRegex().Replace(text, match => $"{_ErrorColor}{match.Groups["error"].Value}{_DefaultColor}");
        }
        else
        {
            text = HighlightRegex().Replace(text, match => match.Groups["highlight"].Value);
            text = CommandRegex().Replace(text, match => match.Groups["command"].Value);
            text = ErrorRegex().Replace(text, match => match.Groups["error"].Value);
        }
        return text;
    }


    public string GetText(string key, params object?[] arguments) => GetTranslatedText(key, CultureInfo.CurrentUICulture, arguments);

    private string GetArgumentString(object? value)
    {
        return value switch
        {
            string s => s,
            int i => i.ToString(CultureInfo.CurrentUICulture),
            float f => f.ToString(CultureInfo.CurrentUICulture),
            double d => d.ToString(CultureInfo.CurrentUICulture),
            _ => value?.ToString() ?? string.Empty,
        };
    }
}
