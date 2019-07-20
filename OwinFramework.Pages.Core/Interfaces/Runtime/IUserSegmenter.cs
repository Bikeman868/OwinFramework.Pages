using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A singleton user segmentation algorithm for A/B testing. This would normally
    /// be overriden in an application to provide a custom segmentation of users
    /// </summary>
    public interface IUserSegmenter
    {
        /// <summary>
        /// Returns a list of possible segments so that the person configuring the
        /// A/B test knows what to configure
        /// </summary>
        UserSegment[] GetSegments();

        /// <summary>
        /// Determines the segment that the requesting user belongs to for A/B testing
        /// </summary>
        /// <param name="owinContext">The request to use for segmentation</param>
        /// <returns></returns>
        string GetSegment(IOwinContext owinContext);
    }

    /// <summary>
    /// POCO class that defines a segment of the users to the website
    /// </summary>
    public class UserSegment
    {
        /// <summary>
        /// The name of the segment. These names should be returned for each request
        /// to the website to indicate which version of the website to show to this
        /// user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An HTML formatted description of this segment of users
        /// </summary>
        public string Description { get; set; }
    }
}
