using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers.Analysable;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class ServiceEndpoint : IRunable, IAnalysable
    {
        public readonly string Path;
        public readonly MethodInfo MethodInfo;

        ElementType INamed.ElementType { get { return ElementType.Service; } }
        string INamed.Name { get; set; }

        IPackage IPackagable.Package { get; set; }

        string IRunable.RequiredPermission { get; set; }
        string IRunable.SecureResource { get; set; }
        bool IRunable.AllowAnonymous { get; set; }
        string IRunable.CacheCategory { get; set; }
        Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }
        CachePriority IRunable.CachePriority { get; set; }

        public IRequestDeserializer RequestDeserializer { get; set; }
        public IResponseSerializer ResponseSerializer { get; set; }

        private EndpointParameter[] _parameters;

        private readonly Action<IEndpointRequest> _method;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IRequestRouter _requestRouter;
        private readonly string[] _pathElements;

        public ServiceEndpoint(
            string path, 
            Action<IEndpointRequest> method,
            MethodInfo methodInfo,
            AnalyticsLevel analytics,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory,
            IRequestRouter requestRouter)
        {
            Path = path;
            MethodInfo = methodInfo;
            _method = method;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _requestRouter = requestRouter;
            _pathElements = path
                .Split('/')
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();
            AddStatistics(analytics, path);
        }

        public void AddParameter(string name, EndpointParameterType parameterType, IParameterParser parser)
        {
            var functions = new List<Func<IEndpointRequest, string, string>>();

            if (parameterType.HasFlag(EndpointParameterType.QueryString))
                functions.Add(QueryStringParam);

            if (parameterType.HasFlag(EndpointParameterType.Header))
                functions.Add(HeaderParam);

            if (parameterType.HasFlag(EndpointParameterType.PathSegment))
            {
                var placeholder = "{" + name + "}";
                for (var i = 0; i < _pathElements.Length; i++)
                {
                    var index = i;
                    if (string.Equals(placeholder, _pathElements[i], StringComparison.OrdinalIgnoreCase))
                    {
                        functions.Add((r, n) => PathSegmentParam(r, index));
                        break;
                    }
                }
            }

            if (parameterType.HasFlag(EndpointParameterType.FormField))
                functions.Add(FormFieldParam);

            var parameter = new EndpointParameter 
            { 
                Name = name, 
                ParameterType = parameterType, 
                Parser = parser,
                Functions = functions.ToArray()
            };

            if (_parameters == null)
            {
                _parameters = new[] { parameter };
            }
            else
            {
                var parameters = _parameters.ToList();
                parameters.Add(parameter);
                _parameters = parameters.ToArray();
            }
        }

        private string QueryStringParam(IEndpointRequest request, string parameterName)
        {
            return request.OwinContext.Request.Query[parameterName];
        }

        private string HeaderParam(IEndpointRequest request, string parameterName)
        {
            return request.OwinContext.Request.Headers[parameterName];
        }

        private string PathSegmentParam(IEndpointRequest request, int pathIndex)
        {
            return request.PathSegment(pathIndex);
        }

        private string FormFieldParam(IEndpointRequest request, string parameterName)
        {
            var form = request.Form;
            if (form == null) return null;

            var values = form.GetValues(parameterName);
            if (values == null || values.Count == 0)
                return null;

            return values[0];
        }

        Task IRunable.Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            trace(context, () => "Executing service endpoint " + Path);

            using (var request = new EndpointRequest(
                trace,
                _requestRouter,
                context, 
                _dataCatalog,
                _dataDependencyFactory,
                RequestDeserializer, 
                ResponseSerializer,
                _parameters))
            {
                try
                {
                    try
                    {
                        _requestCount++;
                        long startTime = _millisecondsPerRequest == null ? 0 : TimeNow;

                        _method(request);

                        if (_millisecondsPerRequest != null)
                            _millisecondsPerRequest.Record(ElapsedMilliseconds(startTime));

                        if (_requestsPerMinute != null)
                            _requestsPerMinute.Record();
                    }
                    catch (TargetInvocationException e)
                    {
                        _failCount++;
                        trace(context, () => "Service endpoint threw an exception");
                        throw e.InnerException;
                    }
                }
                catch (NotImplementedException e)
                {
                    trace(context, () => 
                        "Not Implemented exception: " + 
                        e.Message + (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.NotImplemented, "Not implemented yet");
                }
                catch (AggregateException ex)
                {
                    trace(context, () => "Multiple exceptions...");
                    foreach(var e in ex.InnerExceptions)
                    {
                        trace(context, () => 
                            e.GetType().DisplayName() + " exception: " + e.Message + 
                            (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    }
                    request.HttpStatus(HttpStatusCode.InternalServerError, "Multiple exceptions");
                }
                catch (EndpointParameterException e)
                {
                    trace(context, () =>
                        "Invalid parameter '" + e.ParameterName + "' " + e.ValidationError +
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.BadRequest, "Parameter '" + e.ParameterName + "' is invalid. " + e.ValidationError);
                }
                catch(BodyDeserializationException e)
                {
                    trace(context, () =>
                        "Request body should by '" + e.BodyType.DisplayName() + "'. " + e.ValidationError +
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.BadRequest, "Request body is invalid. " + e.ValidationError);
                }
                catch (Exception e)
                {
                    trace(context, () => 
                        e.GetType().DisplayName() + " exception: " + e.Message + 
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.InternalServerError, "Unhandled exception " + e.Message);
                }
                return request.WriteResponse();
            }
        }

        #region High precision timing

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out Int64 performanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out Int64 frequency);

        private static readonly Int64 Frequency;

        static ServiceEndpoint()
        {
            QueryPerformanceFrequency(out Frequency);
        }

        public static long TimeNow
        {
            get
            {
                long startTime;
                QueryPerformanceCounter(out startTime);
                return startTime;
            }
        }

        private static float ElapsedMilliseconds(long startTime)
        {
            return 1000f * (TimeNow - startTime) / Frequency;            
        }

        #endregion

        #region IAnalysable

        private readonly List<IStatisticInformation> _availableStatistics = new List<IStatisticInformation>();
        private int _requestCount;
        private int _failCount;
        private CountPerMinute _requestsPerMinute;
        private AverageTime _millisecondsPerRequest;

        private class CountPerMinute : Statistic
        {
            private const int SecondsPerBucket = 5;
            private readonly int[] _buckets = new int[10 * 60 / SecondsPerBucket];

            public override IStatistic Refresh()
            {
                int totalCount;

                lock (_buckets)
                {
                    totalCount = _buckets.Aggregate(0, (s, b) => s + b);
                }

                Denominator = _buckets.Length / 60f;

                if (totalCount == 0)
                {
                    Value = 0;
                    Formatted = "No requests";
                }
                else
                {
                    Value = totalCount / Denominator;
                    Formatted = Value.ToString("g3", CultureInfo.InvariantCulture) + "/min";
                }

                return this;
            }

            public void Record()
            {
                var now = DateTime.UtcNow;
                var seconds = now.Minute * 60 + now.Second;
                var bucketIndex = (seconds / SecondsPerBucket) % _buckets.Length;

                lock (_buckets)
                {
                    _buckets[bucketIndex]++;

                    bucketIndex++;
                    if (bucketIndex >= _buckets.Length) bucketIndex = 0;

                    _buckets[bucketIndex] = 0;
                }
            }
        }

        private class AverageTime : Statistic
        {
            private const int SecondsPerBucket = 5;
            private readonly BucketEntry[] _buckets = new BucketEntry[10 * 60 / SecondsPerBucket];

            private struct BucketEntry
            {
                public int Count;
                public float Milliseconds;
            }

            public override IStatistic Refresh()
            {
                float totalMilliseconds;
                int totalCount;

                lock (_buckets)
                {
                    totalMilliseconds = _buckets.Aggregate(0f, (s, b) => s + b.Milliseconds);
                    totalCount = _buckets.Aggregate(0, (s, b) => s + b.Count);
                }

                Denominator = totalCount;

                if (totalCount == 0)
                {
                    Value = 0;
                    Formatted = "No requests";
                }
                else
                {
                    Value = totalMilliseconds / totalCount;
                    Formatted = Value.ToString("g3", CultureInfo.InvariantCulture) + "ms";
                }

                return this;
            }

            public void Record(float elapsedMilliseconds)
            {
                var now = DateTime.UtcNow;
                var seconds = now.Minute * 60 + now.Second;
                var bucketIndex = (seconds / SecondsPerBucket) % _buckets.Length;

                lock (_buckets)
                {
                    _buckets[bucketIndex].Count++;
                    _buckets[bucketIndex].Milliseconds += elapsedMilliseconds;

                    bucketIndex++;
                    if (bucketIndex >= _buckets.Length) bucketIndex = 0;

                    _buckets[bucketIndex].Count = 0;
                    _buckets[bucketIndex].Milliseconds = 0;
                }
            }
        }

        private void AddStatistics(AnalyticsLevel level, string path)
        {
            var baseId = path + "+";

            if (level == AnalyticsLevel.Basic || level == AnalyticsLevel.Full)
            {
                _availableStatistics.Add(
                    new StatisticInformation
                    {
                        Id = baseId + "RequestCount",
                        Name = "Requests to " + path,
                        Description = "The total number of requests to " + path + " since the service was started"
                    });
                _availableStatistics.Add(
                    new StatisticInformation
                    {
                        Id = baseId + "FailCount",
                        Name = "Failed requests to " + path,
                        Description = "The number of requests to " + path + " that failed since the service was started"
                    });
            }
            if (level == AnalyticsLevel.Full)
            {
                _requestsPerMinute = new CountPerMinute();
                _availableStatistics.Add(
                    new StatisticInformation
                    {
                        Id = baseId + "RequestRate",
                        Name = "Requests per minute to " + path,
                        Description = "The number of successful requests per minute made to " + path + " over the last 10 minutes"
                    });
            }
            if (level == AnalyticsLevel.Full)
            {
                _millisecondsPerRequest = new AverageTime();
                _availableStatistics.Add(
                    new StatisticInformation
                    {
                        Id = baseId + "RequestTime",
                        Name = "Average execution time for " + path,
                        Description = "The average time taken to execute successful requests to " + path + " in the last 10 minutes"
                    });
            }
        }

        public IList<IStatisticInformation> AvailableStatistics
        {
            get { return _availableStatistics; }
        }

        public IStatistic GetStatistic(string id)
        {
            if (id.EndsWith("RequestCount"))
                return new IntStatistic(() => _requestCount);

            if (id.EndsWith("FailCount"))
                return new IntStatistic(() => _failCount);

            if (id.EndsWith("RequestRate"))
                return _requestsPerMinute;

            if (id.EndsWith("RequestTime"))
                return _millisecondsPerRequest;

            return null;
        }

        #endregion
    }
}