
namespace Wrld.Transport
{
    /// <summary>
    /// A convenience builder to construct instances of Wrld.Transport.TransportPathfindOptions.
    /// </summary>
    public class TransportPathfindOptionsBuilder
    {
        private TransportDirectedEdgeId m_directedEdgeIdA;
        private TransportDirectedEdgeId m_directedEdgeIdB;
        private double m_parameterizedPointOnEdgeA = 0.0;
        private double m_parameterizedPointOnEdgeB = 0.0;
        private bool m_uTurnAllowedAtA = true;
        private bool m_uTurnAllowedAtB = true;

        private bool m_isPointASet = false;
        private bool m_isPointBSet = false;

        public TransportPathfindOptionsBuilder()
        {

        }

        /// <summary>
        /// Set the start point from the given TransportPositionerPointOnGraph object.
        /// </summary>
        /// <param name="pointOnGraph">A TransportPositionerPointOnGraph object, for example as supplied by TransportPositioner.GetPointOnGraph()</param>
        /// <returns>This object, with the start point set.</returns>
        public TransportPathfindOptionsBuilder SetPointOnGraphA(TransportPositionerPointOnGraph pointOnGraph)
        {
            if (pointOnGraph.IsMatched)
            {
                m_directedEdgeIdA = pointOnGraph.DirectedEdgeId;
                m_parameterizedPointOnEdgeA = pointOnGraph.GetParameterOnDirectedEdge();
                m_isPointASet = true;
            }
            else
            {
                m_directedEdgeIdA = new TransportDirectedEdgeId();
                m_parameterizedPointOnEdgeA = 0.0;
                m_isPointASet = false;
            }

            return this;
        }

        /// <summary>
        /// Set the goal point from the given TransportPositionerPointOnGraph object.
        /// </summary>
        /// <param name="pointOnGraph">A TransportPositionerPointOnGraph object, for example as supplied by TransportPositioner.GetPointOnGraph()</param>
        /// <returns>This object, with the goal point set.</returns>
        public TransportPathfindOptionsBuilder SetPointOnGraphB(TransportPositionerPointOnGraph pointOnGraph)
        {
            if (pointOnGraph.IsMatched)
            {
                m_directedEdgeIdB = pointOnGraph.DirectedEdgeId;
                m_parameterizedPointOnEdgeB = pointOnGraph.GetParameterOnDirectedEdge();
                m_isPointBSet = true;
            }
            else
            {
                m_directedEdgeIdB = new TransportDirectedEdgeId();
                m_parameterizedPointOnEdgeB = 0.0;
                m_isPointBSet = false;
            }

            return this;
        }

        /// <summary>
        /// Explicitly set the start point.
        /// </summary>
        /// <param name="directedEdgeIdA">The id of a directed edge object on which the start point lies.</param>
        /// <param name="parameterizedPointOnEdgeA">A point on the start directed edge, specified as a parameterised distance along the edge, in range 0.0 to 1.0.</param>
        /// <returns>This object, with the start point set.</returns>
        public TransportPathfindOptionsBuilder SetPointOnGraphA(TransportDirectedEdgeId directedEdgeIdA, double parameterizedPointOnEdgeA)
        {
            m_directedEdgeIdA = directedEdgeIdA;
            m_parameterizedPointOnEdgeA = parameterizedPointOnEdgeA;
            m_isPointASet = true;
            return this;
        }

        /// <summary>
        /// Explicitly set the goal point.
        /// </summary>
        /// <param name="directedEdgeIdB">The id of a directed edge object on which the goal point lies.</param>
        /// <param name="parameterizedPointOnEdgeB">A point on the goal directed edge, specified as a parameterised distance along the edge, in range 0.0 to 1.0.</param>
        /// <returns>This object, with the goal point set.</returns>
        public TransportPathfindOptionsBuilder SetPointOnGraphB(TransportDirectedEdgeId directedEdgeIdB, double parameterizedPointOnEdgeB)
        {
            m_directedEdgeIdB = directedEdgeIdB;
            m_parameterizedPointOnEdgeB = parameterizedPointOnEdgeB;
            m_isPointBSet = true;
            return this;
        }

        /// <summary>
        /// Sets whether an immediate U-turn is permitted at the start point.
        /// </summary>
        /// <param name="uTurnAllowedAtA"></param>
        /// <returns>This object, with the start U-turn constraint set.</returns>
        public TransportPathfindOptionsBuilder SetUTurnAllowedAtA(bool uTurnAllowedAtA)
        {
            m_uTurnAllowedAtA = uTurnAllowedAtA;
            return this;
        }

        /// <summary>
        /// Sets whether an immediate U-turn is permitted at the goal point.
        /// </summary>
        /// <param name="uTurnAllowedAtB"></param>
        /// <returns>This object, with the goal U-turn constraint set.</returns>
        public TransportPathfindOptionsBuilder SetUTurnAllowedAtB(bool uTurnAllowedAtB)
        {
            m_uTurnAllowedAtB = uTurnAllowedAtB;
            return this;
        }

        /// <summary>
        /// Create the options object.
        /// </summary>
        /// <returns>The TransportPathfindOptions object.</returns>
        public TransportPathfindOptions Build()
        {
            if (!m_isPointASet && !m_isPointBSet)
            {
                throw new System.ArgumentException("PointOnGraphA and PointOnGraphB must be set before calling Build().");
            }

            return new TransportPathfindOptions(
                m_directedEdgeIdA,
                m_directedEdgeIdB,
                m_parameterizedPointOnEdgeA,
                m_parameterizedPointOnEdgeB,
                m_uTurnAllowedAtA,
                m_uTurnAllowedAtB
                );
        }

    };
}
