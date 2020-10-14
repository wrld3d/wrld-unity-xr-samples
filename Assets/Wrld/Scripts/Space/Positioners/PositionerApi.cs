using System;

namespace Wrld.Space.Positioners
{
    /// <summary>
    /// An API to create and receive change notification for Positioner instances.
    /// </summary>
    public class PositionerApi
    {
        /// <summary>
        /// A delegate type for event handlers receiving notification that a Positioner has changed.
        /// </summary>
        /// <param name="positioner">The Positioner instance that has changed.</param>
        public delegate void PositionerChangedHandler(Positioner positioner);

        /// <summary>
        /// Notification that the resultant point of a Positioner has changed.
        /// This may be due to the input data model of this Positioner changing (for example, by calling SetLocation); or it
        /// maybe due to a change to one of the components on which the resultant point depends - for example, if the
        /// terrain map tile that contains the LatLong location has streamed in, causing the Positioner's height
        /// above ground to be updated.
        /// An app may hook to this event in order to respond to a change to a Positioner by accessing the
        /// updated resultant point via Positioner.TryGetECEFLocation or Positioner.TryGetLatLongAltitude.
        /// </summary>
        public event PositionerChangedHandler OnPositionerTransformedPointChanged;


        /// <summary>
        /// Notification that the screen projection of the resultant point of a Positioner has changed.
        /// This may be raised every frame due to the camera view changing.
        /// An app may hook to this event in order to respond to a change to a Positioner by accessing the
        /// updated resultant projected screen-space point via Positioner.TryGetScreenPoint.
        /// </summary>
        public event PositionerChangedHandler OnPositionerScreenPointChanged;


        private PositionerApiInternal m_apiInternal;
        internal PositionerApi(PositionerApiInternal apiInternal)
        {
            m_apiInternal = apiInternal;

            m_apiInternal.OnPositionerTransformedPointChanged += (positioner) => RaiseEvent(OnPositionerTransformedPointChanged, positioner);
            m_apiInternal.OnPositionerScreenPointChanged += (positioner) => RaiseEvent(OnPositionerScreenPointChanged, positioner);
        }

        /// <summary>
        /// Creates an instance of a Positioner.
        /// </summary>
        /// <param name="positionerOptions">The PositionerOptions object which defines creation parameters for this Positioner.</param>
        public Positioner CreatePositioner(PositionerOptions positionerOptions)
        {
            return m_apiInternal.CreatePositioner(positionerOptions);
        }

        internal PositionerApiInternal GetApiInternal()
        {
            return m_apiInternal;
        }

        private static void RaiseEvent(PositionerChangedHandler eventHandler, Positioner positioner)
        {
            if (eventHandler != null)
            {
                eventHandler(positioner);
            }
        }

    }
}
