using System.Globalization;

using CounterStrikeSharp.API.Modules.Commands;

using PugSharp.Translation;

namespace PugSharp.Extensions;
internal static class CommandInfoExtensions
{
    public static void ReplyToCommand(this CommandInfo commandInfo, ITextHelper textHelper, string textKey, params object?[] arguments)
    {
        string translatedText;
        if (commandInfo.CallingPlayer == null)
        {
            translatedText = textHelper.GetTranslatedText(textKey, CultureInfo.InvariantCulture, false, arguments);
        }
        else
        {
            translatedText = textHelper.GetText(textKey, arguments);
        }

        commandInfo.ReplyToCommand(translatedText);

    }
}
