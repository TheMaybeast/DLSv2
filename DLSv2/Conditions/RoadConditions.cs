using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Conditions
{
    using Core;
    using Rage;
    using Utils;

    public class NodeFlagsCondition : VehicleCondition
    {
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

        protected override bool Evaluate(ManagedVehicle veh)
        {
            var findNodeFlags = IncludeSwitchedOffNodes ? RoadPosition.NodeFlags.INCLUDE_SWITCHED_OFF_NODES : RoadPosition.NodeFlags.NONE;

            NodeProperties allProperties = NodeProperties.NONE;
            bool gotAtLeastOneNode = false;

            for (int n = 0; n < NumNodesToCheck; n++)
            {
                if (!RoadPosition.GetNearestNode(veh.Vehicle, findNodeFlags, n, out Vector3 nodePos, out _, out _)) break;
                if (nodePos.DistanceTo(veh.Vehicle) > MaxDistToNode) break;
                if (!RoadPosition.GetNodeProperties(nodePos, out _, out NodeProperties properties)) break;
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
}
