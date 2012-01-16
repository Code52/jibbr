using System;
using System.Text.RegularExpressions;
using Jabbot;
using Jabbot.Models;
using Jabbot.Sprockets;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace Jabbot.Extensions
{
    public class WeatherSprocket : RegexSprocket
    {

        public override Regex Pattern
        {
            get { return new Regex(@"(?<=\bweather[ ])\d{4,5}", RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, IBot bot)
        {
            var zipCode = match.Captures[0].ToString();
            bot.Say(getWeather(zipCode), message.Receiver); ;
        }

        private string getWeather(string zipcode)
        {
            var result = "Unable to retrieve weather. ";

            try
            {
                //open api request
                string requestUri = String.Format("http://api.wunderground.com/api/ffb2f3f9960dd675/geolookup/conditions/forecast/q/{0}.json", zipcode);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                Stream dataStream;
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                //deserialize JSON results
                JObject obj = JObject.Parse(responseFromServer);

                //process results
                JToken error = obj["response"]["error"];
                if (error == null)
                {
                    //results
                    JToken currentObservation = obj["current_observation"];
                    JToken displayLocation = obj["current_observation"]["display_location"];
                    JToken forecastDay = obj["forecast"]["txt_forecast"]["forecastday"];

                    var cityState = (string)displayLocation["full"];
                    var temperature = (string)currentObservation["temperature_string"];
                    var forecast = (string)forecastDay[0]["fcttext"];

                    //if the city,state & temperature results are null - don't bother returning results. it's likely this will EVER happen
                    if (cityState == null && temperature == null)
                    {
                        result += string.Format(" Weather information was not provided for {0}", zipcode);
                    }
                    else
                    {
                        result = string.Format("Weather in {0} is {1} and {2}", cityState, temperature, forecast);
                    }
                }
                else
                {
                    result = string.Format("{0} is not a valid U.S. zipcode", zipcode);
                }

                //close api request
                reader.Close();
                dataStream.Close();
                response.Close();
                return result;

            }
            catch (Exception ex)
            {
                //if ex.Message is null than the API didn't find any records for the zipcode
                if (ex.Message == null)
                {
                    result += " Something bad happened";
                }
                else
                {
                    result += string.Format("Error {0}", ex.Message);
                }

                return result;
            }
        }
    }
}