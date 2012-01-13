using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Sprockets.Core;
using Newtonsoft.Json;

namespace DisqusAnnouncer
{
    public class DisqusAnnouncer : IAnnounce
    {
        public DisqusAnnouncer()
        {
            Posts = new List<PostResponse>();
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.FromSeconds(10); }
        }

        public List<PostResponse> Posts { get; private set; }

        public static bool Lock { get; set; }

        public void Execute(Bot bot)
        {
            if (Lock == true) return;
            Lock = true;

            Debug.WriteLine(string.Format("Fetching from Disqus! - {0:HH.mm.ss}", DateTime.Now));

            var client = new WebClient();

            var threads = new List<ThreadResponse>();
            threads.AddRange(GetThreads(client));

            var posts = GetPosts(client);
            foreach(var post in posts.Where(p => p.isDeleted != true && p.isSpam != true).OrderBy(p => DateTime.Parse(p.createdAt)))
            {
                if (Posts.SingleOrDefault(p => p.id == post.id) == null)
                {
                    Posts.Add(post);

                    var thread = threads.SingleOrDefault(t => t.id == post.thread);

                    foreach (var room in bot.Rooms)
                    {
                        var msg = Regex.Replace(post.message, @"<br>", "\n");
                        msg = Regex.Replace(msg, @"\"" rel=\""nofollow\"">[-./""\w\s]*://[-./""\w\s]*", string.Empty);
                        msg = Regex.Replace(msg, @"<a href=\""", string.Empty);
                        msg = Regex.Replace(msg, @"<(.|\n)*?>", string.Empty);
                        bot.Say(
                            string.Format("{0} - {1} ({2}) - {3}", thread == null ? "Unknown" : thread.title,
                                          post.author.name, DateTime.Parse(post.createdAt), msg), room);
                    }
                }
            }

            Lock = false;
        }

        private static IEnumerable<ThreadResponse> GetThreads(WebClient client)
        {
            var threadResponse = client.DownloadString(new Uri("https://disqus.com/api/3.0/forums/listThreads.json?forum=code52&api_key=txmSHGCXXRZt558E4pvT9akmwveiCd8Ny685WSDlUKlUeLECP8oxZQ3BtfFIIx0c"));
            var threads = JsonConvert.DeserializeObject<ThreadRootObject>(threadResponse).response;

            return threads;
        }

        private static IEnumerable<PostResponse> GetPosts(WebClient client)
        {
            var postResponse = client.DownloadString(new Uri("https://disqus.com/api/3.0/forums/listPosts.json?forum=code52&api_key=txmSHGCXXRZt558E4pvT9akmwveiCd8Ny685WSDlUKlUeLECP8oxZQ3BtfFIIx0c"));
            var posts = JsonConvert.DeserializeObject<PostRootObject>(postResponse).response;

            return posts;
        }
    }


    public class PostCursor
    {
        public object prev { get; set; }
        public bool hasNext { get; set; }
        public string next { get; set; }
        public bool hasPrev { get; set; }
        public object total { get; set; }
        public string id { get; set; }
        public bool more { get; set; }
    }

    public class Avatar
    {
        public string permalink { get; set; }
        public string cache { get; set; }
    }

    public class PostAuthor
    {
        public string username { get; set; }
        public string about { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string joinedAt { get; set; }
        public string profileUrl { get; set; }
        public string emailHash { get; set; }
        public Avatar avatar { get; set; }
        public bool isAnonymous { get; set; }
        public string id { get; set; }
    }

    public class PostResponse
    {
        public bool isJuliaFlagged { get; set; }
        public string forum { get; set; }
        public int? parent { get; set; }
        public bool isApproved { get; set; }
        public PostAuthor author { get; set; }
        public List<object> media { get; set; }
        public bool isDeleted { get; set; }
        public bool isFlagged { get; set; }
        public int dislikes { get; set; }
        public string raw_message { get; set; }
        public string createdAt { get; set; }
        public bool isSpam { get; set; }
        public string thread { get; set; }
        public int points { get; set; }
        public int likes { get; set; }
        public bool isEdited { get; set; }
        public string message { get; set; }
        public string id { get; set; }
        public bool isHighlighted { get; set; }
    }

    [JsonObject("RootObject")]
    public class PostRootObject
    {
        public PostCursor cursor { get; set; }
        public int code { get; set; }
        public List<PostResponse> response { get; set; }
    }


    public class ThreadCursor
    {
        public object prev { get; set; }
        public bool hasNext { get; set; }
        public string next { get; set; }
        public bool hasPrev { get; set; }
        public object total { get; set; }
        public string id { get; set; }
        public bool more { get; set; }
    }

    public class ThreadResponse
    {
        public string category { get; set; }
        public int reactions { get; set; }
        public string author { get; set; }
        public string forum { get; set; }
        public string title { get; set; }
        public int userScore { get; set; }
        public List<object> identifiers { get; set; }
        public int dislikes { get; set; }
        public string createdAt { get; set; }
        public string slug { get; set; }
        public bool isClosed { get; set; }
        public int posts { get; set; }
        public string link { get; set; }
        public int likes { get; set; }
        public string message { get; set; }
        public string id { get; set; }
        public bool isDeleted { get; set; }
    }

    [JsonObject("RootObject")]
    public class ThreadRootObject
    {
        public ThreadCursor cursor { get; set; }
        public int code { get; set; }
        public List<ThreadResponse> response { get; set; }
    }
}
