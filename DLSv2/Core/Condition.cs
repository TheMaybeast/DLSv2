using System.Collections.Generic;
using System.ComponentModel;
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
            public BaseCondition Condition { get; set; }

            public ConditionInstance() { }

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

    public abstract class InstanceCondition<TConditionKey, TConditionVal> : BaseCondition where TConditionVal : BaseCondition.ConditionInstance, new()
    {
        [XmlIgnore]
        protected static Dictionary<TConditionKey, TConditionVal> instances = new Dictionary<TConditionKey, TConditionVal>();

        protected abstract TConditionKey GetKey(ManagedVehicle veh);
        
        public override ConditionInstance GetInstance(ManagedVehicle mv)
        {
            if (!instances.TryGetValue(GetKey(mv), out var instance))
            {
                instance = new TConditionVal { Condition = this };
                instances.Add(GetKey(mv), instance);
            }

            return instance;
        }
        
    }

    public abstract class GlobalCondition : InstanceCondition<GlobalCondition, BaseCondition.ConditionInstance>
    {
        protected override GlobalCondition GetKey(ManagedVehicle veh) => this;

        // ignores the vehicle argument, as global conditions apply to all vehicles
        protected abstract bool Evaluate();
        protected override bool Evaluate(ManagedVehicle veh) => Evaluate();
    }

    public abstract class VehicleCondition<TInstance> : InstanceCondition<(ManagedVehicle veh, VehicleCondition<TInstance> cond), TInstance> where TInstance : BaseCondition.ConditionInstance, new()
    {
        protected override (ManagedVehicle veh, VehicleCondition<TInstance> cond) GetKey(ManagedVehicle veh) => (veh, this);
    }

    public abstract class VehicleCondition : VehicleCondition<BaseCondition.ConditionInstance>
    {
        
    }

    public abstract class VehicleMinMaxCondition : VehicleCondition
    {
        [XmlIgnore]
        public float? Min
        {
            get => MinValueSpecified ? MinValue : (float?)null;
            set
            {
                MinValueSpecified = value.HasValue;
                if (value.HasValue) MinValue = value.Value;
                else MinValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MinValueSpecified { get; set; }
        [XmlAttribute("min")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MinValue { get; set; }


        [XmlIgnore]
        public float? Max
        {
            get => MaxValueSpecified ? MaxValue : (float?)null;
            set
            {
                MaxValueSpecified = value.HasValue;
                if (value.HasValue) MaxValue = value.Value;
                else MaxValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MaxValueSpecified { get; set; }
        [XmlAttribute("max")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MaxValue { get; set; }

    }
}
