using System;

namespace BlogRss
{
    /// <summary>
    /// RSS feed item entity
    /// </summary>
    public class FeedItem
    {
        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the link
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the item id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Gets or sets the publish date
        /// </summary>
        public DateTime PublishDate { get; set; }
        public string PublishDateText { get; set; }
        public string Summary { get; set; }
        public string PostText { get; set; }

        /// <summary>
        /// Gets or sets the channel id
        /// </summary>
        public int ChannelId { get; set; }
    }
}