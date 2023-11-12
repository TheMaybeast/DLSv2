using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using Rage;

namespace DLSv2.Core
{
    public class ConditionList
    {
        public List<BaseCondition> Conditions { get; set; } = new List<BaseCondition>();

        internal static void AddCustomAttributes(XmlAttributeOverrides overrides)
        {
            XmlAttributes attrs = new XmlAttributes();

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(BaseCondition)) && !t.IsAbstract)
                {
                    attrs.XmlElements.Add(new XmlElementAttribute() {
                        ElementName = t.Name.Replace("Condition", ""),
                        Type = t
                    });
                }
            }

            overrides.Add(typeof(ConditionList), "Conditions", attrs);
        }
    }

    public class ConditionInstance
    {
        public BaseCondition Condition { get; }

        public ConditionInstance(BaseCondition condition)
        {
            Condition = condition;
        }

        private uint lastUpdate = 0;
        private bool lastState = false;

        public bool LastTriggered => lastState;

        // Event handler delegate for events sent by this condition
        public delegate void TriggerEvent(ConditionInstance sender, BaseCondition condition, bool state);
        // Invoked when this specific instance is triggered
        public event TriggerEvent OnInstanceTriggered;
        // Invoked when any instance is triggered
        public static event TriggerEvent OnAnyTriggered;

        public bool Update(ManagedVehicle veh)
        {
            if (Game.GameTime > lastUpdate + Condition.UpdateWait)
            {
                lastUpdate = Game.GameTime;
                bool newState = Condition.Evaluate(veh);
                if (lastState == newState) return newState;
                lastState = newState;
                OnInstanceTriggered?.Invoke(this, Condition, newState);
                OnAnyTriggered?.Invoke(this, Condition, newState);
                return newState;
            }

            return lastState;
        }
    }

    public abstract class BaseCondition
    {
        public abstract ConditionInstance GetInstance(ManagedVehicle veh);

        public abstract bool Evaluate(ManagedVehicle veh);

        [XmlIgnore]
        public virtual uint UpdateWait { get; set; } = 0;

        public bool Update(ManagedVehicle veh)
        {
            var instance = GetInstance(veh);
            return instance.Update(veh);
        }
    }

    public abstract class GlobalCondition : BaseCondition
    {
        [XmlIgnore]
        protected static Dictionary<GlobalCondition, ConditionInstance> instances = new Dictionary<GlobalCondition, ConditionInstance>();

        // ignores the vehicle argument, as global conditions apply to all vehicles
        public override ConditionInstance GetInstance(ManagedVehicle veh)
        {
            if (!instances.TryGetValue(this, out var instance))
            {
                instance = new ConditionInstance(this);
                instances.Add(this, instance);
            }
            return instance;
        }

        public abstract bool Evaluate();
        public override bool Evaluate(ManagedVehicle veh) => Evaluate();
    }

    public abstract class VehicleCondition : BaseCondition
    {
        [XmlIgnore]
        protected static Dictionary<(ManagedVehicle veh, VehicleCondition cond), ConditionInstance> instances = new Dictionary<(ManagedVehicle veh, VehicleCondition cond), ConditionInstance>();

        public override ConditionInstance GetInstance(ManagedVehicle mv)
        {
            if (!instances.TryGetValue((mv, this), out var instance))
            {
                instance = new ConditionInstance(this);
                instances.Add((mv, this), instance);
            }

            return instance;
        }
    }

    public class DriverCondition : VehicleCondition
    {
        [XmlAttribute]
        public bool HasDriver { get; set; } = true;

        public override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.HasDriver == HasDriver;
    }

    public class EngineStateCondition : VehicleCondition
    {
        [XmlAttribute]
        public bool EngineOn { get; set; } = true;

        public override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.IsEngineOn == EngineOn;
    }

    public class WeatherCondition : GlobalCondition
    {
        [XmlArray("AllowedConditions")]
        [XmlArrayItem("Type")]
        public WeatherType[] IncludeWeatherTypes { get; set; } = new WeatherType[] { };

        [XmlArray("ProhibitedConditions")]
        [XmlArrayItem("Type")]
        public WeatherType[] ExcludeWeatherTypes { get; set; } = new WeatherType[] { };

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

        private bool IsWeather(WeatherType weather)
        {
            Rage.Native.NativeFunction.Natives.GET_CURR_WEATHER_STATE(out uint weather1, out uint weather2, out float pctWeather2);
            return (weather1 == (uint)weather && pctWeather2 <= 0.5f) || (weather2 == (uint)weather && pctWeather2 >= 0.5f);
        }
    }
}
