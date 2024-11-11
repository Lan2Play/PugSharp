using System.Globalization;

namespace PugSharp.Translation;

public interface ITextHelper
{
    string GetText(string key, params object?[] arguments);
    string GetTranslatedText(string key, CultureInfo cultureInfo, params object?[] arguments);
    string GetTranslatedText(string key, CultureInfo cultureInfo, bool withColors, params object?[] arguments);
}
