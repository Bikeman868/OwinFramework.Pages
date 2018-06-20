using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class JavascriptWriter: IJavascriptWriter
    {
        public bool Indented { get; set; }
        public bool IncludeComments { get; set; }
        public int IndentLevel { get; set; }
        public bool HasContent { get { return _namespaces.Values.Any(ns => ns.HasContent); } }

        private readonly Dictionary<string, JavascriptNamespace> _namespaces;

        public JavascriptWriter()
        {
            _namespaces = new Dictionary<string, JavascriptNamespace>(StringComparer.OrdinalIgnoreCase);
        }

        public void ToHtml(IHtmlWriter html)
        {
            foreach (var javascriptNamespace in _namespaces.Values)
                javascriptNamespace.Write(html);
        }

        public void ToStringBuilder(IStringBuilder stringBuilder)
        {
            foreach (var javascriptNamespace in _namespaces.Values)
                javascriptNamespace.Write(stringBuilder);
        }

        public IList<string> ToLines()
        {
            var lines = new List<string>();

            foreach (var javascriptNamespace in _namespaces.Values)
                javascriptNamespace.Write(lines);

            return lines;
        }

        public IJavascriptWriter WriteVariable(string variableName, string initializationExpression, string type, IPackage package, bool isPublic )
        {
            var javascriptNamespace = GetNamespace(package);
            javascriptNamespace.Add(
                new VariableElement
                {
                    Name = variableName,
                    IsPublic = isPublic,
                    Type = type,
                    InitializationExpression = initializationExpression
                });
            return this;
        }

        public IJavascriptWriter WriteFunction(string functionName, string parameters, string functionBody, string returnType, IPackage package, bool isPublic)
        {
            var javascriptNamespace = GetNamespace(package);
            javascriptNamespace.Add(
                new FunctionElement
                {
                    Name = functionName,
                    IsPublic = isPublic,
                    Type = returnType,
                    Parameters = parameters,
                    Body = functionBody
                });
            return this;
        }

        public IJavascriptWriter WriteComment(string comment, CommentStyle commentStyle, IPackage package)
        {
            if (!IncludeComments) return this;

            var javascriptNamespace = GetNamespace(package);
            javascriptNamespace.Add(
                new CommentElement
                {
                    Name = null,
                    IsPublic = false,
                    Type = null,
                    Comment = comment,
                    CommentStyle = commentStyle
                });
            return this;
        }

        public void Dispose()
        {
        }

        private JavascriptNamespace GetNamespace(IPackage package)
        {
            var namespaceName = package == null ? string.Empty : package.NamespaceName;
            if (namespaceName == null) namespaceName = string.Empty;

            JavascriptNamespace result;
            if (_namespaces.TryGetValue(namespaceName, out result))
                return result;

            result = new JavascriptNamespace 
            { 
                NamespaceName = namespaceName
            };

            _namespaces[namespaceName] = result;
            return result;
        }

        private abstract class JavascriptElement
        {
            public string Name;
            public string Type;
            public bool IsPublic;

            public abstract void Write(IHtmlWriter writer);
            public abstract void Write(IStringBuilder stringBuilder, string indent);
            public abstract void Write(IList<string> lines, string indent);
        }

        private class CommentElement: JavascriptElement
        {
            public string Comment;
            public CommentStyle CommentStyle;

            public override void Write(IHtmlWriter writer)
            {
                writer.WriteComment(Comment, CommentStyle);
            }

            public override void Write(IStringBuilder stringBuilder, string indent)
            {
                stringBuilder.Append(indent);
                switch (CommentStyle)
                {
                    case CommentStyle.SingleLineC:
                        stringBuilder.Append("// ");
                        stringBuilder.AppendLine(Comment);
                        break;
                    case CommentStyle.MultiLineC:
                        stringBuilder.Append("/* ");
                        stringBuilder.Append(Comment);
                        stringBuilder.AppendLine(" */");
                        break;
                    case CommentStyle.Xml:
                        stringBuilder.Append("<!-- ");
                        stringBuilder.Append(Comment);
                        stringBuilder.AppendLine(" -->");
                        break;
                }
            }

            public override void Write(IList<string> lines, string indent)
            {
                switch (CommentStyle)
                {
                    case CommentStyle.SingleLineC:
                        lines.Add(indent + "// " + Comment);
                        break;
                    case CommentStyle.MultiLineC:
                        lines.Add(indent + "/* " + Comment + " */");
                        break;
                    case CommentStyle.Xml:
                        lines.Add(indent + "<!-- " + Comment + " -->");
                        break;
                }
            }
        }

        private class VariableElement: JavascriptElement
        {
            public string InitializationExpression;

            public override void Write(IHtmlWriter writer)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    if (!string.IsNullOrEmpty(InitializationExpression))
                        writer.Write(InitializationExpression);
                }
                else
                {
                    writer.Write(string.IsNullOrEmpty(Type) ? "var" : Type);
                    writer.Write(' ');
                    writer.Write(Name);
                    if (!string.IsNullOrEmpty(InitializationExpression))
                    {
                        writer.Write(" = ");
                        writer.Write(InitializationExpression);
                    }
                }
                writer.WriteLine(";");
            }

            public override void Write(IStringBuilder stringBuilder, string indent)
            {
                stringBuilder.Append(indent);

                if (string.IsNullOrEmpty(Name))
                {
                    if (!string.IsNullOrEmpty(InitializationExpression))
                        stringBuilder.Append(InitializationExpression);
                }
                else
                {
                    stringBuilder.Append(string.IsNullOrEmpty(Type) ? "var" : Type);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(Name);
                    if (!string.IsNullOrEmpty(InitializationExpression))
                    {
                        stringBuilder.Append(" = ");
                        stringBuilder.Append(InitializationExpression);
                    }
                }

                stringBuilder.AppendLine(";");
            }

            public override void Write(IList<string> lines, string indent)
            {
                var line = indent;

                if (string.IsNullOrEmpty(Name))
                {
                    if (!string.IsNullOrEmpty(InitializationExpression))
                        line += InitializationExpression;
                }
                else
                {
                    line += (string.IsNullOrEmpty(Type) ? "var" : Type) + ' ' + Name;
                    if (!string.IsNullOrEmpty(InitializationExpression))
                        line += " = " + InitializationExpression;
                }

                lines.Add(line);
            }
        }

        private class FunctionElement : JavascriptElement
        {
            public string Parameters;
            public string Body;

            public override void Write(IHtmlWriter writer)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    writer.Write("function (");
                }
                else if (IsPublic)
                {
                    writer.Write("var ");
                    writer.Write(Name);
                    writer.Write(" = function (");
                }
                else
                {
                    writer.Write("function ");
                    writer.Write(Name);
                    writer.Write(" (");
                }

                if (!string.IsNullOrEmpty(Parameters))
                    writer.Write(Parameters);

                writer.WriteLine(") {");
                writer.IndentLevel++;

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                    writer.WriteLine(line);

                writer.IndentLevel--;
                writer.WriteLine(string.IsNullOrEmpty(Name) ? "}();" : "}");
            }

            public override void Write(IStringBuilder stringBuilder, string indent)
            {
                stringBuilder.Append(indent);

                if (string.IsNullOrEmpty(Name))
                {
                    stringBuilder.Append("function (");
                }
                else if (IsPublic)
                {
                    stringBuilder.Append("var ");
                    stringBuilder.Append(Name);
                    stringBuilder.Append(" = function (");
                }
                else
                {
                    stringBuilder.Append("function ");
                    stringBuilder.Append(Name);
                    stringBuilder.Append(" (");
                }

                if (!string.IsNullOrEmpty(Parameters))
                    stringBuilder.Append(Parameters);

                stringBuilder.AppendLine(") {");

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                {
                    stringBuilder.Append(indent);
                    stringBuilder.Append("  ");
                    stringBuilder.AppendLine(line);
                }

                stringBuilder.Append(indent);
                stringBuilder.AppendLine(string.IsNullOrEmpty(Name) ? "}();" : "}");
            }

            public override void Write(IList<string> lines, string indent)
            {
                string header;

                if (string.IsNullOrEmpty(Name))
                    header = indent + "function (";
                else if (IsPublic)
                    header = indent + "var " + Name + " = function (";
                else
                    header = indent + "function " + Name + " (";

                if (!string.IsNullOrEmpty(Parameters))
                    header += Parameters;

                header += ") {";

                lines.Add(header);

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                    lines.Add(indent + "  " + line);

                lines.Add(indent + (string.IsNullOrEmpty(Name) ? "}();" : "}"));
            }
        }

        private class JavascriptNamespace
        {
            public string NamespaceName;
            public bool HasContent { get { return _elements != null; } }

            private List<JavascriptElement> _elements;

            public void Add(JavascriptElement element)
            {
                if (_elements == null) 
                    _elements = new List<JavascriptElement>();
                _elements.Add(element);
            }

            public void Write(IHtmlWriter writer)
            {
                if (_elements == null) return;

                var hasNamespace = !string.IsNullOrEmpty(NamespaceName);

                if (hasNamespace)
                {
                    writer.Write("var ns = (window.ns = window.ns || {});");
                    writer.Write("ns." + NamespaceName + " = function () {");
                    writer.IndentLevel++;
                }

                foreach (var element in _elements)
                {
                    writer.WriteLine();
                    element.Write(writer);
                }

                if (hasNamespace)
                {
                    writer.WriteLine();
                    writer.WriteLine("return {");
                    writer.IndentLevel++;

                    var firstElement = true;
                    foreach (var element in _elements)
                    {
                        if (!firstElement)
                            writer.WriteLine(",");

                        if (element.IsPublic && !string.IsNullOrEmpty(element.Name))
                        {
                            writer.Write(element.Name);
                            firstElement = false;
                        }
                    }
                    if (!firstElement)
                        writer.WriteLine();

                    writer.IndentLevel--;
                    writer.WriteLine("}");

                    writer.IndentLevel--;
                    writer.WriteLine("}();");
                    writer.WriteLine();
                }
            }

            public void Write(IStringBuilder stringBuilder)
            {
                if (_elements == null) return;

                var hasNamespace = !string.IsNullOrEmpty(NamespaceName);

                if (hasNamespace)
                {
                    stringBuilder.AppendLine("var ns = (window.ns = window.ns || {});");
                    stringBuilder.AppendLine("ns." + NamespaceName + " = function () {");
                }

                foreach (var element in _elements)
                {
                    stringBuilder.AppendLine(string.Empty);
                    element.Write(stringBuilder, hasNamespace ? "  " : string.Empty);
                }

                if (hasNamespace)
                {
                    stringBuilder.AppendLine(string.Empty);
                    stringBuilder.AppendLine("  return {");

                    var firstElement = true;
                    foreach (var element in _elements)
                    {
                        if (!firstElement)
                            stringBuilder.AppendLine(",");

                        if (element.IsPublic && !string.IsNullOrEmpty(element.Name))
                        {
                            stringBuilder.Append("    " + element.Name);
                            firstElement = false;
                        }
                    }
                    if (!firstElement)
                        stringBuilder.AppendLine(string.Empty);

                    stringBuilder.AppendLine("  }");
                    stringBuilder.AppendLine("}();");
                    stringBuilder.AppendLine(string.Empty);
                }
            }

            public void Write(IList<string> lines)
            {
                if (_elements == null) return;

                var hasNamespace = !string.IsNullOrEmpty(NamespaceName);

                if (hasNamespace)
                {
                    lines.Add("var ns = (window.ns = window.ns || {});");
                    lines.Add("ns." + NamespaceName + " = function () {");
                }

                foreach (var element in _elements)
                {
                    lines.Add(String.Empty);
                    element.Write(lines, hasNamespace ? "  " : string.Empty);
                }

                if (hasNamespace)
                {
                    lines.Add(string.Empty);
                    lines.Add("  return {");

                    var firstElement = true;
                    foreach (var element in _elements)
                    {
                        if (!firstElement)
                            lines[lines.Count - 1] = lines[lines.Count - 1] + ",";

                        if (element.IsPublic && !string.IsNullOrEmpty(element.Name))
                        {
                            lines.Add("    " + element.Name);
                            firstElement = false;
                        }
                    }

                    lines.Add("  }");
                    lines.Add("}();");
                    lines.Add(string.Empty);
                }
            }
        }
    }
}
