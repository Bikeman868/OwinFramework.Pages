using System;
using System.Collections;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class BuiltRegion : Region
    {
        public Action<IHtmlWriter> WriteOpen;
        public Action<IHtmlWriter> WriteClose;
        public Action<IHtmlWriter> WriteChildOpen;
        public Action<IHtmlWriter> WriteChildClose;

        private Type _repeatType;
        private Type _listType;

        public string RepeatScope { get; set; }

        public Type RepeatType
        {
            get { return _repeatType; }
            set 
            {
                _repeatType = value;
                _listType = typeof(IList<>).MakeGenericType(value);
            }
        }

        public BuiltRegion(IRegionDependenciesFactory regionDependenciesFactory)
            : base(regionDependenciesFactory)
        {
            WriteOpen = w => { };
            WriteClose = w => { };
            WriteChildOpen = w => { };
            WriteChildClose = w => { };
        }

        public override IWriteResult WriteHtml(
            IRenderContext context,
            IElement content)
        {
            var result = WriteResult.Continue();
            WriteOpen(context.Html);

            if (content != null)
            {
                if (_repeatType == null)
                {
                    result.Add(content.WriteHtml(context));
                }
                else
                {
                    var list = (IEnumerable)context.Data.Get(_listType, RepeatScope);
                    var data = context.Data;
                    var childData = data.CreateChild();
                    try
                    {
                        context.Data = childData;
                        foreach (var item in list)
                        {
                            childData.Set(_repeatType, item);
                            WriteChildOpen(context.Html);
                            result.Add(content.WriteHtml(context));
                            WriteChildClose(context.Html);
                        }
                    }
                    finally
                    {
                        context.Data = data;
                        childData.Dispose();
                    }
                }
            }

            WriteClose(context.Html);
            return result;
        }
    }
}
