using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace BlogRss
{
    /// <summary>
    /// RSS manager to read RSS feeds
    /// </summary>
    public static class RssReader
    {
        public static List<FeedItem> ProcessEntireFeed(String url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            using (var response = request.GetResponse())
            {
                XNamespace ns = "http://www.w3.org/2005/Atom"; 
                var xmlDoc = XDocument.Load(response.GetResponseStream());

                // This seems really inefficient, I haven't worked out a way to quickly determine if the format style of a blog feed
                // For now just attempt to get data as specific supported types
                var posts = ProcessAsWordpress(xmlDoc);
                if (!posts.Any())
                {
                    posts = ProcessAsFunnelWeb(xmlDoc, ns);
                }

                // the reason the above probing was required is to ensure 
                // the necessary values for the creation of the FeedItem
                // are all there, and that no further possibly flawed reconstruction
                // of the post details is required

                var dateOptions = new[] {"pubDate", "published"};
                var linkOptions = new[] {"feedburner:origLink", "link", "id"};
                var blogContentOptions = new[] { "description", "content" };
                var constructedItems =
                    from q in posts
                    select new FeedItem
                               {
                                   Title = q.AttemptToGetElement("title", ns),
                                   PublishDate = q.AttemptToGetElementsFromChoices(dateOptions, ns).AsDate(),
                                   Url = q.AttemptToGetElementsFromChoices(linkOptions, ns),
                                   //Summary = q.AttemptToGetElement("summary"),
                                   //PostText = q.AttemptToGetElementsFromChoices(blogContentOptions)
                               };

                return constructedItems.ToList();
            }
        }

        /// <summary>
        /// Returns XElements based on the Wordpress/Feedburner style of atom feed
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static List<XElement> ProcessAsWordpress(XDocument xmlDoc)
        {
            return xmlDoc.Descendants(
                    XName.Get("item")).Where(x =>
                                             x.Element(XName.Get("pubDate")) != null
                                             && x.Element(XName.Get("title")) != null
                                             && x.Element(XName.Get("link")) != null
                    ).ToList();
        }

        /// <summary>
        /// Returns XElements based on the FunnelWeb style of atom feed
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static List<XElement> ProcessAsFunnelWeb(XDocument xmlDoc, XNamespace ns)
        {
            return xmlDoc.Descendants(ns + "entry").Where(x =>
                                             x.Element(ns + "published") != null
                                             && x.Element(ns + "id") != null
                                             && x.Element(ns + "title") != null
                    ).ToList();
        }
    }

    public static class XElementExtensions
    {
        public static string AttemptToGetElement(this XElement xElement, string node, XNamespace ns)
        {
            if (node.Contains(":"))
            {
                //TODO: work out a way to get this from the feed
                //NOTE: right now only supports elements on the feedburner namespace
                var xmlnamespace = XNamespace.Get(@"http://rssnamespace.org/feedburner/ext/1.0");

                var actualNode = node.Substring(node.IndexOf(':') + 1, node.Length - node.IndexOf(':') - 1);

                return xElement.Element(XName.Get(actualNode, xmlnamespace.NamespaceName)) != null
                           ? xElement.Element(XName.Get(actualNode, xmlnamespace.NamespaceName)).Value
                           : "";
            }

            var regular = xElement.Element(XName.Get(node)) != null ? xElement.Element(XName.Get(node)).Value : "";
            if (String.IsNullOrWhiteSpace(regular))
                regular = xElement.Element(ns + node) != null ? xElement.Element(ns + node).Value : "";

            return regular;
        }

        public static String AttemptToGetElementsFromChoices(this XElement xElement, IEnumerable<String> nodes, XNamespace ns)
        {
            var elementValue = "";
            foreach (var result in nodes.ToList().Select(n => AttemptToGetElement(xElement, n, ns)).Where(result => !String.IsNullOrWhiteSpace(result)))
            {
                elementValue = result;
                break;
            }
            return elementValue;
        }

        public static DateTime AsDate(this String dateText)
        {
            DateTime dateResult;
            
            DateTime.TryParse(dateText, out dateResult);

            return dateResult;
        }
    }
}
