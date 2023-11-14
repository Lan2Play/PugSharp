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

        [GeneratedRegex(@"(\*\*(?<highglight>.*?)\*\*)", RegexOptions.ExplicitCapture, _RegexTimeout)]
        private static partial Regex HighlightRegex();

        [GeneratedRegex(@"(`(?<command>.*?)`)", RegexOptions.ExplicitCapture, _RegexTimeout)]
        private static partial Regex CommandRegex();


        [GeneratedRegex(@"(`!!(?<error>.*?)!!)", RegexOptions.ExplicitCapture, _RegexTimeout)]
        private static partial Regex ErrorRegex();

        public string GetText(string key, params object?[] arguments)
        {
            var text = Resources.ResourceManager.GetString(key, CultureInfo.CurrentCulture);
            if (text == null)
            {
                return $"?key?";
            }

            text = string.Format(CultureInfo.CurrentCulture, text, arguments);

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
    }
}
