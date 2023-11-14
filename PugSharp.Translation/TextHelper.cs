using System.Globalization;
using System.Text.RegularExpressions;

namespace PugSharp.Translation
{
    public partial class TextHelper : ITextHelper
    {
        private const char _DefaultColor = '\u0001';
        private const int _RegexTimeout = 1000;
        private readonly char _HighlightColor;
        private readonly char _CommandColor;
        private readonly char _ErrorColor;

        public TextHelper(char highlightColor, char commandColor, char errorColor)
        {
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

        public string GetText(string key, params object?[] arguments)
        {

            var text = Resources.ResourceManager.GetString(key.Replace('_', '.'), CultureInfo.CurrentCulture);
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
                catch (Exception)
                {
                    // Do nothing
                }
            }

            if (HighlightRegex().IsMatch(text)
                || CommandRegex().IsMatch(text)
                || ErrorRegex().IsMatch(text))
            {
                text = $" {_DefaultColor}{text}";
            }

            text = HighlightRegex().Replace(text, match => $"{_HighlightColor}{match.Groups["highlight"].Value}{_DefaultColor}");
            text = CommandRegex().Replace(text, match => $"{_CommandColor}{match.Groups["command"].Value}{_DefaultColor}");
            text = ErrorRegex().Replace(text, match => $"{_ErrorColor}{match.Groups["error"].Value}{_DefaultColor}");

            return text;
        }

        private string GetArgumentString(object? value)
        {
            return value switch
            {
                string s => s,
                int i => i.ToString(CultureInfo.CurrentCulture),
                float f => f.ToString(CultureInfo.CurrentCulture),
                double d => d.ToString(CultureInfo.CurrentCulture),
                _ => value?.ToString() ?? string.Empty,
            };
        }
    }
}
