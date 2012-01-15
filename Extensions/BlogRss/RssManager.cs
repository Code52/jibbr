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
                var xmlDoc = XDocument.Load(response.GetResponseStream());

                // This seems really inefficient, I haven't worked out a way to quickly determine if the format style of a blog feed
                // For now just attempt to get data as specific supported types
                var posts = ProcessAsWordpress(xmlDoc);
                if (!posts.Any())
                {
                    //TODO: fix this, right now it doesn't process the feed at all can't find the 'entries' hating on XML
                    posts = ProcessAsFunnelWeb(xmlDoc);
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
                                   Title = q.AttemptToGetElement("title"),
                                   PublishDate = q.AttemptToGetElementsFromChoices(dateOptions).AsDate(),
                                   Url = q.AttemptToGetElementsFromChoices(linkOptions),
                                   //Summary = q.AttemptToGetElement("summary"),
                                   //PostText = q.AttemptToGetElementsFromChoices(blogContentOptions)
                               };

                return constructedItems.ToList();
            }
        }

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
        /// TODO: fix this, it doesn't work :(
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static List<XElement> ProcessAsFunnelWeb(XDocument xmlDoc)
        {
            //debug code:
            var result = new List<XElement>();
            var nodes = xmlDoc.DescendantNodes().ToList();
            nodes.ForEach(x => { result.AddRange(x.Document.Elements()); });

            //doesn't detect 'entry' at all in the Descendants()
            var basic = xmlDoc.Elements("entry").ToList();
            var c = result.Count;
            return xmlDoc.Descendants(
                    XName.Get("entry"))/*.Where(x =>
                                             x.Element(XName.Get("published")) != null
                                             && x.Element(XName.Get("title")) != null
                                             && x.Element(XName.Get("id")) != null
                                             //&& x.Element(XName.Get("summary")) != null
                    )*/.ToList();
        }
    }

    public static class XElementExtensions
    {
        public static String AttemptToGetElement(this XElement xElement, String node)
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

            return xElement.Element(XName.Get(node)) != null ? xElement.Element(XName.Get(node)).Value : "";
        }

        public static String AttemptToGetElementsFromChoices(this XElement xElement, IEnumerable<String> nodes)
        {
            var elementValue = "";
            foreach (var result in nodes.ToList().Select(n => AttemptToGetElement(xElement, n)).Where(result => !String.IsNullOrWhiteSpace(result)))
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
