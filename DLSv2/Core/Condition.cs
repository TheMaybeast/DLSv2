using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Linq;
using Rage.Attributes;

namespace DLSv2.Core;

using Utils;

public abstract class BaseCondition
{
# if DEBUG
    private static Dictionary<BaseCondition, List<double>> executionTimeLog = new Dictionary<BaseCondition, List<double>>();

    [ConsoleCommand]
    private static void Command_DebugConditionExecutionTime(bool showParentConditions = false)
    {
        $"Condition,Count,Min,Max,Avg".ToLog(LogLevel.DEBUG);
        foreach (var condition in executionTimeLog)
        {
            if (!showParentConditions && condition.Key is GroupConditions) continue;

            var count = condition.Value.Count;
            var avg = count > 0 ? condition.Value.Average() : 0;
            var max = count > 0 ? condition.Value.Max() : 0;
            var min = count > 0 ? condition.Value.Min() : 0;
            $"{condition.Key.GetType().Name},{count},{min:0.####},{max:0.####},{avg:0.####}".ToLog(LogLevel.DEBUG);
        }
    }

    public BaseCondition()
    {
        executionTimeLog.Add(this, new List<double>());
    }
#endif

    [XmlAttribute("delay_time")]
    public int DelayTime { get; set; } = 0;

    [XmlAttribute("max_on_time")]
    public int MaxOnTime { get; set; } = 0;

    [XmlAttribute("min_on_time")]
    public int MinOnTime { get; set; } = 0;

    [XmlAttribute("stay_on_time")]
    public int StayOnTime { get; set; } = 0;

    public abstract ConditionInstance GetInstance(ManagedVehicle veh);

    public bool Update(ManagedVehicle veh)
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
        bool value = GetInstance(veh).Update(veh);
        sw.Stop();
        executionTimeLog[this].Add(sw.Elapsed.TotalMilliseconds);
        return value;
#else
            return GetInstance(veh).Update(veh);
#endif
    }

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

        private uint lastCalcTime;
        private bool lastCalcResult;
        private uint lastCalcChangedTime;
        private bool lastExternalState;
        private uint lastExternalChangedOnTime;

        public bool LastTriggered => lastExternalState;
        public bool LastCalculated => lastCalcResult;

        public uint TimeSinceUpdate => CachedGameTime.GameTime - lastCalcTime;

        // Event handler delegate for events sent by this condition
        public delegate void TriggerEvent(ConditionInstance sender, BaseCondition condition, bool state);
        // Invoked when this specific instance is triggered
        public event TriggerEvent OnInstanceTriggered;
        // Invoked when any instance is triggered
        public static event TriggerEvent OnAnyTriggered;

        public bool Update(ManagedVehicle veh)
        {
            if (CachedGameTime.GameTime > lastCalcTime + Condition.UpdateWait)
            {
                lastCalcTime = CachedGameTime.GameTime;
                bool newCalcState = Condition.Evaluate(veh);

                if (newCalcState != lastCalcResult)
                {
                    lastCalcResult = newCalcState;
                    lastCalcChangedTime = CachedGameTime.GameTime;
                }

                uint timeSinceCalcChanged = CachedGameTime.GameTime - lastCalcChangedTime;
                uint timeSinceExternalOn = CachedGameTime.GameTime - lastExternalChangedOnTime;

                bool newExternalState = newCalcState;

                // if the new calculated state is on, and the actual external state was previously off,
                // and the wait time has not been met, leave it off
                if (newCalcState && !lastExternalState && Condition.DelayTime > 0 && timeSinceCalcChanged < Condition.DelayTime)
                {
                    newExternalState = false;
                }

                // if the new calculated state is on, and the actual external state has been on for more than the max time, turn it off
                if (newCalcState && Condition.MaxOnTime > 0 && (timeSinceCalcChanged > Condition.MaxOnTime || (lastExternalState && timeSinceExternalOn > Condition.MaxOnTime)))
                {
                    newExternalState = false;
                }

                // if the new calculated state is off, and the previous actual external state was on, 
                // and the minimum on time has not yet been reached, leave it on
                if (!newCalcState && lastExternalState && Condition.MinOnTime > 0 && timeSinceExternalOn < Condition.MinOnTime)
                {
                    newExternalState = true;
                }

                // if the current calculated state is off, but the external state is still on
                // and there is a stay-on time configured, keep on until time is met
                if (!newCalcState && lastExternalState && Condition.StayOnTime > 0 && timeSinceCalcChanged < Condition.StayOnTime)
                {
                    newExternalState = true;
                }

                // If external state has not changed, return current state and do not trigger events
                if (newExternalState == lastExternalState) return lastExternalState;

                // If external state has changed, update saved state and trigger events
                lastExternalState = newExternalState;
                if (newExternalState) lastExternalChangedOnTime = CachedGameTime.GameTime;

                OnInstanceTriggered?.Invoke(this, Condition, newExternalState);
                OnAnyTriggered?.Invoke(this, Condition, newExternalState);
                    
                return newExternalState;
            }

            return lastExternalState;
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