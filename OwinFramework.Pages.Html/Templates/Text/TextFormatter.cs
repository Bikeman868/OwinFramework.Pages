namespace OwinFramework.Pages.Html.Templates.Text
{
    public class TextFormatter : ITextFormatter
    {

        public string LimitLength(string text, int maxLength, string padString, bool wholeWords)
        {
            if (text == null)
                return string.Empty;

            if (maxLength <= 0)
                return text;

            if (padString == null)
                padString = "...";

            var length = maxLength - padString.Length;

            if (length <= 0)
                return padString;

            if (text.Length > length)
            {
                if (wholeWords && char.IsLetterOrDigit(text[length]))
                {
                    while (length > 0 && char.IsLetterOrDigit(text[length - 1]))
                        --length;
                    while (length > 0 && !char.IsLetterOrDigit(text[length - 1]))
                        --length;
                }

                text = text.Substring(0, length);
                text = text + padString;
            }

            return text;
        }

        public string LimitLength(string text, int maxLength, string padString)
        {
            return LimitLength(text, maxLength, padString, false);
        }

        public string LimitLength(string text, int maxLength)
        {
            return LimitLength(text, maxLength, "...", false);
        }
    }
}
