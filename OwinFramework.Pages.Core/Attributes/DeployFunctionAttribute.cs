using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to deploy a static JavaScript function with this element
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DeployFunctionAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies a static JavaScript function with this element
        /// </summary>
        /// <param name="returnType">Optional function return type, for exampple "void"</param>
        /// <param name="functionName">The name of the function</param>
        /// <param name="parameters">A comma separated list of the parameters to the function</param>
        /// <param name="body">The body of the function</param>
        public DeployFunctionAttribute(
            string returnType, 
            string functionName,
            string parameters,
            string body)
        {
            ReturnType = returnType;
            FunctionName = functionName;
            Parameters = parameters;
            Body = body;
            IsPublic = true;
        }

        /// <summary>
        /// The return type of this function
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// The name of the function
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Optional list of parameters to this function
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// The body of the function
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// True to export this function from the package namespace
        /// </summary>
        public bool IsPublic { get; set; }
    }
}
