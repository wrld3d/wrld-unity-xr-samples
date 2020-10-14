using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wrld.Common.Maths;
using Wrld.Utilities;
using System.Linq;

namespace Wrld.Transport
{
    internal class TransportApiInternal
    {
        private IDictionary<int, TransportPositioner> m_positionerIdToObject = new Dictionary<int, TransportPositioner>();
        private readonly IntPtr m_handleToSelf;

        public delegate void TransportNetworkChangedHandler(TransportNetworkType networkType, TransportCellKey cellKey);

        public event TransportNetworkChangedHandler OnTransportNetworkCellAdded;
        public event TransportNetworkChangedHandler OnTransportNetworkCellRemoved;
        public event TransportNetworkChangedHandler OnTransportNetworkCellUpdated;


        internal TransportApiInternal()
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        public TransportPositioner CreatePositioner(TransportPositionerOptions options)
        {
            var optionsInterop = options.ToInterop();

            var transportPositionerId = NativeTransportApi_CreatePositioner(NativePluginRunner.API, ref optionsInterop);

            var transportPositioner = new TransportPositioner(
                this,
                transportPositionerId,
                options
                );

            m_positionerIdToObject.Add(transportPositionerId, transportPositioner);

            NotifyTransportPositionerPointOnGraphChanged(transportPositionerId);

            return transportPositioner;
        }

        public TransportPathfindResult FindShortestPath(TransportPathfindOptions options)
        {
            var optionsInterop = options.ToInterop();

            var pathfindResultInterop = NativeTransportApi_FindShortestPath(NativePluginRunner.API, ref optionsInterop);

            if (pathfindResultInterop.IsPathFound)
            {
                // alloc and pin buffers
                var pathDirectedEdgeIdsBuffer = new TransportDirectedEdgeIdInterop[pathfindResultInterop.PathDirectedEdgesSize];
                var pathDirectedEdgeIdsBufferGCHandle = GCHandle.Alloc(pathDirectedEdgeIdsBuffer, GCHandleType.Pinned);
                pathfindResultInterop.PathDirectedEdges = pathDirectedEdgeIdsBufferGCHandle.AddrOfPinnedObject();

                var pathPointsBuffer = new DoubleVector3[pathfindResultInterop.PathPointsSize];
                var pathPointsBufferGCHandle = GCHandle.Alloc(pathPointsBuffer, GCHandleType.Pinned);
                pathfindResultInterop.PathPoints = pathPointsBufferGCHandle.AddrOfPinnedObject();

                var pathPointParamsBuffer = new double[pathfindResultInterop.PathPointsSize];
                var pathPointParamsBufferGCHandle = GCHandle.Alloc(pathPointParamsBuffer, GCHandleType.Pinned);
                pathfindResultInterop.PathPointParams = pathPointParamsBufferGCHandle.AddrOfPinnedObject();

                var result = PopulateAndReleaseTransportPathfindResult(pathDirectedEdgeIdsBuffer, pathPointsBuffer, pathPointParamsBuffer, ref pathfindResultInterop);

                pathDirectedEdgeIdsBufferGCHandle.Free();
                pathPointsBufferGCHandle.Free();
                pathPointParamsBufferGCHandle.Free();

                return result;
            }
            else
            {
                var failedResult = new TransportPathfindResult();
                return failedResult;
            }
        }

        private TransportPathfindResult PopulateAndReleaseTransportPathfindResult(
            TransportDirectedEdgeIdInterop[] pathDirectedEdgeIdsBuffer,
            DoubleVector3[] pathPointsBuffer,
            double[] pathPointParamsBuffer,
            ref TransportPathfindResultInterop pathfindResultInterop
            )
        {
            var success = NativeTransportApi_TryPopulateAndReleaseTransportPathfindResult(NativePluginRunner.API, ref pathfindResultInterop);

            if (!success)
            {
                return new TransportPathfindResult();
            }
            var pathDirectedEdgeIds = new List<TransportDirectedEdgeId>();

            for (int i = 0; i < pathfindResultInterop.PathDirectedEdgesSize; ++i)
            {
                var directedEdgeId = pathDirectedEdgeIdsBuffer[i].FromInterop();
                pathDirectedEdgeIds.Add(directedEdgeId);
            }

            var pathPoints = pathPointsBuffer.ToList();
            var pathPointParams = pathPointParamsBuffer.ToList();

            var result = new TransportPathfindResult(
                pathfindResultInterop.IsPathFound,
                pathDirectedEdgeIds.AsReadOnly(),
                pathfindResultInterop.FirstEdgeParam,
                pathfindResultInterop.LastEdgeParam,
                pathfindResultInterop.DistanceMeters,
                pathPoints.AsReadOnly(),
                pathPointParams.AsReadOnly()
                );

            return result;
        }

        private void ValidateExists(int id)
        {
            if (!m_positionerIdToObject.ContainsKey(id))
            {
                throw new System.ArgumentException(string.Format("TransportPositioner id {0}does not exist.", id));
            }
        }

        public void NotifyTransportPositionerPointOnGraphChanged(int transportPositionerId)
        {
            var transportPositioner = m_positionerIdToObject[transportPositionerId];
            transportPositioner.NotifyPointOnGraphChanged();
        }

        public IList<TransportNodeId> GetNodeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            var interopIds = FetchNodeIdsForNetwork(transportNetwork);
            var nodeIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return nodeIds;
        }

        public IList<TransportDirectedEdgeId> GetDirectedEdgeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            var interopIds = FetchDirectedEdgeIdsForNetwork(transportNetwork);
            var directedEdgeIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return directedEdgeIds;
        }

        public IList<TransportWayId> GetWayIdsForNetwork(TransportNetworkType transportNetwork)
        {
            var interopIds = FetchWayIdsForNetwork(transportNetwork);
            var wayIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return wayIds;
        }

        public IList<TransportNodeId> GetNodeIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var cellKeyInterop = cellKey.ToInterop();
            var interopIds = FetchNodeIdsForNetworkInCell(transportNetwork, cellKeyInterop);
            var nodeIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return nodeIds;
        }

        public IList<TransportDirectedEdgeId> GetDirectedEdgeIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var cellKeyInterop = cellKey.ToInterop();
            var interopIds = FetchDirectedEdgeIdsForNetworkInCell(transportNetwork, cellKeyInterop);
            var directedEdgeIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return directedEdgeIds;
        }

        public IList<TransportWayId> GetWayIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var cellKeyInterop = cellKey.ToInterop();
            var interopIds = FetchWayIdsForNetworkInCell(transportNetwork, cellKeyInterop);
            var wayIds = interopIds.Select(_x => _x.FromInterop()).ToList();
            return wayIds;
        }

        public bool TryGetNode(TransportNodeId nodeId, out TransportNode node)
        {
            return TryFetchNode(nodeId.ToInterop(), out node);
        }

        public bool TryGetDirectedEdge(TransportDirectedEdgeId directedEdgeId, out TransportDirectedEdge directedEdge)
        {
            return TryFetchDirectedEdge(directedEdgeId.ToInterop(), out directedEdge);
        }

        public bool TryGetWay(TransportWayId wayId, out TransportWay way)
        {
            return TryFetchWay(wayId.ToInterop(), out way);
        }

        public DoubleVector3 GetPointEcefOnPolyline(DoubleVector3[] polylinePoints, double[] polylineParams, double t)
        {
            var polylineParamsGCHandle = GCHandle.Alloc(polylineParams, GCHandleType.Pinned);
            var polylineParamsPtr = polylineParamsGCHandle.AddrOfPinnedObject();

            int i0;
            int i1;
            double s;
            NativeTransportApi_GetLinearInterpolationParams(NativePluginRunner.API, polylineParamsPtr, polylinePoints.Length, t, out i0, out i1, out s);

            polylineParamsGCHandle.Free();
            return DoubleVector3.Lerp(polylinePoints[i0], polylinePoints[i1], s);
        }

        public DoubleVector3 GetDirectionEcefOnPolyline(DoubleVector3[] polylinePoints, double[] polylineParams, double t)
        {
            var polylineParamsGCHandle = GCHandle.Alloc(polylineParams, GCHandleType.Pinned);
            var polylineParamsPtr = polylineParamsGCHandle.AddrOfPinnedObject();

            int i0;
            int i1;
            double s;
            NativeTransportApi_GetLinearInterpolationParams(NativePluginRunner.API, polylineParamsPtr, polylinePoints.Length, t, out i0, out i1, out s);

            polylineParamsGCHandle.Free();

            if (i0 == i1)
            {
                return DoubleVector3.zero;
            }

            var directionEcef = (polylinePoints[i1] - polylinePoints[i0]).normalized;

            return directionEcef;
        }

        public void NotifyTransportGraphChanged(
            TransportNetworkType transportNetworkType,
            TransportCellKey cellKey,
            TransportGraphChangeReason transportGraphChangeReason
            )
        {
            switch (transportGraphChangeReason)
            {
                case TransportGraphChangeReason.TransportGraphCellAdded:
                    if (OnTransportNetworkCellAdded != null)
                    {
                        OnTransportNetworkCellAdded(transportNetworkType, cellKey);
                    }
                    break;
                case TransportGraphChangeReason.TransportGraphCellRemoved:
                    if (OnTransportNetworkCellRemoved != null)
                    {
                        OnTransportNetworkCellRemoved(transportNetworkType, cellKey);
                    }
                    break;
                case TransportGraphChangeReason.TransportGraphCellUpdated:
                    if (OnTransportNetworkCellUpdated != null)
                    {
                        OnTransportNetworkCellUpdated(transportNetworkType, cellKey);
                    }
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException("transportGraphChangeReason");
            }
        }

        private IList<TransportNode> BuildNodesForCell(
            TransportNetworkType transportNetworkType,
            TransportCellKey cellKey
        )
        {
            var cellKeyInterop = cellKey.ToInterop();
            int nodeCount = NativeTransportApi_GetNodeCountForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop);
            if (nodeCount <= 0)
            {
                return new List<TransportNode>();
            }

            // alloc and pin buffers
            var nodeIdInteropBuffer = new TransportNodeIdInterop[nodeCount];
            var nodeIdInteropBufferGCHandle = GCHandle.Alloc(nodeIdInteropBuffer, GCHandleType.Pinned);
            var nodeIdInteropBufferPtr = nodeIdInteropBufferGCHandle.AddrOfPinnedObject();

            // populate buffers from C++ api
            var nodeSuccess = NativeTransportApi_TryGetNodeIdsForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop, nodeCount, nodeIdInteropBufferPtr);
            if (!nodeSuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for node ids");
            }

            var nodes = FetchNodes(nodeIdInteropBuffer);

            // free buffers
            nodeIdInteropBufferGCHandle.Free();

            return nodes;
        }


        private IList<TransportDirectedEdge> BuildDirectedEdgesForCell(
            TransportNetworkType transportNetworkType,
            TransportCellKey cellKey
            )
        {
            var cellKeyInterop = cellKey.ToInterop();
            int directedEdgeCount = NativeTransportApi_GetDirectedEdgeCountForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop);
            if (directedEdgeCount <= 0)
            {
                return new List<TransportDirectedEdge>();
            }

            // alloc and pin buffers
            var directedEdgeIdInteropBuffer = new TransportDirectedEdgeIdInterop[directedEdgeCount];
            var directedEdgeIdInteropBufferGCHandle = GCHandle.Alloc(directedEdgeIdInteropBuffer, GCHandleType.Pinned);
            var directedEdgeIdInteropBufferPtr = directedEdgeIdInteropBufferGCHandle.AddrOfPinnedObject();

            // populate buffers from C++ api
            var directedEdgeSuccess = NativeTransportApi_TryGetDirectedEdgeIdsForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop, directedEdgeCount, directedEdgeIdInteropBufferPtr);
            if (!directedEdgeSuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for directed edge ids");
            }

            var directedEdges = FetchDirectedEdges(directedEdgeIdInteropBuffer);

            // free buffer
            directedEdgeIdInteropBufferGCHandle.Free();
            return directedEdges;
        }

        private IList<TransportWay> BuildWaysForCell(
            TransportNetworkType transportNetworkType,
            TransportCellKey cellKey
        )
        {
            var cellKeyInterop = cellKey.ToInterop();
            int wayCount = NativeTransportApi_GetWayCountForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop);
            if (wayCount <= 0)
            {
                return new List<TransportWay>();
            }

            // alloc and pin buffers
            var wayIdInteropBuffer = new TransportWayIdInterop[wayCount];
            var wayIdInteropBufferGCHandle = GCHandle.Alloc(wayIdInteropBuffer, GCHandleType.Pinned);
            var wayIdInteropBufferPtr = wayIdInteropBufferGCHandle.AddrOfPinnedObject();

            // populate buffers from C++ api
            var waySuccess = NativeTransportApi_TryGetWayIdsForNetworkInCell(NativePluginRunner.API, transportNetworkType, cellKeyInterop, wayCount, wayIdInteropBufferPtr);
            if (!waySuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for way ids");
            }

            var ways = FetchWays(wayIdInteropBuffer);

            // free buffers
            wayIdInteropBufferGCHandle.Free();

            return ways;
        }

        private IList<TransportNodeIdInterop> FetchNodeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            int nodeCount = NativeTransportApi_GetNodeCountForNetwork(NativePluginRunner.API, transportNetwork);
            if (nodeCount <= 0)
            {
                return new List<TransportNodeIdInterop>();
            }

            var nodeIdInteropBuffer = new TransportNodeIdInterop[nodeCount];
            var nodeIdInteropBufferGCHandle = GCHandle.Alloc(nodeIdInteropBuffer, GCHandleType.Pinned);
            var nodeIdInteropBufferPtr = nodeIdInteropBufferGCHandle.AddrOfPinnedObject();
            if (!NativeTransportApi_TryGetNodeIdsForNetwork(NativePluginRunner.API, transportNetwork, nodeCount, nodeIdInteropBufferPtr))
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for node ids");
            }

            nodeIdInteropBufferGCHandle.Free();

            return nodeIdInteropBuffer;
        }

        private IList<TransportDirectedEdgeIdInterop> FetchDirectedEdgeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            int directedEdgeCount = NativeTransportApi_GetDirectedEdgeCountForNetwork(NativePluginRunner.API, transportNetwork);
            if (directedEdgeCount <= 0)
            {
                return new List<TransportDirectedEdgeIdInterop>();
            }

            var directedEdgeIdInteropBuffer = new TransportDirectedEdgeIdInterop[directedEdgeCount];
            var directedEdgeIdInteropBufferGCHandle = GCHandle.Alloc(directedEdgeIdInteropBuffer, GCHandleType.Pinned);
            var directedEdgeIdInteropBufferPtr = directedEdgeIdInteropBufferGCHandle.AddrOfPinnedObject();
            var directedEdgeSuccess = NativeTransportApi_TryGetDirectedEdgeIdsForNetwork(NativePluginRunner.API, transportNetwork, directedEdgeCount, directedEdgeIdInteropBufferPtr);
            if (!directedEdgeSuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for directed edge ids");
            }

            directedEdgeIdInteropBufferGCHandle.Free();

            return directedEdgeIdInteropBuffer;
        }

        private IList<TransportWayIdInterop> FetchWayIdsForNetwork(TransportNetworkType transportNetwork)
        {
            int wayCount = NativeTransportApi_GetWayCountForNetwork(NativePluginRunner.API, transportNetwork);
            if (wayCount <= 0)
            {
                return new List<TransportWayIdInterop>();
            }

            var wayIdInteropBuffer = new TransportWayIdInterop[wayCount];
            var wayIdInteropBufferGCHandle = GCHandle.Alloc(wayIdInteropBuffer, GCHandleType.Pinned);
            var wayIdInteropBufferPtr = wayIdInteropBufferGCHandle.AddrOfPinnedObject();
            var waySuccess = NativeTransportApi_TryGetWayIdsForNetwork(NativePluginRunner.API, transportNetwork, wayCount, wayIdInteropBufferPtr);
            if (!waySuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for way ids");
            }

            wayIdInteropBufferGCHandle.Free();

            return wayIdInteropBuffer;
        }

        private IList<TransportNodeIdInterop> FetchNodeIdsForNetworkInCell(TransportNetworkType transportNetwork, MortonKeyInterop cellKeyInterop)
        {
            int nodeCount = NativeTransportApi_GetNodeCountForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop);
            if (nodeCount <= 0)
            {
                return new List<TransportNodeIdInterop>();
            }

            var nodeIdInteropBuffer = new TransportNodeIdInterop[nodeCount];
            var nodeIdInteropBufferGCHandle = GCHandle.Alloc(nodeIdInteropBuffer, GCHandleType.Pinned);
            var nodeIdInteropBufferPtr = nodeIdInteropBufferGCHandle.AddrOfPinnedObject();
            if (!NativeTransportApi_TryGetNodeIdsForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop, nodeCount, nodeIdInteropBufferPtr))
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for node ids");
            }

            nodeIdInteropBufferGCHandle.Free();

            return nodeIdInteropBuffer;
        }

        private IList<TransportDirectedEdgeIdInterop> FetchDirectedEdgeIdsForNetworkInCell(TransportNetworkType transportNetwork, MortonKeyInterop cellKeyInterop)
        {
            int directedEdgeCount = NativeTransportApi_GetDirectedEdgeCountForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop);
            if (directedEdgeCount <= 0)
            {
                return new List<TransportDirectedEdgeIdInterop>();
            }

            var directedEdgeIdInteropBuffer = new TransportDirectedEdgeIdInterop[directedEdgeCount];
            var directedEdgeIdInteropBufferGCHandle = GCHandle.Alloc(directedEdgeIdInteropBuffer, GCHandleType.Pinned);
            var directedEdgeIdInteropBufferPtr = directedEdgeIdInteropBufferGCHandle.AddrOfPinnedObject();
            var directedEdgeSuccess = NativeTransportApi_TryGetDirectedEdgeIdsForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop, directedEdgeCount, directedEdgeIdInteropBufferPtr);
            if (!directedEdgeSuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for directed edge ids");
            }

            directedEdgeIdInteropBufferGCHandle.Free();

            return directedEdgeIdInteropBuffer;
        }

        private IList<TransportWayIdInterop> FetchWayIdsForNetworkInCell(TransportNetworkType transportNetwork, MortonKeyInterop cellKeyInterop)
        {
            int wayCount = NativeTransportApi_GetWayCountForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop);
            if (wayCount <= 0)
            {
                return new List<TransportWayIdInterop>();
            }

            var wayIdInteropBuffer = new TransportWayIdInterop[wayCount];
            var wayIdInteropBufferGCHandle = GCHandle.Alloc(wayIdInteropBuffer, GCHandleType.Pinned);
            var wayIdInteropBufferPtr = wayIdInteropBufferGCHandle.AddrOfPinnedObject();
            var waySuccess = NativeTransportApi_TryGetWayIdsForNetworkInCell(NativePluginRunner.API, transportNetwork, cellKeyInterop, wayCount, wayIdInteropBufferPtr);
            if (!waySuccess)
            {
                throw new System.ArgumentOutOfRangeException("incorrect buffer size passed for way ids");
            }

            wayIdInteropBufferGCHandle.Free();

            return wayIdInteropBuffer;
        }

        private List<TransportNode> FetchNodes(TransportNodeIdInterop[] nodeIdInteropBuffer)
        {
            var nodes = new List<TransportNode>();

            foreach (var nodeIdInterop in nodeIdInteropBuffer)
            {
                TransportNode node;
                if (!TryFetchNode(nodeIdInterop, out node))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch node");
                }
                nodes.Add(node);
            }

            return nodes;
        }

        private bool TryFetchNode(TransportNodeIdInterop nodeIdInterop, out TransportNode node)
        {
            int incidentDirectedEdgeCount = NativeTransportApi_GetIncidentDirectedEdgeCountForNode(NativePluginRunner.API, nodeIdInterop);
            if (incidentDirectedEdgeCount < 0)
            {
                node = TransportNode.MakeEmpty();
                return false;
            }

            var incidentDirectedEdgeIdInteropBuffer = new TransportDirectedEdgeIdInterop[incidentDirectedEdgeCount];
            var incidentDirectedEdgeIdInteropBufferGCHandle = GCHandle.Alloc(incidentDirectedEdgeIdInteropBuffer, GCHandleType.Pinned);
            var incidentDirectedEdgeIdInteropPtr = incidentDirectedEdgeIdInteropBufferGCHandle.AddrOfPinnedObject();

            var nodeInterop = new TransportNodeInterop()
            {
                Id = nodeIdInterop,
                IncidentDirectedEdgeIdsSize = incidentDirectedEdgeCount,
                IncidentDirectedEdgeIds = incidentDirectedEdgeIdInteropPtr
            };

            bool success = NativeTransportApi_TryGetNode(NativePluginRunner.API, ref nodeInterop);
            if (!success)
            {
                node = TransportNode.MakeEmpty();
                return false;
            }

            var incidedDirectedEdgeIds = incidentDirectedEdgeIdInteropBuffer.Select(_x => _x.FromInterop()).ToList();

            node = new TransportNode(
                nodeInterop.Id.FromInterop(),
                nodeInterop.Point,
                incidedDirectedEdgeIds
                );

            incidentDirectedEdgeIdInteropBufferGCHandle.Free();

            return true;
        }

        private List<TransportDirectedEdge> FetchDirectedEdges(TransportDirectedEdgeIdInterop[] directedEdgesIdInteropBuffer)
        {
            var directedEdges = new List<TransportDirectedEdge>();

            foreach (var directedEdgeIdInterop in directedEdgesIdInteropBuffer)
            {
                TransportDirectedEdge directedEdge;
                if (!TryFetchDirectedEdge(directedEdgeIdInterop, out directedEdge))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch directedEdge");
                }

                directedEdges.Add(directedEdge);
            }

            return directedEdges;
        }

        private bool TryFetchDirectedEdge(TransportDirectedEdgeIdInterop directedEdgeIdInterop, out TransportDirectedEdge directedEdge)
        {
            var directedEdgeInterop = new TransportDirectedEdgeInterop()
            {
                Id = directedEdgeIdInterop
            };

            bool success = NativeTransportApi_TryGetDirectedEdge(NativePluginRunner.API, ref directedEdgeInterop);
            if (!success)
            {
                directedEdge = TransportDirectedEdge.MakeEmpty();
                return false;
            }

            directedEdge = new TransportDirectedEdge(
                directedEdgeInterop.Id.FromInterop(),
                directedEdgeInterop.NodeIdA.FromInterop(),
                directedEdgeInterop.NodeIdB.FromInterop(),
                directedEdgeInterop.WayId.FromInterop(),
                directedEdgeInterop.IsWayReversed
                );
            return true;
        }

        private List<TransportWay> FetchWays(TransportWayIdInterop[] wayIdInteropBuffer)
        {
            var ways = new List<TransportWay>();

            foreach (var wayIdInterop in wayIdInteropBuffer)
            {
                TransportWay way;
                if (!TryFetchWay(wayIdInterop, out way))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch way");
                }

                ways.Add(way);
            }

            return ways;
        }

        private bool TryFetchWay(TransportWayIdInterop wayIdInterop, out TransportWay way)
        {
            var wayInterop = new TransportWayInterop()
            {
                Id = wayIdInterop
            };


            bool success = NativeTransportApi_TryGetWayBufferSizes(NativePluginRunner.API, ref wayInterop);
            if (!success)
            {
                way = TransportWay.MakeEmpty();
                return false;
            }

            var centerLinePointsBuffer = new DoubleVector3[wayInterop.CenterLinePointsBufferSize];
            var centerLinePointsBufferGCHandle = GCHandle.Alloc(centerLinePointsBuffer, GCHandleType.Pinned);
            wayInterop.CenterLinePoints = centerLinePointsBufferGCHandle.AddrOfPinnedObject();

            var centerLineSplineParamsBuffer = new double[wayInterop.CenterLineSplineParamsBufferSize];
            var centerLineSplineParamsBufferGCHandle = GCHandle.Alloc(centerLineSplineParamsBuffer, GCHandleType.Pinned);
            wayInterop.CenterLineSplineParams = centerLineSplineParamsBufferGCHandle.AddrOfPinnedObject();

            var classificationBuffer = new byte[wayInterop.ClassificationBufferSize];
            var classificationBufferGCHandle = GCHandle.Alloc(classificationBuffer, GCHandleType.Pinned);
            wayInterop.Classification = classificationBufferGCHandle.AddrOfPinnedObject();

            success = NativeTransportApi_TryGetWay(NativePluginRunner.API, ref wayInterop);
            if (!success)
            {
                way = TransportWay.MakeEmpty();
                return false;
            }

            var classification = Marshal.PtrToStringAnsi(wayInterop.Classification, wayInterop.ClassificationBufferSize - 1);

            way = new TransportWay(
                wayInterop.Id.FromInterop(),
                centerLinePointsBuffer,
                centerLineSplineParamsBuffer,
                wayInterop.LengthMeters,
                wayInterop.HalfWidthMeters,
                wayInterop.WayDirection,
                classification,
                wayInterop.AverageSpeedKph,
                wayInterop.ApproximateSpeedLimitKph
                );

            centerLinePointsBufferGCHandle.Free();
            centerLineSplineParamsBufferGCHandle.Free();
            classificationBufferGCHandle.Free();
            return true;
        }

        public string TransportCellKeyToString(TransportCellKey cellKey)
        {
            const int fixedBufferSize = 16; // sufficient for largest expected key
            var buffer = new byte[fixedBufferSize];
            var bufferGCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var bufferPtr = bufferGCHandle.AddrOfPinnedObject();

            var success = NativeTransportApi_TryTransportCellKeyToString(NativePluginRunner.API, cellKey.ToInterop(), fixedBufferSize, bufferPtr);
            if (!success)
            {
                throw new System.ArgumentOutOfRangeException("TransportCellKeyToString failed");
            }

            var asString = Marshal.PtrToStringAnsi(bufferPtr);
            return asString;
        }

        public void DestroyPositioner(TransportPositioner transportPositioner)
        {
            ValidateExists(transportPositioner.Id);
            m_positionerIdToObject.Remove(transportPositioner.Id);
            NativeTransportApi_DestroyPositioner(NativePluginRunner.API, transportPositioner.Id);
        }

        public void SetPositionerInputCoordinates(TransportPositioner transportPositioner, double latitudeDegrees, double longitudeDegrees)
        {
            ValidateExists(transportPositioner.Id);
            NativeTransportApi_SetPositionerInputCoordinates(NativePluginRunner.API, transportPositioner.Id, latitudeDegrees, longitudeDegrees);
        }

        public void SetPositionerInputHeading(TransportPositioner transportPositioner, double headingDegrees)
        {
            ValidateExists(transportPositioner.Id);
            NativeTransportApi_SetPositionerInputHeading(NativePluginRunner.API, transportPositioner.Id, headingDegrees);
        }

        public void ClearPositionerInputHeading(TransportPositioner transportPositioner)
        {
            ValidateExists(transportPositioner.Id);
            NativeTransportApi_ClearPositionerInputHeading(NativePluginRunner.API, transportPositioner.Id);
        }

        public bool IsPositionerMatched(TransportPositioner transportPositioner)
        {
            ValidateExists(transportPositioner.Id);
            return NativeTransportApi_IsPositionerMatched(NativePluginRunner.API, transportPositioner.Id);
        }

        public TransportPositionerPointOnGraph GetPositionerPointOnGraph(TransportPositioner transportPositioner)
        {
            var result = NativeTransportApi_GetPositionerPointOnGraph(NativePluginRunner.API, transportPositioner.Id);
            return result.FromInterop();
        }

        public delegate void TransportPositionerPointOnGraphChangedDelegate(IntPtr transportApiHandle, int transportPositionerId);
        public delegate void TransportPositionerGraphChangedDelegate(
            IntPtr transportApiHandle, 
            TransportNetworkType transportNetworkType, 
            MortonKeyInterop cellKey, 
            TransportGraphChangeReason transportGraphChangeReason);

        [MonoPInvokeCallback(typeof(TransportPositionerPointOnGraphChangedDelegate))]
        public static void OnTransportPositionerPointOnGraphChanged(IntPtr positionerApiHandle, int transportPositionerId)
        {
            var transportApiInternal = positionerApiHandle.NativeHandleToObject<TransportApiInternal>();

            transportApiInternal.NotifyTransportPositionerPointOnGraphChanged(transportPositionerId);
        }

        [MonoPInvokeCallback(typeof(TransportPositionerGraphChangedDelegate))]
        public static void OnTransportGraphChanged(
            IntPtr transportApiHandle, 
            TransportNetworkType transportNetworkType, 
            MortonKeyInterop cellKeyInterop, 
            TransportGraphChangeReason transportGraphChangeReason)
        {
            var transportApiInternal = transportApiHandle.NativeHandleToObject<TransportApiInternal>();
            var cellKey = cellKeyInterop.FromInterop();
            transportApiInternal.NotifyTransportGraphChanged(transportNetworkType, cellKey, transportGraphChangeReason);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_CreatePositioner(IntPtr ptr, ref TransportPositionerOptionsInterop options);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeTransportApi_DestroyPositioner(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_PositionerExists(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeTransportApi_SetPositionerInputCoordinates(IntPtr ptr, int positionerId, double latitudeDegrees, double longitudeDegrees);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeTransportApi_SetPositionerInputHeading(IntPtr ptr, int positionerId, double inputHeadingDegrees);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeTransportApi_ClearPositionerInputHeading(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_IsPositionerMatched(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern TransportPositionerPointOnGraphInterop NativeTransportApi_GetPositionerPointOnGraph(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern TransportPathfindResultInterop NativeTransportApi_FindShortestPath(IntPtr ptr, ref TransportPathfindOptionsInterop options);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryPopulateAndReleaseTransportPathfindResult(IntPtr ptr, ref TransportPathfindResultInterop result);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetNodeCountForNetwork(IntPtr ptr, TransportNetworkType networkType);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetDirectedEdgeCountForNetwork(IntPtr ptr, TransportNetworkType networkType);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetWayCountForNetwork(IntPtr ptr, TransportNetworkType networkType);


        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetNodeCountForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetDirectedEdgeCountForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetWayCountForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetNodeIdsForNetwork(IntPtr ptr, TransportNetworkType networkType, int bufferElementCount, IntPtr nodeIdInteropBuffer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetDirectedEdgeIdsForNetwork(IntPtr ptr, TransportNetworkType networkType, int bufferElementCount, IntPtr directedEdgeIdInteropBuffer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetWayIdsForNetwork(IntPtr ptr, TransportNetworkType networkType, int bufferElementCount, IntPtr wayIdInteropBuffer);


        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetNodeIdsForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop, int bufferElementCount, IntPtr nodeIdInteropBuffer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetDirectedEdgeIdsForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop, int bufferElementCount, IntPtr directedEdgeIdInteropBuffer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetWayIdsForNetworkInCell(IntPtr ptr, TransportNetworkType networkType, MortonKeyInterop cellKeyInterop, int bufferElementCount, IntPtr wayIdInteropBuffer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeTransportApi_GetIncidentDirectedEdgeCountForNode(IntPtr ptr, TransportNodeIdInterop nodeIdInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetNode(IntPtr ptr, ref TransportNodeInterop nodeInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetDirectedEdge(IntPtr ptr, ref TransportDirectedEdgeInterop directedEdgeInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetWayBufferSizes(IntPtr ptr, ref TransportWayInterop wayInterop);
        
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryGetWay(IntPtr ptr, ref TransportWayInterop wayInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeTransportApi_TryTransportCellKeyToString(IntPtr ptr, MortonKeyInterop cellKeyInterop, int bufferSize, IntPtr stringBufer);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeTransportApi_GetLinearInterpolationParams(IntPtr ptr, IntPtr orderedValues, int orderedValuesSize, double value, out int index0, out int index1, out double interpolationParam);
    }
}
