using System;
using System.Collections.Generic;
using System.Reflection;

#if false
namespace DLSv2.Core
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

    public class ConditionInstance
    {
        public BaseCondition Condition { get; set; }
        
    }
}


/*

Condition config: 1 instance per occurrence in an xml 
Condition itself: 1 instance per managed vehicle 
Condition class specifies (via a generic?) which type of config to use

or....


Condition config class gets serialized
Process takes a managed vehicle instance argument 
Up to the config class how to pass that off... potentially to a subclass
Have a generic subclass that handles state for normal vehicle conditions 

However state depends on the specific config... so maybe it can look things up by 
vehicle *and* config somehow? Nested dictionary? 

Use a generic to specify the type of state object used




*/

#endif