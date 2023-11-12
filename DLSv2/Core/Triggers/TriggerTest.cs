using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using Rage;
using Rage.Attributes;

namespace DLSv2.Core.Triggers
{
    /*
    public static class TriggerTest
    {
        [Rage.Attributes.ConsoleCommand]
        public static void LoadTestXML()
        {
            string path = @"plugins\dls\dls_test2.xml";
            
        }
    }
    */

    public class TriggerTest
    {
    }

    public class SubTriggers : TriggerTest
    {
        public List<TriggerTest> Tests { get; set; } = new List<TriggerTest>();
    }

    public class Any : SubTriggers
    {
    }

    public class All : SubTriggers
    {
    }

    public class TestA : TriggerTest
    {
        public string foo;
    }

    public class TestB : TriggerTest
    {
        public string bar;
    }

    public class TestWrapper : Any
    {
    }

    public class TestConfigFile
    {
        [XmlAttribute("name")]
        public string Name = "Test Name";

        [XmlElement("yield")]
        public bool ShouldYield = true;

        [XmlElement("Triggers")]
        public TestWrapper conditions = new TestWrapper();
    }

    public static class ExampleTriggerTest
    {
        [ConsoleCommand]
        private static void CreateTriggerTest()
        {
            var w = new TestWrapper();
            var f = new TestConfigFile();
            f.conditions = w;

            w.Tests.Add(new TestA() { foo = "hello" });
            w.Tests.Add(new TestB() { bar = "world" });

            var any = new Any();
            any.Tests.Add(new TestA() { foo = "suba" });
            any.Tests.Add(new TestB() { bar = "subb" });
            w.Tests.Add(any);

            var all = new All();
            all.Tests.Add(new TestA() { foo = "subc" });
            all.Tests.Add(new TestB() { bar = "subd" });
            w.Tests.Add(all);

            XmlAttributes attrs = new XmlAttributes();

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(TriggerTest)) && !t.IsAbstract)
                {
                    XmlElementAttribute attr = new XmlElementAttribute();
                    attr.ElementName = t.Name;
                    attr.Type = t;
                    attrs.XmlElements.Add(attr);
                }
            }

            XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();
            attributeOverrides.Add(typeof(SubTriggers), "Tests", attrs);

            XmlSerializer serializer = new XmlSerializer(typeof(TestConfigFile), attributeOverrides);

            // XmlSerializer serializer = new XmlSerializer(typeof(TestWrapper));
            using (StreamWriter writer = new StreamWriter("plugins\\dls\\dls_test.xml"))
            {
                serializer.Serialize(writer, f);
            }
        }

        [ConsoleCommand]
        private static void ReadTriggerTest()
        {
            XmlAttributes attrs = new XmlAttributes();

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(TriggerTest)) && !t.IsAbstract)
                {
                    XmlElementAttribute attr = new XmlElementAttribute();
                    attr.ElementName = t.Name;
                    attr.Type = t;
                    attrs.XmlElements.Add(attr);
                }
            }

            XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();
            attributeOverrides.Add(typeof(SubTriggers), "Tests", attrs);

            XmlSerializer serializer = new XmlSerializer(typeof(TestConfigFile), attributeOverrides);

            // XmlSerializer serializer = new XmlSerializer(typeof(TestWrapper));
            using (StreamReader reader = new StreamReader("plugins\\dls\\dls_test.xml"))
            {
                TestConfigFile f = (TestConfigFile)serializer.Deserialize(reader);
            }
        }

        [ConsoleCommand]
        private static void GenerateDLSConfigTest()
        {
            DLSModel model = new DLSModel();
            model.Vehicles = "police";
            model.Modes = new List<Mode>();

            Mode s1 = new Mode();
            model.Modes.Add(s1);
            s1.Yield = new Yield() {  Enabled = true };
            s1.SirenSettings = new SirenSetting();
            s1.Triggers = new ConditionList();
            s1.Triggers.Conditions.Add(new DriverCondition() { HasDriver = true });
            s1.Triggers.Conditions.Add(new EngineStateCondition() { EngineOn = true });
            s1.Triggers.Conditions.Add(new WeatherCondition() { IncludeWeatherTypes = new WeatherType[] { WeatherType.Rain, WeatherType.Clouds } });

            XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
            ConditionList.AddCustomAttributes(attrOverrides);

            XmlSerializer dlsSerializer = new XmlSerializer(typeof(DLSModel), attrOverrides);
            using (StreamWriter w = new StreamWriter(@"plugins\dls\dls_test3.xml"))
            {
                dlsSerializer.Serialize(w, model);
            }

        }
    }
}
