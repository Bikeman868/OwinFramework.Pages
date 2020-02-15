using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    // TODO: Add minification. https://stackoverflow.com/questions/8567070/is-there-a-net-library-for-minifying-javascript
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

        public IJavascriptWriter WriteLineRaw(string line, IPackage package)
        {
            var javascriptNamespace = GetNamespace(package);
            javascriptNamespace.Add(
                new RawTextElement
                {
                    Text = line
                });
            return this;
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

        public IJavascriptWriter WriteClass(string className, string classBody, IPackage package, bool isPublic)
        {
            var javascriptNamespace = GetNamespace(package);
            javascriptNamespace.Add(
                new ClassElement
                {
                    Name = className,
                    IsPublic = isPublic,
                    Body = classBody
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

        private class RawTextElement: JavascriptElement
        {
            public string Text;

            public override void Write(IHtmlWriter writer)
            {
                writer.Write(Text);
                writer.WriteLine();
            }

            public override void Write(IStringBuilder stringBuilder, string indent)
            {
                stringBuilder.Append(indent);
                stringBuilder.AppendLine(Text);
            }

            public override void Write(IList<string> lines, string indent)
            {
                lines.Add(Text);
            }
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

        private class ClassElement : JavascriptElement
        {
            public string Body;

            public override void Write(IHtmlWriter writer)
            {
                if (string.IsNullOrEmpty(Name))
                    throw new Exception("You can not add a JavaScript class with no name");

                writer.Write("var ");
                writer.Write(Name);
                writer.WriteLine(" = function () {");
                writer.IndentLevel++;

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                    writer.WriteLine(line);

                writer.IndentLevel--;
                writer.WriteLine("}();");
            }

            public override void Write(IStringBuilder stringBuilder, string indent)
            {
                if (string.IsNullOrEmpty(Name))
                    throw new Exception("You can not add a JavaScript class with no name");

                if (string.IsNullOrEmpty(Body))
                    throw new Exception("You can not add a JavaScript class with no body");

                stringBuilder.Append(indent);

                stringBuilder.Append("var ");
                stringBuilder.Append(Name);
                stringBuilder.AppendLine(" = function () {");

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                {
                    stringBuilder.Append(indent);
                    stringBuilder.Append("  ");
                    stringBuilder.AppendLine(line);
                }

                stringBuilder.Append(indent);
                stringBuilder.AppendLine("}();");
            }

            public override void Write(IList<string> lines, string indent)
            {
                if (string.IsNullOrEmpty(Name))
                    throw new Exception("You can not add a JavaScript class with no name");

                lines.Add(indent + "var " + Name + " = function () {");

                foreach (var line in Body.Replace("\r", "").Split('\n'))
                    lines.Add(indent + "  " + line);

                lines.Add(indent + "}();");
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
                    writer.Write(Line1());
                    writer.Write(Line2());
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

                    foreach (var element in _elements.Where(e => e.IsPublic && !string.IsNullOrEmpty(e.Name)))
                        writer.WriteLine("exported." + element.Name + " = " + element.Name + ";");

                    writer.IndentLevel--;
                    writer.WriteLine(LineN2());
                    writer.WriteLine(LineN1());
                    writer.WriteLine();
                }
            }

            public void Write(IStringBuilder stringBuilder)
            {
                if (_elements == null) return;

                var hasNamespace = !string.IsNullOrEmpty(NamespaceName);

                if (hasNamespace)
                {
                    stringBuilder.AppendLine(Line1());
                    stringBuilder.AppendLine(Line2());
                }

                foreach (var element in _elements)
                {
                    stringBuilder.AppendLine(string.Empty);
                    element.Write(stringBuilder, hasNamespace ? "  " : string.Empty);
                }

                if (hasNamespace)
                {
                    stringBuilder.AppendLine(string.Empty);

                    foreach (var element in _elements.Where(e => e.IsPublic && !string.IsNullOrEmpty(e.Name)))
                            stringBuilder.AppendLine("  exported." + element.Name + " = " + element.Name + ";");

                    stringBuilder.AppendLine(LineN2());
                    stringBuilder.AppendLine(LineN1());
                    stringBuilder.AppendLine(string.Empty);
                }
            }

            public void Write(IList<string> lines)
            {
                if (_elements == null) return;

                var hasNamespace = !string.IsNullOrEmpty(NamespaceName);

                if (hasNamespace)
                {
                    lines.Add(Line1());
                    lines.Add(Line2());
                }

                foreach (var element in _elements)
                {
                    element.Write(lines, hasNamespace ? "  " : string.Empty);
                }

                if (hasNamespace)
                {
                    lines.Add(string.Empty);

                    foreach (var element in _elements.Where(e => e.IsPublic && !string.IsNullOrEmpty(e.Name)))
                        lines.Add("  exported." + element.Name + " = " + element.Name + ";");

                    lines.Add(LineN2());
                    lines.Add(LineN1());
                    lines.Add(string.Empty);
                }
            }

            private string Line1()
            {
                return "var ns = (window.ns = window.ns || {});";
            }

            private string Line2()
            {
                return "ns." + NamespaceName + " = function (exported) {";
            }

            private string LineN2()
            {
                return "  return exported;";
            }

            private string LineN1()
            {
                return "}(ns." + NamespaceName + " || {});";
            }
        }
    }
}
