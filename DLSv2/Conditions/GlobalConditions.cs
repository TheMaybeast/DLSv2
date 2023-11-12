using System.Xml.Serialization;
using DLSv2.Core;
using Rage;
using Rage.Native;

namespace DLSv2.Conditions
{
    public class WeatherCondition : GlobalCondition
    {
        [XmlArray("AllowedConditions")]
        [XmlArrayItem("Type")]
        public string[] IncludeWeatherTypes { get; set; } = new string[] { };

        [XmlArray("ProhibitedConditions")]
        [XmlArrayItem("Type")]
        public string[] ExcludeWeatherTypes { get; set; } = new string[] { };

        public override bool Evaluate()
        {
            bool ok = true;
            if (IncludeWeatherTypes != null && IncludeWeatherTypes.Length > 0)
            {
                ok = false;
                foreach (var weather in IncludeWeatherTypes)
                {
                    ok = ok || IsWeather(weather);
                }
            }
            if (ExcludeWeatherTypes != null && ExcludeWeatherTypes.Length > 0)
            {
                foreach (var weather in ExcludeWeatherTypes)
                {
                    ok = ok && !IsWeather(weather);
                }
            }
            return ok;
        }

        private bool IsWeather(string weather)
        {
            NativeFunction.Natives.GET_CURR_WEATHER_STATE(out uint weather1, out uint weather2, out float pctWeather2);
            return (weather1 == Game.GetHashKey(weather) && pctWeather2 <= 0.5f) || (weather2 == Game.GetHashKey(weather) && pctWeather2 >= 0.5f);
        }
    }
}
