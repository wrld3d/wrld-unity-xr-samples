
namespace Wrld.Transport
{
    static internal class TransportApiInteropExtensions
    {
        public static TransportCellKey FromInterop(this MortonKeyInterop interop)
        {
            return new TransportCellKey
            {
                Value = interop.Value
            };
        }

        public static MortonKeyInterop ToInterop(this TransportCellKey cellKey)
        {
            return new MortonKeyInterop
            {
                Value = cellKey.Value
            };
        }

        public static TransportNodeId FromInterop(this TransportNodeIdInterop interop)
        {
            return new TransportNodeId
            {
                CellKey = FromInterop(interop.CellKey),
                NetworkType = interop.TransportNetworkType,
                LocalNodeId = interop.LocalNodeId
            };
        }

        public static TransportNodeIdInterop ToInterop(this TransportNodeId nodeId)
        {
            return new TransportNodeIdInterop
            {
                CellKey = ToInterop(nodeId.CellKey),
                LocalNodeId = nodeId.LocalNodeId,
                TransportNetworkType = nodeId.NetworkType
            };
        }

        public static TransportDirectedEdgeId FromInterop(this TransportDirectedEdgeIdInterop interop)
        {
            return new TransportDirectedEdgeId
            {
                CellKey = interop.CellKey.FromInterop(),
                NetworkType = interop.TransportNetworkType,
                LocalDirectedEdgeId = interop.LocalDirectedEdgeId
            };
        }

        public static TransportDirectedEdgeIdInterop ToInterop(this TransportDirectedEdgeId directedEdgeId)
        {
            return new TransportDirectedEdgeIdInterop
            {
                CellKey = directedEdgeId.CellKey.ToInterop(),
                LocalDirectedEdgeId = directedEdgeId.LocalDirectedEdgeId,
                TransportNetworkType = directedEdgeId.NetworkType
            };
        }


        public static TransportWayId FromInterop(this TransportWayIdInterop interop)
        {
            return new TransportWayId
            {
                CellKey = interop.CellKey.FromInterop(),
                NetworkType = interop.TransportNetworkType,
                LocalWayId = interop.LocalWayId
            };
        }

        public static TransportWayIdInterop ToInterop(this TransportWayId wayId)
        {
            return new TransportWayIdInterop
            {
                CellKey = ToInterop(wayId.CellKey),
                LocalWayId = wayId.LocalWayId,
                TransportNetworkType = wayId.NetworkType
            };
        }


        public static TransportPositionerPointOnGraph FromInterop(this TransportPositionerPointOnGraphInterop interop)
        {
            return new TransportPositionerPointOnGraph(
                interop.IsMatched,
                interop.IsWayReversed,
                FromInterop(interop.DirectedEdgeId),
                interop.ParameterizedPointOnWay,
                interop.PointOnWay,
                interop.DirectionOnWay,
                interop.HeadingOnWayDegrees
                );
        }

        public static TransportPositionerOptionsInterop ToInterop(this TransportPositionerOptions options)
        {
            var optionsInterop = new TransportPositionerOptionsInterop
            {
                InputLatitudeDegrees = options.InputLatitudeDegrees,
                InputLongitudeDegrees = options.InputLongitudeDegrees,
                HasHeading = options.HasHeading,
                InputHeadingDegrees = options.InputHeadingDegrees,
                MaxDistanceToMatchedPointMeters = options.MaxDistanceToMatchedPointMeters,
                MaxHeadingDeviationToMatchedPointDegrees = options.MaxHeadingDeviationToMatchedPointDegrees,
                TransportNetworkType = options.TransportNetworkType
            };

            return optionsInterop;
        }

        public static TransportPathfindOptionsInterop ToInterop(this TransportPathfindOptions options)
        {
            return new TransportPathfindOptionsInterop
            {
                DirectedEdgeA = options.DirectedEdgeIdA.ToInterop(),
                DirectedEdgeB = options.DirectedEdgeIdB.ToInterop(),
                ParameterizedPointOnEdgeA = options.ParameterizedPointOnEdgeA,
                ParameterizedPointOnEdgeB = options.ParameterizedPointOnEdgeB,
                UTurnAllowedAtA = options.UTurnAllowedAtA,
                UTurnAllowedAtB = options.UTurnAllowedAtB
            };
        }
    }
}
