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
    /// enclosed in double curly braces with data obtained from the render context.
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
        private readonly MustacheMixIn _mustacheMixIn;

        public MustacheParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            _mustacheMixIn = new MustacheMixIn();
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package, IModule module)
        {
            var template = _templateBuilder
                .BuildUpTemplate()
                .PartOf(package)
                .DeployIn(module);

            foreach (var resource in resources)
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var text = encoding.GetString(resource.Content);

                _mustacheMixIn.AddToTemplate(template, text);
            }
            return template.Build();
        }
    }
}
