## JibbR

### A Jabbr bot designed for collaborative projects

Extending the Jabbot library - a bot API for [JabbR](https://github.com/davidfowl/JabbR).

## Connecting to a Jabbr Room

It's as easy as:

```csharp
var bot = new Bot("http://myjabbot", "username", "password");
bot.PowerUp();
bot.Join("someroom");
bot.Say("Hello", "someroom");
bot.Say("Ok I'm off");
bot.ShutDown();
```


## Writing Sprockets

Sprockets are things you can plug-in to enhance the behavior of your bot. Simply drop a dll with classes that implement
ISprocket into a Sprockets folder and you're done. Here's an port of the [math.coffee](https://github.com/github/hubot/blob/master/src/scripts/math.coffee) from hubot:

```csharp
public class MathSprocket : RegexSprocket
{
    public override Regex Pattern
    {
        get { return new Regex("(calc|calculate|convert|math)( me)? (.*)"); }
    }

    protected override void ProcessMatch(Match match, ChatMessage message, Bot bot)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us"));
        client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

        client.GetAsync("http://www.google.com/ig/calculator?hl=en&q=" + Uri.EscapeDataString(match.Groups[3].Value))
              .ContinueWith(task =>
              {
                  if (task.Result.IsSuccessStatusCode)
                  {
                      task.Result.Content.ReadAsStringAsync().ContinueWith(readTask =>
                      {
                          dynamic json =  JsonConvert.DeserializeObject(readTask.Result);

                          string solution = json.rhs;

                          bot.Reply(message.FromUser, solution ?? "Could not compute.", message.Room);
                      });
                  }
              });
    }
}
```

A new extension being added to JibbR is for announcement-style sprockets. You can specify how often an announcement may occur, and include code to execute for the active bot.

```csharp
public class EchoAnnouncement : IAnnounce
{
    public TimeSpan Interval
    {
        get { return TimeSpan.FromMinutes(10); }
    }

    public void Execute(Bot bot)
    {
        foreach (var room in bot.Rooms)
        {
            bot.Say("Hello world!", room);
        }
    }
}
```

### another code52 project

#### code52.org

=======
