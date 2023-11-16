using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Core
{
    public abstract class GroupConditions : InstanceCondition<GroupConditions, BaseCondition.ConditionInstance>
    {
        internal static void AddCustomAttributes(XmlAttributeOverrides overrides)
        {
            XmlAttributes attrs = new XmlAttributes();

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(BaseCondition)) && !t.IsAbstract)
                {
                    attrs.XmlElements.Add(new XmlElementAttribute()
                    {
                        ElementName = t.Name.Replace("Condition", ""),
                        Type = t
                    });
                }
            }

            overrides.Add(typeof(GroupConditions), "NestedConditions", attrs);
        }

        public List<BaseCondition> NestedConditions { get; set; } = new List<BaseCondition>();

        protected override GroupConditions GetKey(ManagedVehicle veh) => this;
    }

    public class AllCondition : GroupConditions
    {
        protected override bool Evaluate(ManagedVehicle veh)
        {
            bool ok = true;
            foreach (BaseCondition condition in NestedConditions)
            {
                ok = ok && condition.Update(veh);
            }
            return ok;
        }
    }

    public class AnyCondition : GroupConditions
    {
        protected override bool Evaluate(ManagedVehicle veh)
        {
            foreach (BaseCondition condition in NestedConditions)
            {
                if (condition.Update(veh)) return true;
            }
            return false;
        }
    }
}
