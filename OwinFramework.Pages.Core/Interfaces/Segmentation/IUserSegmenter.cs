using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Segmentation
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
        /// <returns>An index into the UserSegment array for this request or null
        /// if this segment of users is not part of the A/B test</returns>
        int? GetSegmentIndex(IOwinContext owinContext);
    }

    /// <summary>
    /// POCO class that defines a segment of the users to the website
    /// </summary>
    public class UserSegment
    {
        /// <summary>
        /// The name of the segment. These names are used to annotate charts that show
        /// how segments compared in the A/B tests, and are also chosen in the UI that
        /// is used to set up the test and define how the UX for each segment of users.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An HTML formatted description of this segment of users
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This must be a valid CSS and JavaScript identifer that can be appended
        /// to names of assets to differentiate the segments. For example if we have
        /// a different css class name for each segment of users then this suffix will
        /// be appended to the css class name to create the css class name specific to
        /// this segment. Keys must also be unique and are used to identify changes
        /// that are applied to the website for different user segments
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// This must be set to the index position of this user segment within the 
        /// user segment array
        /// </summary>
        public int Index { get; set; }
    }
}
