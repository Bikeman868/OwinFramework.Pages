using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace Sample1.SampleServices
{
    /// <summary>
    /// This class demonstrates that a class can be both a component
    /// and a servcie. In this case the component renders forms with
    /// a submit buttons and it handles the POST.
    /// </summary>
    [IsComponent("math_form")]
    [IsService("arithmetic", "/page3/", new[] { Method.Post })]
    public class MathFormComponent: Component
    {
        public MathFormComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        [Endpoint(Analytics = AnalyticsLevel.Full)]
        [EndpointParameter(
            "a", typeof(AnyValue<double>), 
            EndpointParameterType.FormField,
            Description = "The first number to add")]
        [EndpointParameter(
            "b", typeof(AnyValue<double>), 
            EndpointParameterType.FormField,
            Description = "The second number to add")]
        [EndpointParameter(
            "r", typeof(AnyValue<string>), 
            EndpointParameterType.FormField,
            Description = "The path of the page to return the results on")]
        [Description("Adds two numbers and returns the sum")]
        public void Add(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            var r = request.Parameter<string>("r");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " + " + b + " = " + (a + b) });
            request.Rewrite(r);
        }

        [Endpoint]
        [EndpointParameter(
            "a", typeof(AnyValue<double>), 
            EndpointParameterType.FormField,
            Description = "The number to subtract from")]
        [EndpointParameter(
            "b", typeof(AnyValue<double>), 
            EndpointParameterType.FormField,
            Description = "The amount to subtract from a")]
        [EndpointParameter(
            "r", typeof(AnyValue<string>),
            EndpointParameterType.FormField,
            Description = "The path of the page to return the results on")]
        [Description("Calculates a-b and returns the result")]
        public void Subtract(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            var r = request.Parameter<string>("r");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " - " + b + " = " + (a - b) });
            request.Rewrite(r);
        }

        [Endpoint(Analytics = AnalyticsLevel.None)]
        [EndpointParameter(
            "a", typeof(AnyValue<double>),
            EndpointParameterType.FormField,
            Description = "The first number to multiply")]
        [EndpointParameter(
            "b", typeof(AnyValue<double>),
            EndpointParameterType.FormField,
            Description = "The second number to multiply")]
        [EndpointParameter(
            "r", typeof(AnyValue<string>),
            EndpointParameterType.FormField,
            Description = "The path of the page to return the results on")]
        [Description("Multiplies two numbers together and returns the product")]
        public void Multiply(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            var r = request.Parameter<string>("r");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " x " + b + " = " + (a * b) });
            request.Rewrite(r);
        }

        [Endpoint]
        [EndpointParameter(
            "a", typeof(AnyValue<double>),
            EndpointParameterType.FormField,
            Description = "The numerator")]
        [EndpointParameter(
            "b", typeof(AnyValue<double>),
            EndpointParameterType.FormField,
            Description = "The denominator")]
        [EndpointParameter(
            "r", typeof(AnyValue<string>),
            EndpointParameterType.FormField,
            Description = "The path of the page to return the results on")]
        [Description("Divides the numerator by the denominator and returns the result")]
        public void Divide(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            var r = request.Parameter<string>("r");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " / " + b + " = " + (a / b) });
            request.Rewrite(r);
        }

        public override System.Collections.Generic.IEnumerable<PageArea> GetPageAreas()
        {
            return new[] { PageArea.Body };
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                var html = context.Html;

                // Add form

                html.WriteOpenTag("form", "action", "/page3/add", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "9");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "5");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "hidden", "name", "r", "value", "/page3");
                html.WriteUnclosedElement("input", "type", "submit", "value", "Add");

                html.WriteCloseTag("form");
                html.WriteUnclosedElement("hr");

                // Subtract form

                html.WriteOpenTag("form", "action", "/page3/subtract", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "15");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "6");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "hidden", "name", "r", "value", "/page3");
                html.WriteUnclosedElement("input", "type", "submit", "value", "Subtract");

                html.WriteCloseTag("form");
                html.WriteUnclosedElement("hr");

                // Multiply form

                html.WriteOpenTag("form", "action", "/page3/multiply", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "24");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "18");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "hidden", "name", "r", "value", "/page3");
                html.WriteUnclosedElement("input", "type", "submit", "value", "Multiply");

                html.WriteCloseTag("form");
                html.WriteUnclosedElement("hr");

                // Divide form

                html.WriteOpenTag("form", "action", "/page3/divide", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "23");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:20px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "7");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "hidden", "name", "r", "value", "/page3");
                html.WriteUnclosedElement("input", "type", "submit", "value", "Divide");

                html.WriteCloseTag("form");
                html.WriteUnclosedElement("hr");

                var result = context.OwinContext.Get<ArithmeticResult>(typeof(ArithmeticResult).FullName);
                if (result != null)
                {
                    html.WriteUnclosedElement("br");
                    html.WriteElement("div", result.Result);
                }
            }
            return WriteResult.Continue();
        }

        private class ArithmeticResult
        {
            public string Result;
        }

    }
}