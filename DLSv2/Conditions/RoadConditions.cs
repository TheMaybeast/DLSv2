using Rage;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace DLSv2.Conditions;

using Core;
using Utils;

public class NodeFlagsCondition : VehicleCondition<NodeFlagsCondition.NodeFlagConditionInstance>
{
    protected override uint UpdateWait => 60;

    [XmlAttribute("nearest_n")]
    public int NumNodesToCheck { get; set; } = 1;

    [XmlAttribute("max_dist_to_node")]
    public float MaxDistToNode { get; set; } = 50;

    [XmlAttribute("no_node_status")]
    public bool StatusIfNoNode { get; set; } = false;

    [XmlAttribute("include_disabled_nodes")]
    public bool IncludeSwitchedOffNodes { get; set; } = false;

    [XmlAttribute("all")]
    public bool RequireAll { get; set; } = true;

    [XmlArray("Flags")]
    [XmlArrayItem("Item")]
    public List<NodePropertyState> PropertyRequirements { get; set; } = new List<NodePropertyState>();

    public class NodeFlagConditionInstance : ConditionInstance
    {
        public Vector3 LastPosition;
    }

    protected override bool Evaluate(ManagedVehicle veh)
    {
        Vector3 pos = veh.Vehicle.Position;
        var inst = GetInstance(veh) as NodeFlagConditionInstance;
        if (pos.DistanceTo(inst.LastPosition) < 0.5f) return inst.LastCalculated;
        inst.LastPosition = pos;

        var findNodeFlags = IncludeSwitchedOffNodes ? RoadPosition.NodeFlags.INCLUDE_SWITCHED_OFF_NODES : RoadPosition.NodeFlags.NONE;

        NodeProperties allProperties = NodeProperties.NONE;
        bool gotAtLeastOneNode = false;

        for (int n = 0; n < NumNodesToCheck; n++)
        {
            if (!RoadPosition.GetNearestNode(pos, findNodeFlags, n, out Vector3 nodePos, out _, out _)) break;
            if (nodePos.DistanceTo(pos) > MaxDistToNode) break;
            if (!RoadPosition.GetNodeProperties(nodePos, out NodeProperties properties)) break;
            allProperties = allProperties | properties;
            gotAtLeastOneNode = true;
        }

        if (!gotAtLeastOneNode) return StatusIfNoNode;

        bool all = true;
        bool any = false;

        foreach (var req in PropertyRequirements)
        {
            bool ok = allProperties.HasFlag(req.Property) == req.MustHaveFlag;
            any = any || ok;
            all = all && ok;
        }

        return RequireAll ? all : any;
    }

    public class NodePropertyState
    {
        [XmlAttribute("has_flag")]
        public bool MustHaveFlag { get; set; } = true;

        [XmlText]
        public NodeProperties Property { get; set; } = NodeProperties.NONE;
    }

}

public abstract class RoadPositionCondition : VehicleCondition
{
    private static Dictionary<ManagedVehicle, RoadPosition> cachedRoadPositions = new Dictionary<ManagedVehicle, RoadPosition>();

    private protected RoadPosition GetRoadPos(ManagedVehicle veh)
    {
        if (!cachedRoadPositions.TryGetValue(veh, out RoadPosition roadPos))
        {
            roadPos = new RoadPosition(veh.Vehicle);
            cachedRoadPositions.Add(veh, roadPos);
        }

        roadPos.Process();
        return roadPos;
    }
}

public abstract class RoadPosMinMaxCondition : RoadPositionCondition
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

public class RoadLanesLeftCondition : RoadPosMinMaxCondition
{
    protected override bool Evaluate(ManagedVehicle veh)
    {
        int lanesLeft = GetRoadPos(veh).LanesToLeft;
        return (!Min.HasValue || lanesLeft >= Min.Value) && (!Max.HasValue || lanesLeft <= Max.Value);
    }
}

public class RoadLanesRightCondition : RoadPosMinMaxCondition
{
    protected override bool Evaluate(ManagedVehicle veh)
    {
        int lanesRight = GetRoadPos(veh).LanesToRight;
        return (!Min.HasValue || lanesRight >= Min.Value) && (!Max.HasValue || lanesRight <= Max.Value);
    }
}

public class RoadShoulderCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("on_shoulder")]
    public bool IsOnShoulder { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        var roadPos = GetRoadPos(veh);

        if (roadPos.InShoulder && IsOnShoulder)
            return (!Min.HasValue || roadPos.DistFromLanes >= Min.Value) && (!Max.HasValue || roadPos.DistFromLanes <= Max.Value);

        return (roadPos.InShoulder == IsOnShoulder);
    }
}

public class RoadMedianCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("in_median")]
    public bool IsInMedian { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        var roadPos = GetRoadPos(veh);
            
        if (roadPos.InMedian && IsInMedian)
            return (!Min.HasValue || roadPos.DistFromLanes >= Min.Value) && (!Max.HasValue || roadPos.DistFromLanes <= Max.Value);

        return (roadPos.InMedian == IsInMedian);
    }
}

public class RoadDirectionCondition : RoadPositionCondition
{
    [XmlAttribute("is_one_way")]
    public bool IsOneWay { get; set; }

    protected override bool Evaluate(ManagedVehicle veh) => GetRoadPos(veh).OneWayRoad == IsOneWay;
}

public class RoadLanesCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("both_directions")]
    public bool BothDirections { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        int lanes = BothDirections ? GetRoadPos(veh).TotalLanes : GetRoadPos(veh).LanesThisSide;

        return (!Min.HasValue || lanes >= Min.Value) && (!Max.HasValue || lanes <= Max.Value);
    }
}

public class RoadLanePositionCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("abs")]
    public bool AbsValue { get; set; } = false;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        float pos = GetRoadPos(veh).PosInLane;
        if (AbsValue) pos = Math.Abs(pos);

        return (!Min.HasValue || pos >= Min.Value) && (!Max.HasValue || pos <= Max.Value);
    }
}

public class OnRoadCondition : RoadPositionCondition
{
    [XmlAttribute("on_road")]
    public bool IsOnRoad { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh) => GetRoadPos(veh).InLane == IsOnRoad;
}

public class RoadLaneIndexCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("from_left")]
    public bool FromLeft { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        var roadPos = GetRoadPos(veh);
        int lane = roadPos.CurrentLane;
        if (!FromLeft) lane = roadPos.LanesThisSide - lane + 1;
        
        return roadPos.InLane && (!Min.HasValue || lane >= Min.Value) && (!Max.HasValue || lane <= Max.Value);
    }
}

public class RoadHeadingOffsetCondition : RoadPosMinMaxCondition
{
    [XmlAttribute("abs")]
    public bool AbsValue { get; set; } = false;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        float headingDiff = GetRoadPos(veh).HeadingOffset;
        if (AbsValue) headingDiff = Math.Abs(headingDiff);

        return (!Min.HasValue || headingDiff >= Min.Value) && (!Max.HasValue || headingDiff <= Max.Value);
    }
}