// ============================================================================
// TRAFFICLIGHTSTATE.CS - State Enumeration
// ============================================================================
//
// FOR STUDENTS:
// This is a simple enum - exactly the same as you'd use in a console app!
// Nothing WPF-specific here at all.
//
// The traffic light can be in one of four states:
//   - Red:      Stop (red light on)
//   - RedAmber: Get ready to go (red AND amber on) - UK sequence only
//   - Green:    Go (green light on)
//   - Amber:    Prepare to stop (amber light on)
//
// STATE MACHINE CONCEPT:
// A state machine is a common pattern where:
//   1. Your system can be in exactly ONE state at a time
//   2. There are defined rules for transitioning between states
//   3. Each state determines what the system does/shows
//
// This is the same concept used in:
//   - Game character states (Idle, Walking, Jumping, Attacking)
//   - Order processing (Pending, Confirmed, Shipped, Delivered)
//   - UI modes (Editing, Viewing, Loading)
// ============================================================================

namespace TrafficLightWPF
{
    /// <summary>
    /// Represents the possible states of a traffic light.
    /// </summary>
    /// <remarks>
    /// UK Sequence: Red -> RedAmber -> Green -> Amber -> Red (and repeat)
    /// Simple Sequence: Red -> Green -> Amber -> Red (and repeat)
    /// </remarks>
    public enum TrafficLightState
    {
        /// <summary>
        /// Red light - vehicles must stop.
        /// </summary>
        Red,

        /// <summary>
        /// Red and Amber together - prepare to go (UK sequence only).
        /// This warns drivers that green is coming soon.
        /// </summary>
        RedAmber,

        /// <summary>
        /// Green light - vehicles may proceed.
        /// </summary>
        Green,

        /// <summary>
        /// Amber light - prepare to stop (unless unsafe to do so).
        /// </summary>
        Amber
    }
}
