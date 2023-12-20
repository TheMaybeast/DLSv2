/*

RoadPosition class developed by PNWParksFan for DLS

If you use this code (or any derivative work) in any project, you MUST credit 
"PNWParksFan" in your mod credits, both included in the download package's 
credits/readme file AND visible on the download page BEFORE downloading the file.

*/

using System;
using System.Collections.Generic;
using Rage;
using Rage.Native;

namespace DLSv2.Utils
{
    [Flags]
    public enum NodeProperties
    {
        NONE = 0,
        offroad = 1,
        player_road = 2,
        no_trucks = 4,
        disabled = 8,
        inside = 16,
        dead_end = 32,
        highway = 64,
        junction = 128,
        traffic_light = 256,
        stop_sign = 512,
        water = 1024
    }

    internal class RoadPosition
    {
        [Flags]
        public enum NodeFlags
        {
            NONE = 0,
            INCLUDE_SWITCHED_OFF_NODES = 1,
            INCLUDE_BOAT_NODES = 2,
            IGNORE_SLIP_LANES = 4,
            IGNORE_SWITCHED_OFF_DEADENDS = 8
        }

        public enum RoadSideDirection
        {
            FORWARDS = 0,
            BACK = 1,
            EITHER = -1
        }

        private Func<(bool valid, Vector3 pos, float heading)> GetLocationFunc;
        public uint MinUpdateWait { get; set; } = 250;
        public uint MaxUpdateWait { get; set; } = 60000;
        public float MinMoveDist { get; set; } = 1;
        public float MinHeadingChange { get; set; } = 15;

        public NodeFlags NodeSearchFlags { get; set; } = NodeFlags.INCLUDE_SWITCHED_OFF_NODES;

        public int TryNextNodes { get; set; } = 5;

        private const float zMeasureMult = 10f;
        private uint lastUpdate;
        private Vector3 lastLocation;
        private float lastHeading;
        private Vector3 nearestNodePos;
        private int nearestNodeLanes;
        private float nearestNodeHeading;
        private int nearestNodeDensity;
        private NodeProperties nearestNodeProperties;
        private NodeProperties nextNodeProperties;
        private Vector3 nodePosA;
        private Vector3 nodePosB;
        private Vector3 boundaryPosA;
        private Vector3 boundaryPosB;
        private Vector3 nearestBoundaryPos;
        private int lanesAB;
        private int lanesBA;
        private float median;
        private (Vector3 start, Vector3 end)[] laneEdgesAB = new (Vector3, Vector3)[] {};
        private (Vector3 start, Vector3 end)[] laneEdgesBA = new (Vector3, Vector3)[] {};
        private Vector3[] medianAB = new Vector3[] {};
        private Vector3[] medianBA = new Vector3[] { };
        private float laneWidthAB;
        private float laneWidthBA;
        private float nearestNodeLaneWidth;
        private bool oneWay;
        private float nodeHeadingA;
        private float nodeHeadingB;
        private float headingDiffA;
        private float headingDiffB;
        private float ABheading;
        private float BAheading;
        private bool segmentOK;
        private List<Vector3> attemptedNewNodePositions = new List<Vector3>();
        private List<(Vector3, Vector3)> attemptedNewSegments = new List<(Vector3, Vector3)>();
        private Vector3 n;
        private float p;
        private float distToPlane;

        // Position in the current lane, -1f to 1f, where 0f is in center of lane, -1f is all the way to the left side, +1f is ll the way to the right side
        public float PosInLane { get; private set; }

        // Total number of lanes in both directions
        public int TotalLanes { get; private set; }

        // Total number of lanes on this side
        public int LanesThisSide { get; private set; }

        // Number of lanes to the left (not including current lane)
        public int LanesToLeft { get; private set; }

        // Number of lanes to the right (not including current lane)
        public int LanesToRight { get; private set; }

        // Distance from the current position to the edge of the nearest lane (if in median or shoulder)
        public float DistFromLanes { get; private set; }

        // True if in the median or left of the boundary on a one-way
        public bool InMedian { get; private set; }

        // True if on the right side of the rightmost lane
        public bool InShoulder { get; private set; }
        
        // True if in the road (between the lanes)
        public bool InLane { get; private set; }
        public float LanePosition { get; private set; }
        public int CurrentLane => (int)Math.Floor(LanePosition) + 1;
        public char CurrentLaneAlpha => (char)('A' + (CurrentLane - 1));

        public float HeadingOffset { get; private set; }

        // Flag if road segment is one-way
        public bool OneWayRoad { get; private set; }

        public RoadPosition(Vector3 position, float heading)
        {
            SetTarget(position, heading);
        }

        public RoadPosition(Entity entity)
        {
            SetTarget(entity);
        }

        public void SetTarget(Entity entity)
        {
            GetLocationFunc = () => entity ? (true, entity.Position, entity.Heading) : (false, Vector3.Zero, 0);
        }

        public void SetTarget(Vector3 position, float heading)
        {
            GetLocationFunc = () => (true, position, heading);
        }

        public void Process()
        {
            // Get game time ms since last time location was processed
            uint timeSinceUpdate = CachedGameTime.GameTime - lastUpdate;

            // If minimum wait time has not been met, do not process
            if (timeSinceUpdate < MinUpdateWait) return;

            // Get current position and heading
            (bool valid, Vector3 pos, float heading) = GetLocationFunc();
            if (!valid) return;

            // If location has not moved at least the minimum distance and max update time has not been 
            // exceeded, do not process. Always proceed if max wait time has been exceeded. 
            if (pos.DistanceTo(lastLocation) < MinMoveDist && Math.Abs(NormalizeHeadingDiff(lastHeading, heading, false)) < MinHeadingChange && timeSinceUpdate < MaxUpdateWait) return;

            // try to get the nearest node matching the requirements; continue if unable to find any nodes
            if (!GetNearestNode(pos, NodeSearchFlags, 0, out nearestNodePos, out nearestNodeHeading, out nearestNodeLanes, zMeasureMult)) return;

            // try to get properties of the nearest node
            if (!GetNodeProperties(nearestNodePos, out nearestNodeProperties)) return;

            // attempt to get road segment matching the lanes of the nearest node
            if (!GetRoadSegment(nearestNodePos, 0, nearestNodeLanes, !NodeSearchFlags.HasFlag(NodeFlags.INCLUDE_SWITCHED_OFF_NODES), out nodePosA, out nodePosB, out lanesBA, out lanesAB, out median)) return;


            // if the target location is past the selected segment, try to get the next segment that connects back to here
            if (TryNextNodes > 0)
            {
                Vector3 dirNearest = MathHelper.ConvertHeadingToDirection(nearestNodeHeading).ToNormalized();
                Vector3 testPosFwd = nearestNodePos + dirNearest * 2f;
                Vector3 testPosBack = nearestNodePos - dirNearest * 2f;
                bool posCloserToFwd = (pos.DistanceTo2D(testPosFwd) < pos.DistanceTo2D(testPosBack));
                bool segCloserToFwd = Math.Max(nodePosB.DistanceTo2D(testPosFwd), nodePosA.DistanceTo2D(testPosFwd)) < Math.Max(nodePosB.DistanceTo2D(testPosBack), nodePosA.DistanceTo2D(testPosBack));
                attemptedNewNodePositions.Clear();
                attemptedNewSegments.Clear();
                if (posCloserToFwd != segCloserToFwd)
                {
                    for (int n = 1; n <= TryNextNodes; n++)
                    {
                        GetNearestNode(pos, NodeSearchFlags, n, out Vector3 newSegStartPos, out _, out _, zMeasureMult);
                        GetRoadSegment(newSegStartPos, 0, nearestNodeLanes, !NodeSearchFlags.HasFlag(NodeFlags.INCLUDE_SWITCHED_OFF_NODES), out Vector3 newNodePosA, out Vector3 newNodePosB, out int newLanesBA, out int newLanesAB, out float newMedianWidth);
                        attemptedNewNodePositions.Add(newSegStartPos);
                        attemptedNewSegments.Add((newNodePosA, newNodePosB));
                        if ((newNodePosA == nearestNodePos || newNodePosB == nearestNodePos) && !(newNodePosA == nodePosA && newNodePosB == nodePosB))
                        {
                            nodePosA = newNodePosA;
                            nodePosB = newNodePosB;
                            lanesAB = newLanesAB;
                            lanesBA = newLanesBA;
                            median = newMedianWidth;
                            break;
                        }
                    }
                }
            }
            
            Vector3 dirAB = nodePosB - nodePosA;
            Vector3 dirBA = nodePosA - nodePosB;
            ABheading = MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(dirAB));
            BAheading = MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(dirBA));

            // Reset time of update to now if all above checks are successful
            lastUpdate = CachedGameTime.GameTime;
            lastLocation = pos;
            lastHeading = heading;

            bool gotBoundaryA = GetRoadBoundary(nodePosA, ABheading, out boundaryPosA);
            bool gotBoundaryB = GetRoadBoundary(nodePosB, BAheading, out boundaryPosB);

            laneWidthAB = 5.4f;
            laneWidthBA = 5.4f;
            Vector3 normAB;
            Vector3 normBA;

            if (gotBoundaryA)
            {
                laneWidthAB = lanesAB > 0 ? (boundaryPosA.DistanceTo(nodePosA) - (0.5f * median)) / lanesAB : 0;
                normAB = (boundaryPosA - nodePosA).ToNormalized() * laneWidthAB;
            } else
            {
                normAB = MathHelper.ConvertHeadingToDirection(MathHelper.RotateHeading(ABheading, -90)).ToNormalized() * laneWidthAB;
                boundaryPosA = nodePosA + (normAB * laneWidthAB * lanesAB);
            }

            if (gotBoundaryB)
            {
                laneWidthBA = lanesBA > 0 ? (boundaryPosB.DistanceTo(nodePosB) - (0.5f * median)) / lanesBA : 0;
                normBA = (boundaryPosB - nodePosB).ToNormalized() * laneWidthBA;
            } else
            {
                normBA = MathHelper.ConvertHeadingToDirection(MathHelper.RotateHeading(BAheading, -90)).ToNormalized() * laneWidthBA;
                boundaryPosB = nodePosB + (normBA * laneWidthBA * lanesBA);
            }

            Vector3 medNormAB = normAB.ToNormalized() * median / 2;
            Vector3 medNormBA = normBA.ToNormalized() * median / 2;

            medianAB = new Vector3[2] { nodePosA + medNormAB, nodePosB + medNormAB };
            medianBA = new Vector3[2] { nodePosB + medNormBA, nodePosA + medNormBA };

            oneWay = (lanesAB == 0 || lanesBA == 0);
            
            if (oneWay)
            {
                laneWidthAB *= 2;
                laneWidthBA *= 2;
                medNormAB *= 2;
                medNormBA *= 2;
                normAB *= 2;
                normBA *= 2;

                if (lanesAB > 0)
                {
                    medianAB = new Vector3[2] { nodePosA - normAB * 0.5f * lanesAB + medNormAB, nodePosB - normAB * 0.5f * lanesAB + medNormAB };
                    medianBA = medianAB;
                    laneWidthBA = 0;
                }
                else 
                {
                    medianBA = new Vector3[2] { nodePosB - normBA * 0.5f * lanesBA + medNormBA, nodePosA - normBA * 0.5f * lanesBA + medNormBA };
                    medianAB = medianBA;
                    laneWidthAB = 0;
                }
            }

            // Find which node (A or B) is the main node, and get respective headings for checks
            if (nodePosA.DistanceTo2D(nearestNodePos) < nodePosB.DistanceTo2D(nearestNodePos))
            {
                // if node A is cloest to the nearest node, then A = nearest
                nodeHeadingA = nearestNodeHeading;
                nearestNodeLanes = lanesAB;
                nearestNodeLaneWidth = laneWidthAB;
                nearestBoundaryPos = boundaryPosA;
                // node B is the next node
                if (!GetNearestNode(nodePosB, NodeSearchFlags, out _, out nodeHeadingB)) return;
                if (!GetNodeProperties(nodePosB, out nextNodeProperties)) return;
            }
            else
            {
                // else B = nearest
                nodeHeadingB = nearestNodeHeading;
                nearestNodeLanes = lanesBA;
                nearestNodeLaneWidth = laneWidthBA;
                nearestBoundaryPos = boundaryPosB;
                // node A is the next node
                if (!GetNearestNode(nodePosA, NodeSearchFlags, out _, out nodeHeadingA)) return;
                if (!GetNodeProperties(nodePosA, out nextNodeProperties)) return;
            }

            headingDiffA = NormalizeHeadingDiff(ABheading, nodeHeadingA);
            headingDiffB = NormalizeHeadingDiff(BAheading, nodeHeadingB);

            // merge the properties of both nodes for checking flags which may indicate problem areas
            NodeProperties combinedProperties = nearestNodeProperties | nextNodeProperties;

            bool isJunction = nearestNodeProperties.HasFlag(NodeProperties.junction); 
            bool isOnOffRoad = ((combinedProperties & NodeProperties.offroad) != (nearestNodeProperties & nextNodeProperties & NodeProperties.offroad));
            float minHeadingDiff = Math.Min(headingDiffA, headingDiffB);
            bool badLanes = (nearestNodeLanes != (lanesAB + lanesBA) && Math.Min(laneWidthAB, laneWidthBA) < 0);
            bool badHeading = minHeadingDiff > 10;

            segmentOK = !badHeading && !badLanes;

            if (segmentOK)
            {
                laneEdgesAB = GetLaneEdges(nodePosA, nodePosB, normAB, medNormAB, lanesAB, oneWay);
                laneEdgesBA = GetLaneEdges(nodePosB, nodePosA, normBA, medNormBA, lanesBA, oneWay);

                Vector3 planeOrigin = nodePosA;
                Vector3 planeNorm = normAB;
                if (oneWay)
                {
                    planeOrigin = medianAB[0];
                    if (lanesBA > 0) planeNorm = normBA;
                }
                else if (normAB == Vector3.Zero)
                {
                    planeNorm = -normBA;
                }

                CalculatePlane(planeOrigin, dirAB, planeNorm);
                UpdateFinalPosition();

                return;
            }

            // If proceeding past here, the road segment returned is not aligned with the main road
            // (often a junction link will get picked up). Use alternate approach. 

            // check if either node on the segment is a junction, OR if one node is on-road and the other is off-road
            if (isJunction || isOnOffRoad) return;

            // Lane count is typically wrong when a bad link gets selected; recalculate it based on boundary pos
            // assuming lanes will be the standard width and node is in the center (one-way)
            float fullWidth = nearestBoundaryPos.DistanceTo2D(nearestNodePos) * 2;
            float roundedLanes = (float)Math.Round(fullWidth / 5.4f, 1);
            float guessLaneWidth = 5.4f;
            if (roundedLanes % 1 != 0 && Math.Round(fullWidth / 4f, 1) % 1 == 0)
            {
                guessLaneWidth = 4f;
            }
            int guessLanes = (int)Math.Round(fullWidth / guessLaneWidth);

            // crossover links almost exclusively get selected on one-way nodes, assume one-way
            oneWay = true;

            float segmentGuessLength = MathHelper.Clamp(lastLocation.DistanceTo2D(nearestNodePos), 5, 20);
            Vector3 guessDir = MathHelper.ConvertHeadingToDirection(nearestNodeHeading).ToNormalized() * segmentGuessLength;
            Vector3 guessNorm = MathHelper.ConvertHeadingToDirection(MathHelper.RotateHeading(nearestNodeHeading, -90)).ToNormalized() * guessLaneWidth;
            Vector3 guessNode2 = nearestNodePos + guessDir;

            medianAB = new Vector3[2] { nearestNodePos - guessNorm * 0.5f * guessLanes, guessNode2 - guessNorm * 0.5f * guessLanes };
            medianBA = medianAB;
            laneEdgesAB = GetLaneEdges(nearestNodePos, guessNode2, guessNorm, Vector3.Zero, guessLanes, true);
            laneEdgesBA = new (Vector3 start, Vector3 end)[] { };

            // set to final values instead of intermediate
            lanesAB = guessLanes;
            laneWidthAB = guessLaneWidth;
            lanesBA = 0;
            laneWidthBA = 0;
            median = 0;
            ABheading = nearestNodeHeading;
            BAheading = -nearestNodeHeading;

            CalculatePlane(medianAB[0], guessDir, guessNorm);
            UpdateFinalPosition();
        }

        private float NormalizeHeadingDiff(float heading1, float heading2, bool ignoreDirection = true)
        {
            float diff = (MathHelper.NormalizeHeading(heading1) - MathHelper.NormalizeHeading(heading2)) % (ignoreDirection ? 180 : 360);

            if (ignoreDirection) diff = Math.Abs(diff);

            if (ignoreDirection && diff > 90) diff = Math.Abs(diff - 180);

            return diff;
        }

        private (Vector3 start, Vector3 end)[] GetLaneEdges(Vector3 node1, Vector3 node2, Vector3 laneNorm, Vector3 medNorm, int lanes, bool oneWay)
        {
            float offset = oneWay ? 0.5f : 0;
            (Vector3 start, Vector3 end)[] edgeCoords = new (Vector3, Vector3)[lanes + 1];
            for (int l = 0; l <= lanes; l++)
            {
                edgeCoords[l].start = node1 + medNorm  + laneNorm * (l) - offset * laneNorm * lanes;
                edgeCoords[l].end   = node2 + medNorm  + laneNorm * (l) - offset * laneNorm * lanes;
            }
            return edgeCoords;
        }

        private void CalculatePlane(Vector3 origin, Vector3 dir1, Vector3 dir2)
        {
            Vector3 n1 = Vector3.Cross(dir1, dir2).ToNormalized();
            n = Vector3.Cross(n1, dir1).ToNormalized();
            p = -Vector3.Dot(n, origin);
        }

        private float GetDistToPlane(Vector3 point)
        {
            return Vector3.Dot(n, point) + p;
        }

        private void UpdateFinalPosition()
        {
            distToPlane = GetDistToPlane(lastLocation);

            float laneWidth = 0;
            int lanes = 0;
            float lanePos = 0;
            float segmentHeading = 0;

            if (oneWay)
            {
                if (lanesAB > 0)
                {
                    laneWidth = laneWidthAB;
                    lanes = lanesAB;
                    segmentHeading = ABheading;
                }
                else
                {
                    laneWidth = laneWidthBA;
                    lanes = lanesBA;
                    segmentHeading = BAheading;
                }

                InMedian = distToPlane < 0;
                lanePos = Math.Max(distToPlane, 0) / laneWidth;
                DistFromLanes = InMedian ? Math.Abs(distToPlane) : Math.Max(distToPlane - (laneWidth * lanes), 0);
            }
            else
            {
                if (distToPlane >= 0)
                {
                    laneWidth = laneWidthAB;
                    lanes = lanesAB;
                    segmentHeading = ABheading;
                }
                else
                {
                    laneWidth = laneWidthBA;
                    lanes = lanesBA;
                    segmentHeading = BAheading;
                }

                // positive distance to plane means we are in the A-B side of the segment
                // if the distance is less than half the median, we are in the median
                InMedian = (Math.Abs(distToPlane) <= (median / 2));
                // fractional lane position is distance to the median divided by lane width
                lanePos = (Math.Abs(distToPlane) - (median / 2)) / laneWidth;
                // leftover distance after subtracting lanes and median gives how far off the road you are
                DistFromLanes = Math.Max(Math.Abs(distToPlane) - (median / 2) - (laneWidth * lanes), 0);
            }

            TotalLanes = lanesAB + lanesBA;
            LanesThisSide = lanes;
            HeadingOffset = NormalizeHeadingDiff(segmentHeading, lastHeading, false);
            LanePosition = MathHelper.Clamp(lanePos, 0, lanes);
            // if lane position is past the edge, we are on the shoulder
            InShoulder = (lanePos > lanes);
            LanesToLeft = (int)Math.Floor(LanePosition);
            LanesToRight = lanes - (int)Math.Ceiling(LanePosition);
            InLane = !(InMedian || InShoulder);
            PosInLane = InLane ? 2 * ((LanePosition % 1) - 0.5f) : 0;
        }

        private static Dictionary<Vector3, (int density, NodeProperties properties)> cachedNodeProperties = new Dictionary<Vector3, (int density, NodeProperties properties)>();

        public static bool GetNodeProperties(Vector3 coords, out int density, out NodeProperties properties)
        {
            if (!cachedNodeProperties.TryGetValue(coords, out var cachedValue))
            {
                bool result = NativeFunction.Natives.GET_VEHICLE_NODE_PROPERTIES<bool>(coords, out density, out int iNodeProperties);
                properties = (NodeProperties)iNodeProperties;
                cachedNodeProperties.Add(coords, (density, properties));
                return result;
            }

            density = cachedValue.density;
            properties = cachedValue.properties;
            return true;
        }

        public static bool GetNodeProperties(Vector3 coords, out NodeProperties properties) => GetNodeProperties(coords, out _, out properties);

        public static bool GetNearestNode(ISpatial location, NodeFlags searchMode, out Vector3 nodePosition, out float nodeHeading) => 
            GetNearestNode(location.Position, searchMode, out nodePosition, out nodeHeading);

        public static bool GetNearestNode(Vector3 coords, NodeFlags searchMode, out Vector3 nodePosition, out float nodeHeading) =>
            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(coords, out nodePosition, out nodeHeading, (int)searchMode);

        public static bool GetNearestNode(ISpatial location, NodeFlags searchMode, int N, out Vector3 nodePosition, out float nodeHeading, out int numLanes, float zMeasureMult = 3.0f) =>
            GetNearestNode(location.Position, searchMode, N, out nodePosition, out nodeHeading, out numLanes, zMeasureMult);

        public static bool GetNearestNode(Vector3 coords, NodeFlags searchMode, int N, out Vector3 nodePosition, out float nodeHeading, out int numLanes, float zMeasureMult = 3.0f) =>
            NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(coords, N, out nodePosition, out nodeHeading, out numLanes, (int)searchMode, zMeasureMult, /* zMeasureTolerance */ 0.0f);

        public static bool GetRoadSegment(Vector3 coords, float minLength, int minLanes, bool ignoreDisabledNodes, out Vector3 nodePosA, out Vector3 nodePosB, out int lanesBtoA, out int lanesAtoB, out float medianWidth) =>
            NativeFunction.Natives.GET_CLOSEST_ROAD<bool>(coords, minLength, minLanes, out nodePosA, out nodePosB, out lanesBtoA, out lanesAtoB, out medianWidth, ignoreDisabledNodes);

        public static bool GetPositionBySideOfRoad(Vector3 nodePos, RoadSideDirection direction, out Vector3 roadSidePos) =>
            NativeFunction.Natives.GET_POSITION_BY_SIDE_OF_ROAD<bool>(nodePos, (int)direction, out roadSidePos);

        public static bool GetRoadBoundary(Vector3 nodePos, float nodeHeading, out Vector3 roadBoundaryPos) =>
            NativeFunction.Natives.GET_ROAD_BOUNDARY_USING_HEADING<bool>(nodePos, nodeHeading, out roadBoundaryPos);
    }
}
