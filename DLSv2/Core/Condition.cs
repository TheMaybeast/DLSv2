using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using Rage;

namespace DLSv2.Core
{
    public abstract class BaseCondition
    {
        public abstract ConditionInstance GetInstance(ManagedVehicle veh);

        public bool Update(ManagedVehicle veh) => GetInstance(veh).Update(veh);

        protected abstract bool Evaluate(ManagedVehicle veh);

        [XmlIgnore]
        protected virtual uint UpdateWait { get; set; } = 0;

        public class ConditionInstance
        {
            public BaseCondition Condition { get; }

            public ConditionInstance(BaseCondition condition)
            {
                Condition = condition;
            }

            private uint lastUpdate;
            private bool lastState;

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

        protected abstract bool Evaluate();
        protected override bool Evaluate(ManagedVehicle veh) => Evaluate();
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
}
