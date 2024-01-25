using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Core;

public abstract class GroupConditions : VehicleCondition
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
}

public class AllCondition : GroupConditions
{
    public AllCondition() : base() { }

    public AllCondition(IEnumerable<BaseCondition> conditions)
    {
        NestedConditions = conditions.ToList();
    }

    public AllCondition(params BaseCondition[] conditions) : this(conditions.AsEnumerable()) { }

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
    public AnyCondition() : base() { }

    public AnyCondition(IEnumerable<BaseCondition> conditions)
    {
        NestedConditions = conditions.ToList();
    }

    public AnyCondition(params BaseCondition[] conditions) : this(conditions.AsEnumerable()) { }

    protected override bool Evaluate(ManagedVehicle veh)
    {
        foreach (BaseCondition condition in NestedConditions)
        {
            if (condition.Update(veh)) return true;
        }
        return false;
    }
}