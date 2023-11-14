namespace PugSharp.Translation
{
    public interface ITextHelper
    {
        string GetText(string key, params object?[] arguments);
    }
}
