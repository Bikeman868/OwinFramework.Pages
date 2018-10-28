using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample1.SampleDataProviders
{
    internal class VerticalText
    {
        public string Caption { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float TextHeight { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public string Background { get; set; }
        public string TextStyle { get; set; }

        public VerticalText(string caption, float width, float height)
        {
            Caption = caption;
            Width = width;
            Height = height;
            TextHeight = height * 0.8f;
            X = width * 0.8f;
            Y = height * 0.9f;
            Background = "blue";
            TextStyle = "font: 30px serif; fill: white;";
        }
    }

    [IsDataProvider("application", typeof(VerticalText))]
    internal class VerticalTextDataProvider : DataProvider
    {
        public VerticalTextDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(new VerticalText("Vertical", 60, 160));
        }
    }
}