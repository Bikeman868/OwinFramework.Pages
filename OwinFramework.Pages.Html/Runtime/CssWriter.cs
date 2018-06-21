using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class CssWriter: ICssWriter
    {
        public bool Indented { get; set; }
        public bool IncludeComments { get; set; }
        public bool HasContent { get { return _elements.Count > 0; } }

        private readonly List<CssElement> _elements = new List<CssElement>();

        public void Dispose()
        {
        }

        public void ToHtml(IHtmlWriter html)
        {
            foreach (var element in _elements)
                element.ToHtml(html);
        }

        public void ToStringBuilder(IStringBuilder stringBuilder)
        {
            foreach (var element in _elements)
                element.ToStringBuilder(stringBuilder);
        }

        public IList<string> ToLines()
        {
            var lines = new List<string>();

            foreach (var element in _elements)
                element.ToLines(lines);

            return lines;
        }

        public ICssWriter WriteRule(string selector, string styles, IPackage package)
        {
            _elements.Add(new CssRule
            {
                Selector = selector,
                Styles = styles
            });
            return this;
        }

        public ICssWriter WriteComment(string comment)
        {
            if (IncludeComments)
                _elements.Add(new CssComment { Comment = comment });
            return this;
        }

        private abstract class CssElement
        {
            public abstract void ToHtml(IHtmlWriter writer);
            public abstract void ToStringBuilder(IStringBuilder writer);
            public abstract void ToLines(IList<string> writer);
        }

        private class CssComment: CssElement
        {
            public string Comment;

            public override void ToHtml(IHtmlWriter writer)
            {
                if (!string.IsNullOrEmpty(Comment))
                    writer.WriteComment(Comment, CommentStyle.MultiLineC);
            }

            public override void ToStringBuilder(IStringBuilder writer)
            {
                if (!string.IsNullOrEmpty(Comment))
                {
                    writer.Append("/* ");
                    writer.Append(Comment);
                    writer.AppendLine(" */");
                }
            }

            public override void ToLines(IList<string> writer)
            {
                //if (!string.IsNullOrEmpty(Comment))
                //{
                //    writer.Add("/* " + Comment + " */");
                //}
            }
        }

        private class CssRule : CssElement
        {
            public string Selector;
            public string Styles;

            public override void ToHtml(IHtmlWriter writer)
            {
                writer.Write(Selector);
                writer.Write(" { ");
                writer.Write(Styles);
                writer.WriteLine(" }");
            }

            public override void ToStringBuilder(IStringBuilder writer)
            {
                writer.Append(Selector);
                writer.Append(" { ");
                writer.Append(Styles);
                writer.AppendLine(" }");
            }

            public override void ToLines(IList<string> writer)
            {
                writer.Add(Selector + " { " + Styles + " }");
            }
        }
    }
}
