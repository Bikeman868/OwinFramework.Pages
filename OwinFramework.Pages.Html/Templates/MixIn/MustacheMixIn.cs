using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    public class MustacheMixIn
    {
        private readonly Regex _mustacheRegex;
        private static readonly ITruthyEvaluator _truthyEvaluator = new TruthyEvaluator(true);
        private static readonly ITruthyEvaluator _notTruthyEvaluator = new TruthyEvaluator(false);

        public MustacheMixIn()
        {
            _mustacheRegex = new Regex(
                @"{{([a-zA-Z0-9=+:.#/\?\! ]+)}}", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        }

        public void AddToTemplate(ITemplateDefinition template, PageArea pageArea, string text)
        {
            var matches = _mustacheRegex.Matches(text);
            if (matches == null || matches.Count == 0)
            {
                template.AddHtml(pageArea, text);
                return;
            }

            var types = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            
            var pos = 0;
            var matchIndex = 0;
            while (pos < text.Length)
            {
                if (char.IsWhiteSpace(text[pos]))
                    pos++;
                else if (matches.Count > matchIndex && pos == matches[matchIndex].Index)
                {
                    pos = matches[matchIndex].Index + matches[matchIndex].Length;
                    matchIndex++;
                }
                else
                    break;
            }

            foreach(Match match in matches)
            {
                if (match.Index > pos)
                    template.AddHtml(pageArea, text.Substring(pos, match.Index - pos));

                if (pos <= match.Index) 
                    pos = match.Index + match.Length;

                var mustache = match.Groups[1].Value.Replace(" ", "");

                if (TryParseAlias(mustache, types)) continue;
                if (TryParseStartRepeat(mustache, template, pageArea, types)) continue;
                if (TryParseEndRepeat(mustache, template, types)) continue;
                if (TryParseField(mustache, template, pageArea, types)) continue;
            }

            if (pos < text.Length)
                template.AddHtml(pageArea, text.Substring(pos));
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

        private bool TryParseStartRepeat(string mustache, ITemplateDefinition template, PageArea pageArea, Dictionary<string, Type> types)
        {
            if (mustache.Length == 0) return false;

            RepeatType repeatType;
            switch (mustache[0])
            {
                case '#':
                    repeatType = RepeatType.RepeatList;
                    break;
                case '?':
                    repeatType = RepeatType.IfTrue;
                    break;
                case '!':
                    repeatType = RepeatType.IfNotTrue;
                    break;
                default:
                    return false;
            }

            var indexOfColon = mustache.IndexOf(':');

            var typeName = indexOfColon < 0 ? mustache.Substring(1) : mustache.Substring(1, indexOfColon - 1);
            var fieldName = indexOfColon < 0 ? null : mustache.Substring(indexOfColon + 1);

            if (repeatType == RepeatType.RepeatList && fieldName != null)
                repeatType = RepeatType.IfTrue;

            var type = types.ContainsKey(typeName)
                ? types[typeName]
                : ResolveTypeName(typeName);

            switch (repeatType)
            {
                case RepeatType.RepeatList:
                    template.RepeatStart(pageArea, type);
                    break;
                case RepeatType.IfTrue:
                    template.RepeatStart(pageArea, type, fieldName, _truthyEvaluator);
                    break;
                case RepeatType.IfNotTrue:
                    template.RepeatStart(pageArea, type, fieldName, _notTruthyEvaluator);
                    break;
            }

            return true;
        }

        private bool TryParseEndRepeat(string mustache, ITemplateDefinition template, Dictionary<string, Type> types)
        {
            if (mustache.Length == 0 || mustache[0] != '/') return false;

            template.RepeatEnd();

            return true;
        }

        private bool TryParseField(string mustache, ITemplateDefinition template, PageArea pageArea, Dictionary<string, Type> types)
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

            template.AddDataField(pageArea, type, fieldName);

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

        private class TruthyEvaluator : ITruthyEvaluator
        {
            private bool _positiveLogic;

            public TruthyEvaluator(bool positiveLogic)
            {
                _positiveLogic = positiveLogic;
            }

            public bool IsTruthy(PropertyInfo property, object value)
            {
                var propertyValue = property.GetValue(value, null);
                bool result;

                if (property.PropertyType == typeof(bool))
                    result = (bool)propertyValue;

                else if (property.PropertyType == typeof(int))
                    result = (int)propertyValue != 0;

                else if (property.PropertyType == typeof(string))
                    result = !string.IsNullOrEmpty((string)propertyValue);

                else result = propertyValue != null;

                return _positiveLogic ? result : !result;
            }
        }
    }
}
