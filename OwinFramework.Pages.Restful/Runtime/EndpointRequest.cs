using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class EndpointRequest: IEndpointRequest, IDataContext
    {
        private readonly IOwinContext _context;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IRequestDeserializer _deserializer;
        private readonly IResponseSerializer _serializer;

        private Func<Task> _writeResponse;

        private bool _bodyDeserialized;
        private object _body;

        public EndpointRequest(
            IOwinContext context,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory,
            IRequestDeserializer deserializer,
            IResponseSerializer serializer)
        {
            _context = context;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _deserializer = deserializer;
            _serializer = serializer;

            Success();
        }

        public void Dispose()
        {
        }

        public Task WriteResponse()
        {
            return _writeResponse();
        }

        public IOwinContext OwinContext
        {
            get { return _context; }
        }

        #region Request processing

        public T Data<T>(string scopeName = null)
        {
            var dataContext = (IDataContext)this;
            return dataContext.Get<T>(scopeName);
        }

        public T Body<T>()
        {
            if (!_bodyDeserialized)
            {
                _bodyDeserialized = true;
                _body = _deserializer.Body<T>(_context);
            }
            return (T)_body;
        }

        public T Parameter<T>(string name)
        {
            return default(T);
        }

        #endregion

        #region Response production

        public void Success()
        {
            _writeResponse = () => _serializer.Success(_context);
        }

        public void Success<T>(T valueToSerialize)
        {
            _writeResponse = () => _serializer.Success(_context, valueToSerialize);
        }

        public void HttpStatus(HttpStatusCode statusCode, string message)
        {
            _writeResponse = () => _serializer.HttpStatus(_context, statusCode, message);
        }

        public void NotFound(string message)
        {
            HttpStatus(HttpStatusCode.NotFound, message ?? "Not Found");
        }

        public void NoContent(string message)
        {
            HttpStatus(HttpStatusCode.NoContent, message ?? "No Content");
        }

        public void BadRequest(string message)
        {
            HttpStatus(HttpStatusCode.BadRequest, message ?? "Bad Request");
        }

        public void Redirect(Uri url, bool permenant = false)
        {
            _writeResponse = () => _serializer.Redirect(_context, url, permenant);
        }

        public void Rewrite(Uri url, Methods httpMethod = Methods.Get)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataContext

        void IDataContext.Set(Type type, object value, string scopeName, int level)
        {
            if (string.IsNullOrEmpty(scopeName))
                SetProperty(type, value);
            else
                SetProperty(type, scopeName, value);
        }

        object IDataContext.Get(Type type, string scopeName, bool required)
        {
            var retry = false;
            while (true)
            {
                object result;
                if (string.IsNullOrEmpty(scopeName))
                {
                    if (TryGetProperty(type, out result))
                        return result;
                }
                else if (TryGetProperty(type, scopeName, out result))
                    return result;

                var dependency = _dataDependencyFactory.Create(type, scopeName);

                if (retry)
                    throw new Exception("This endpoint request does not know how to find missing" +
                        " data of type " + type.DisplayName() + " because after executing the data supplier" +
                        " the data was still missing. Check your implementation of the data supplier for " +
                        dependency);

                var dataSupplier = _dataCatalog.FindSupplier(dependency);
                if (dataSupplier == null)
                    throw new Exception("You must add a data provider to your application that supplies " + dependency);

                var supply = dataSupplier.GetSupply(dependency);
                if (supply == null)
                    throw new Exception(
                        "Data supplier " + dataSupplier + " is registered as a supplier of " + dependency +
                        " but it did not return a supply of this type of data");

                supply.Supply(null, this);
                retry = true;
            }
        }

        void IDataContext.Set<T>(T value, string scopeName, int level)
        {
            var dataContext = (IDataContext)this;
            dataContext.Set(typeof(T), scopeName, level);
        }

        T IDataContext.Get<T>(string scopeName, bool required)
        {
            var dataContext = (IDataContext)this;
            return (T)dataContext.Get(typeof(T), scopeName, required);
        }

        IDataContext IDataContext.CreateChild(IDataContextBuilder dataContextBuilder)
        {
            throw new Exception("You can not create child data contexts for a service endpoint");
        }

        private readonly System.Collections.Generic.LinkedList<PropertyEntry> _properties;

        private void SetProperty(Type type, string scopeName, object value)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                SetProperty(type, value);
                return;
            }

            var existing = _properties.FirstOrDefault(
                p => p.Type == type && string.Equals(scopeName, p.ScopeName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                _properties.AddFirst(new PropertyEntry { Type = type, ScopeName = scopeName, Value = value });
            else
                existing.Value = value;
        }

        private void SetProperty(Type type, object value)
        {
            var existing = _properties.FirstOrDefault(
                p => p.Type == type && string.IsNullOrEmpty(p.ScopeName));

            if (existing == null)
                _properties.AddFirst(new PropertyEntry { Type = type, Value = value });
            else
                existing.Value = value;
        }

        private bool TryGetProperty(Type type, out object value)
        {
            PropertyEntry match = null;
            foreach (var property in _properties.Where(p => p.Type == type))
            {
                if (match == null)
                    match = property;
                else
                {
                    if (!string.IsNullOrEmpty(match.ScopeName))
                        match = property;
                }
            }

            if (match == null)
            {
                value = null;
                return false;
            }

            value = match.Value;
            return true;
        }

        private bool TryGetProperty(Type type, string scopeName, out object value)
        {
            if (string.IsNullOrEmpty(scopeName))
                return TryGetProperty(type, out value);

            var match = _properties.FirstOrDefault(
                p => p.Type == type && string.Equals(scopeName, p.ScopeName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                value = null;
                return false;
            }

            value = match.Value;
            return true;
        }

        private class PropertyEntry
        {
            public Type Type;
            public string ScopeName;
            public object Value;

            public override string ToString()
            {
                var result = Type.DisplayName(TypeExtensions.NamespaceOption.None);
                if (string.IsNullOrEmpty(ScopeName)) return result;
                return result + " in '" + ScopeName + "' scope";
            }
        }

        #endregion
    }
}