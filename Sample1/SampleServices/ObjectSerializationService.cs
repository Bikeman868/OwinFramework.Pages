using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using OwinFramework.Pages.Restful.Runtime;

namespace Sample1.SampleServices
{
    [IsService("objects", "/object/", new[] { Method.Get, Method.Post })]
    public class ObjectSerializationService : Service
    {
        public ObjectSerializationService(
            IServiceDependenciesFactory serviceDependenciesFactory) 
            : base(serviceDependenciesFactory)
        {
        }

        [Endpoint(MethodsToRoute = new[] { Method.Get, Method.Post })]
        [EndpointParameter("which", typeof(AnyValue<Which>))]
        [Description("This is a test of object serialization")]
        public void New(IEndpointRequest request)
        {
            var which = request.Parameter<Which>("which");
            TestPerson person;
            switch (which)
            {
                case Which.One:
                    person = request.Body<TestPerson>();
                    break;
                case Which.Two:
                    person = new TestPerson { FirstName = "John", LastName = "Doe" };
                    break;
                case Which.Three:
                    person = new TestPerson { FirstName = "Jane", LastName = "Doe" };
                    break;
                default:
                    request.NoContent();
                    return;
            }

            person.FullName = person.FirstName + " " + person.LastName;
            request.Success(person);
        }
    }

    public enum Which
    {
        One,
        Two,
        Three
    }

    public class TestPerson
    {
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string FullName { get; set; } 
    }
}