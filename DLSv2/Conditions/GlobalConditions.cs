using System;
using System.Xml.Serialization;
using Rage;
using Rage.Native;

namespace DLSv2.Conditions;

using Core;

public class TimeCondition : GlobalCondition
{
    protected override uint UpdateWait => 1000;

    [XmlAttribute("start")]
    public string Start
    {
        get => StartTime.ToString(@"hh\:mm");
        set => StartTime = TimeSpan.Parse(value);
    }
        
    [XmlAttribute("end")]
    public string End
    {
        get => EndTime.ToString(@"hh\:mm");
        set => EndTime = TimeSpan.Parse(value);
    }

    [XmlIgnore]
    public TimeSpan StartTime { get; set; }

    [XmlIgnore]
    public TimeSpan EndTime { get; set; }

    protected override bool Evaluate()
    {
        var time = World.TimeOfDay;

        if (EndTime > StartTime) return (time >= StartTime) && (time <= EndTime);
        // If end time is before start time, then the time period passes through midnight
        else return (time >= StartTime) || (time <= EndTime);
    }
}

public class WeatherCondition : GlobalCondition
{
    protected override uint UpdateWait => 5000;

    [XmlArray("AllowedConditions")]
    [XmlArrayItem("Type")]
    public string[] IncludeWeatherTypes { get; set; } = new string[] { };

    [XmlArray("ProhibitedConditions")]
    [XmlArrayItem("Type")]
    public string[] ExcludeWeatherTypes { get; set; } = new string[] { };

    protected override bool Evaluate()
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