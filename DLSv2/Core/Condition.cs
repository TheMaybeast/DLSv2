using System;
using System.Collections.Generic;
using System.Reflection;

namespace DLSv2.Core.Triggers
{
    public class ConditionArgs : EventArgs
    {
        public bool ConditionMet;
        public ConditionArgs(bool conditionMet) => ConditionMet = conditionMet;
    }

    public abstract class BaseCondition
    {
        internal static Dictionary<string, Type> TriggerTypes = new Dictionary<string, Type>();
        private static void GetTriggers() 
        {
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(BaseCondition)) && !t.IsAbstract)
                {
                    TriggerTypes.Add(t.Name, t);
                }
            }
        }

        // Static constructor will run when the class is first loaded, and registers all 
        // available trigger types dynamically into the TriggerTypes dictionary
        static BaseCondition()
        {
            GetTriggers();
        }

        public event EventHandler<ConditionArgs> ConditionChangedEvent;

        public bool LastTriggered = false;

        public void ConditionResult(bool status)
        {
            if (LastTriggered == status) return;
            LastTriggered = status;
            OnRaiseCustomEvent(new ConditionArgs(status));
        }

        protected virtual void OnRaiseCustomEvent(ConditionArgs e)
        {
            EventHandler<ConditionArgs> raiseEvent = ConditionChangedEvent;
            if (raiseEvent != null) raiseEvent(this, e);
        }

        protected string arguments;

        public abstract bool Evaluate();
    }

    public abstract class VehicleCondition : BaseCondition
    {
        public ManagedVehicle MV { private set;  get; }
        public Rage.Vehicle Vehicle => MV.Vehicle;

        public virtual void Init(ManagedVehicle managedVehicle, string args)
        {
            MV = managedVehicle;
            arguments = args;
        }
    }

    public abstract class VehicleOnOffCondition : VehicleCondition
    {
        public abstract bool GetVehState();

        public override bool Evaluate()
        {
            bool state = GetVehState();
            if (arguments == "on") return state;
            if (arguments == "off") return !state;
            else throw new ArgumentException("VehicleOnOffCondition argument must be \"on\" or \"off\"");
        }
    }

    public abstract class GlobalCondition : BaseCondition
    {
        public virtual void Init(string args)
        {
            arguments = args;
        }
    }
}
