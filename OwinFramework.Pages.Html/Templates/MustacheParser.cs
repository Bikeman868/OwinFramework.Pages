using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that replaces data binding expressions
    /// enclosed in double { with data obtained from the render context.
    /// The data binding expressions are like this {{type:field}} for
    /// example {{Person:FirstName}}.
    /// 
    /// Type names can be fully qualified, or you can declare referencecs
    /// to them at the start of you template. See examples.
    /// You can also access properties of contained objects by adding
    /// periods like this {{Person:Address.City}}
    /// 
    /// You can also use the standard Mustache # and / syntax to repeat 
    /// output for each element in a list.
    /// </summary>
    /// <example>
    ///   {{Person=MyApp.DataModels.IPerson}}
    ///   <h1>{{Person:LastName}}</h1>
    ///   <p>Hi {{Person:FirstName}},</p>
    ///   <p>Welcome to my wonderful app</p>
    /// </example>
    /// <example>
    ///   <h1>{{MyApp.DataModels.IPerson:LastName}}</h1>
    ///   <p>Hi {{MyApp.DataModels.IPerson:FirstName}},</p>
    ///   <p>Welcome to my wonderful app</p>
    /// </example>
    /// <example>
    ///   <svg width="50" height="200">
    ///     <g transform="translate(40,200) rotate(-90)">
    ///       <text class="title">{{Person:Title}} {{Person:LastName}}</text>
    ///     </g>
    ///   </svg>
    /// </example>
    /// <example>
    ///   {{Address=MyApp.DataModels.IAddress}}
    ///   {{#Address}}<p>{{Street}},{{City}},{{Zip}}</p>{{/Address}}
    /// </example>
    public class MustacheParser : ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly Regex _mustacheRegex;

        public MustacheParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            _mustacheRegex = new Regex(
                @"{{([a-zA-Z0-9=+:.#/]+)}}", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var text = encoding.GetString(template);

            var templateDefinition = _templateBuilder.BuildUpTemplate().PartOf(package);
            ParseMustache(templateDefinition, text);
            return templateDefinition.Build();
        }

        private void ParseMustache(ITemplateDefinition template, string text)
        {
            var matches = _mustacheRegex.Matches(text);
            if (matches == null || matches.Count == 0)
            {
                template.AddHtml(text);
                return;
            }

            var types = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            
            var pos = 0;
            foreach(Match match in matches)
            {
                if (match.Index > pos)
                    template.AddHtml(text.Substring(pos, match.Index - pos));

                pos = match.Index + match.Length;

                var mustache = match.Groups[1].Value;

                if (TryParseAlias(mustache, types)) continue;
                if (TryParseStartRepeat(mustache, template, types)) continue;
                if (TryParseEndRepeat(mustache, template, types)) continue;
                if (TryParseField(mustache, template, types)) continue;
            }

            if (pos < text.Length)
                template.AddHtml(text.Substring(pos));
        }

        private bool TryParseAlias(string mustache, Dictionary<string, Type> types)
        {
            var equalsPos = mustache.IndexOf('=');
            if (equalsPos < 0) return false;
             
            if (equalsPos == 0)
                throw new TemplateBuilderException(
                    "Mustache expressions in templates can not begin with equals. Your template contains '{{" + mustache + "}}'");

            var alias = mustache.Substring(0, equalsPos);
            var typeName = mustache.Substring(equalsPos + 1);
            var type = ResolveTypeName(typeName);
            types[alias] = type;
            return true;
        }

        private bool TryParseStartRepeat(string mustache, ITemplateDefinition template, Dictionary<string, Type> types)
        {
            if (mustache.Length == 0 || mustache[0] != '#') return false;

            var indexOfColon = mustache.IndexOf(':');

            var typeName = indexOfColon < 0 ? mustache.Substring(1) : mustache.Substring(1, indexOfColon - 1);
            var scopeName = indexOfColon < 0 ? null : mustache.Substring(indexOfColon + 1);

            var type = types.ContainsKey(typeName)
                ? types[typeName]
                : ResolveTypeName(typeName);

            template.RepeatStart(type, scopeName);

            return true;
        }

        private bool TryParseEndRepeat(string mustache, ITemplateDefinition template, Dictionary<string, Type> types)
        {
            if (mustache.Length == 0 || mustache[0] != '/') return false;

            template.RepeatEnd();

            return true;
        }

        private bool TryParseField(string mustache, ITemplateDefinition template, Dictionary<string, Type> types)
        {
            var colonPos = mustache.IndexOf(':');
            if (colonPos < 0) return false;

            if (colonPos == 0)
                throw new TemplateBuilderException(
                    "Mustache expressions in templates can not begin with colon. Your template contains '{{" + mustache + "}}'");

            var typeName = mustache.Substring(0, colonPos);
            var fieldName = mustache.Substring(colonPos + 1);

            var type = types.ContainsKey(typeName) 
                ? types[typeName] 
                : ResolveTypeName(typeName);

            template.AddDataField(type, fieldName);

            return true;
        }

        private Type ResolveTypeName(string typeName)
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null) return type;

            var possibleTypes = new List<Type>();
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    possibleTypes.AddRange(assembly
                        .GetTypes()
                        .Where(t => t.FullName.EndsWith(typeName, StringComparison.InvariantCultureIgnoreCase)));
                }
                catch
                {}
            }

            if (possibleTypes.Count == 0)
                throw new TemplateBuilderException(
                    "Unkown type name in mustache template '" + typeName + "'. See documentation for Type.GetType() for more details");

            type = possibleTypes[0];
            for (var i = 1; i < possibleTypes.Count; i++)
            {
                if (possibleTypes[i].FullName.Length < type.FullName.Length)
                    type = possibleTypes[i];
            }

            return type;
        }
    }
}
