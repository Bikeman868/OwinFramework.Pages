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
    [IsComponent("math_form")]
    public class MathFormComponent: Component
    {
        public MathFormComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
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

                html.WriteOpenTag("form", "action", "/math/add", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:15px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "9");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:15px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "5");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "submit", "value", "Add");

                html.WriteCloseTag("form");
                html.WriteUnclosedElement("hr");

                // Subtract form

                html.WriteOpenTag("form", "action", "/math/subtract", "method", "POST");

                html.WriteElement("label", "A", "for", "a");
                html.WriteElement("span", "&nbsp;", "style", "width:15px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "a", "value", "15");
                html.WriteUnclosedElement("br");

                html.WriteElement("label", "B", "for", "b");
                html.WriteElement("span", "&nbsp;", "style", "width:15px;");
                html.WriteUnclosedElement("input", "type", "input", "name", "b", "value", "6");
                html.WriteUnclosedElement("br");

                html.WriteUnclosedElement("input", "type", "submit", "value", "Subtract");

                html.WriteCloseTag("form");

                var result = context.OwinContext.Get<ArithmeticResult>(typeof(ArithmeticResult).FullName);
                if (result != null)
                {
                    html.WriteUnclosedElement("br");
                    html.WriteElement("div", result.Result);
                }
            }
            return WriteResult.Continue();
        }
    }

    internal class ArithmeticResult
    {
        public string Result;
    }


    [IsService("arithmetic", "/math/", new[] { Method.Post, Method.Get })]
    public class ArithmeticService
    {
        [Endpoint(UrlPath = "add/{a}/{b}")]
        [EndpointParameter("a", typeof(double), EndpointParameterType.PathSegment)]
        [EndpointParameter("b", typeof(double), EndpointParameterType.PathSegment)]
        public void Add1(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a + b);
        }

        [Endpoint(UrlPath = "add", Analytics = AnalyticsLevel.Full)]
        [EndpointParameter("a", typeof(AnyValue<double>), EndpointParameterType.FormField | EndpointParameterType.QueryString)]
        [EndpointParameter("b", typeof(AnyValue<double>), EndpointParameterType.FormField | EndpointParameterType.QueryString)]
        [Description("Adds two numbers and returns the sum")]
        public void Add2(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " + " + b + " = " + (a + b) });
            request.Rewrite("/page3");
        }

        [Endpoint]
        [EndpointParameter(
            "a", typeof(AnyValue<double>), 
            EndpointParameterType.FormField | EndpointParameterType.QueryString,
            Description = "The number to subtract from")]
        [EndpointParameter(
            "b", typeof(AnyValue<double>), 
            EndpointParameterType.FormField | EndpointParameterType.QueryString,
            Description = "The amount to subtract from a")]
        [Description("Calculates a-b and returns the result")]
        [Example("http://myservice.com/subtract?a=12&b=6")]
        public void Subtract(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");

            request.OwinContext.Set(
                typeof(ArithmeticResult).FullName,
                new ArithmeticResult { Result = a + " - " + b + " = " + (a - b) });
            request.Rewrite("/page3");
        }

        [Endpoint(Analytics = AnalyticsLevel.None)]
        [EndpointParameter("a", typeof(AnyValue<double>))]
        [EndpointParameter("b", typeof(AnyValue<double>))]
        public void Multiply(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a * b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(AnyValue<double>))]
        [EndpointParameter("b", typeof(NonZeroValue<double>))]
        public void Divide(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a / b);
        }
    }
}