using System;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace WeatherSprocket
{
    public class WeatherSprocket : RegexSprocket
    {

        public override Regex Pattern
        {
            get { return new Regex(@"(?<=\bweather[ ])\d{4,5}", RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            if (match.Length > 0)
            {
                var matchResult = match.Captures[0].ToString();
                bot.Say(getWeather(matchResult), message.Receiver); ;
            }
        }

        private string getWeather(string zipcode)
        {
            string requestUri = String.Format("http://api.wunderground.com/api/ffb2f3f9960dd675/geolookup/conditions/forecast/q/{0}.json",zipcode);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            Stream dataStream;
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JObject obj = JObject.Parse(responseFromServer);
            JToken currentObservation = obj["current_observation"];
            JToken displayLocation = obj["current_observation"]["display_location"];
            reader.Close();
            dataStream.Close();
            response.Close();
            var cityState = (string)displayLocation["full"];
            var weather = (string)currentObservation["weather"];
            var temperature = (string)currentObservation["temperature_string"];
            var output = string.Format("Weather in {0} is {1} and {2}",cityState,temperature,weather);
            return output;
        }
    }
}