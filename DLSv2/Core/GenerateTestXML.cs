#if DEBUG


using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Rage.Attributes;

namespace DLSv2.Core;

using Conditions;

internal static class GenerateTestXML
{
    [ConsoleCommand]
    public static void GenerateModelConfig()
    {
        DLSModel model = new DLSModel
        {
            Vehicles = "police",
            Modes = new List<LightMode>()
        };

        LightMode s1 = new();
        model.Modes.Add(s1);
        s1.Yield = new Yield() { Enabled = true };
        s1.SirenSettings = new SirenSetting();
        s1.Triggers = new AnyCondition();
        s1.Triggers.NestedConditions.Add(new DriverCondition() { HasDriver = true });
        var a = new AllCondition();
        s1.Triggers.NestedConditions.Add(a);
        a.NestedConditions.Add(new EngineStateCondition() { EngineOn = true });
        a.NestedConditions.Add(new WeatherCondition() { IncludeWeatherTypes = new string[] { "RAIN", "CLOUDS" } });

        XmlAttributeOverrides attrOverrides = new();
        GroupConditions.AddCustomAttributes(attrOverrides);
            
        XmlSerializer dlsSerializer = new XmlSerializer(typeof(DLSModel), attrOverrides);
        using (StreamWriter w = new StreamWriter(@"plugins\dls\dls_test4.xml"))
        {
            dlsSerializer.Serialize(w, model);
        }
    }
}
#endif