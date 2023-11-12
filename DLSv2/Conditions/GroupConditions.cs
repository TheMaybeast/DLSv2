using System.Collections.Generic;
using System.Xml.Serialization;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    public abstract class GroupConditions : BaseCondition
    {
        protected static Dictionary<GroupConditions, ConditionInstance> instances = new Dictionary<GroupConditions, ConditionInstance>();
        public override ConditionInstance GetInstance(ManagedVehicle mv)
        {
            if (!instances.TryGetValue(this, out var instance))
            {
                instance = new ConditionInstance(this);
                instances.Add(this, instance);
            }

            return instance;
        }

        [XmlElement("Group")]
        public ConditionList NestedConditions { get; set; } = new ConditionList();
    }

    public class AllCondition : GroupConditions
    {
        public override bool Evaluate(ManagedVehicle veh)
        {
            bool ok = true;
            foreach (BaseCondition condition in NestedConditions.Conditions)
            {
                ok = ok && condition.Evaluate(veh);
            }
            return ok;
        }
    }

    public class AnyCondition : GroupConditions
    {
        public override bool Evaluate(ManagedVehicle veh)
        {
            foreach (BaseCondition condition in NestedConditions.Conditions)
            {
                if (condition.Evaluate(veh)) return true;
            }
            return false;
        }
    }
}
