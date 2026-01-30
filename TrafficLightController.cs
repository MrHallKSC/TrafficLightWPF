// ============================================================================
// TRAFFICLIGHTCONTROLLER.CS - Core Logic / State Machine
// ============================================================================
//
// FOR STUDENTS:
// This is the HEART of the application - the state machine and countdown logic.
//
// *** IMPORTANT: NO WPF TYPES IN THIS FILE! ***
//
// Notice there's no "using System.Windows..." at the top. This class knows
// NOTHING about WPF, XAML, buttons, or colours. It just manages:
//   - What state the traffic light is in
//   - How many seconds are remaining
//   - Whether the simulation is running or paused
//
// WHY KEEP LOGIC SEPARATE FROM UI?
// This is called "Separation of Concerns" - a fundamental principle:
//
//   1. TESTABILITY: You could write unit tests for this class without
//      needing to create any UI. Just call methods and check properties.
//
//   2. REUSABILITY: This same controller could work with:
//      - A different UI (WinForms, web, console)
//      - A physical traffic light (with hardware control)
//      - A network-controlled system
//
//   3. MAINTAINABILITY: If the traffic light rules change, you edit THIS file.
//      If the UI changes, you edit MainWindow. They don't interfere.
//
//   4. CLARITY: Each file has ONE job. This file = logic. MainWindow = display.
//
// THINK OF IT LIKE A CONSOLE APP:
// If you were writing this as a console app, you'd have similar code:
//   - A variable for current state
//   - A variable for countdown
//   - Methods to advance the state
// The only difference is a console app would use Console.WriteLine to display,
// while WPF will update visual controls. But the LOGIC is identical!
// ============================================================================

namespace TrafficLightWPF
{
    /// <summary>
    /// Controls the traffic light state machine and countdown timer.
    /// This class contains all the simulation logic with NO UI dependencies.
    /// </summary>
    public class TrafficLightController
    {
        // ====================================================================
        // PRIVATE FIELDS - Internal tracking
        // ====================================================================

        /// <summary>
        /// Accumulates elapsed time between ticks.
        /// We need this because the timer might tick faster than once per second
        /// (e.g., every 250ms), but we only decrement the countdown once per full second.
        /// </summary>
        private double _elapsedMilliseconds = 0;

        /// <summary>
        /// One second in milliseconds - used for countdown calculations.
        /// </summary>
        private const double OneSecondMs = 1000.0;

        // ====================================================================
        // PUBLIC PROPERTIES - Exposed to the UI
        // ====================================================================

        /// <summary>
        /// The current state of the traffic light.
        /// The UI reads this to know which lamps to illuminate.
        /// </summary>
        public TrafficLightState CurrentState { get; private set; } = TrafficLightState.Red;

        /// <summary>
        /// Seconds remaining in the current state before transitioning.
        /// The UI displays this to the user.
        /// </summary>
        public int RemainingSeconds { get; private set; } = 0;

        /// <summary>
        /// Whether the simulation is currently running (not paused).
        /// The UI uses this to enable/disable buttons and show status.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Whether the simulation has been started at least once.
        /// Used to distinguish "not started" from "paused".
        /// </summary>
        public bool HasStarted { get; private set; } = false;

        // ====================================================================
        // CONTROL METHODS - Called by the UI when buttons are clicked
        // ====================================================================

        /// <summary>
        /// Starts the simulation from the beginning (Red state).
        /// </summary>
        /// <param name="config">Configuration with timing settings.</param>
        public void Start(TrafficLightConfig config)
        {
            // Begin in the Red state
            CurrentState = TrafficLightState.Red;

            // Set the countdown to the red light duration
            RemainingSeconds = GetDurationForState(CurrentState, config);

            // Reset the millisecond accumulator
            _elapsedMilliseconds = 0;

            // Mark as running and started
            IsRunning = true;
            HasStarted = true;
        }

        /// <summary>
        /// Pauses the simulation. Timer stops ticking, but state is preserved.
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Resumes a paused simulation.
        /// </summary>
        public void Resume()
        {
            // Only resume if we've actually started
            if (HasStarted)
            {
                IsRunning = true;
            }
        }

        /// <summary>
        /// Resets the simulation back to initial state (Red, stopped).
        /// </summary>
        /// <param name="config">Configuration with timing settings.</param>
        public void Reset(TrafficLightConfig config)
        {
            // Go back to Red state
            CurrentState = TrafficLightState.Red;

            // Reset countdown
            RemainingSeconds = GetDurationForState(CurrentState, config);

            // Reset accumulator
            _elapsedMilliseconds = 0;

            // Stop running but remember we've been started
            IsRunning = false;
            HasStarted = false;
        }

        /// <summary>
        /// Immediately advances to the next state (manual step).
        /// Works even when paused - useful for debugging/demonstration.
        /// </summary>
        /// <param name="config">Configuration with timing and sequence settings.</param>
        public void Step(TrafficLightConfig config)
        {
            // Mark as started (in case we're stepping before Start was called)
            HasStarted = true;

            // Move to the next state
            CurrentState = GetNextState(CurrentState, config.UseUKSequence);

            // Reset countdown for the new state
            RemainingSeconds = GetDurationForState(CurrentState, config);

            // Reset accumulator
            _elapsedMilliseconds = 0;
        }

        /// <summary>
        /// Called regularly by the UI's timer to advance the simulation.
        /// </summary>
        /// <param name="elapsed">Time elapsed since last tick.</param>
        /// <param name="config">Configuration with timing and sequence settings.</param>
        /// <remarks>
        /// The timer might tick every 250ms, but we only want the countdown
        /// to decrease once per full second. So we accumulate milliseconds
        /// and only act when we've accumulated a full second.
        /// </remarks>
        public void Tick(TimeSpan elapsed, TrafficLightConfig config)
        {
            // If not running, do nothing
            if (!IsRunning)
            {
                return;
            }

            // Add the elapsed time to our accumulator
            _elapsedMilliseconds += elapsed.TotalMilliseconds;

            // Process full seconds
            while (_elapsedMilliseconds >= OneSecondMs)
            {
                // Subtract one second from accumulator
                _elapsedMilliseconds -= OneSecondMs;

                // Decrease the countdown
                RemainingSeconds--;

                // Check if it's time to transition
                if (RemainingSeconds <= 0)
                {
                    // Move to next state
                    CurrentState = GetNextState(CurrentState, config.UseUKSequence);

                    // Reset countdown for new state
                    RemainingSeconds = GetDurationForState(CurrentState, config);
                }
            }
        }

        // ====================================================================
        // STATE MACHINE LOGIC - Methods for state transitions
        // ====================================================================

        /// <summary>
        /// Determines the next state in the sequence.
        /// </summary>
        /// <param name="currentState">The current traffic light state.</param>
        /// <param name="useUKSequence">True for UK sequence, false for simple.</param>
        /// <returns>The next state in the sequence.</returns>
        /// <remarks>
        /// UK Sequence:    Red -> RedAmber -> Green -> Amber -> Red -> ...
        /// Simple Sequence: Red -> Green -> Amber -> Red -> ...
        ///
        /// This is a PURE FUNCTION: same inputs always give same output.
        /// It doesn't modify any state - it just calculates and returns.
        /// </remarks>
        public static TrafficLightState GetNextState(TrafficLightState currentState, bool useUKSequence)
        {
            if (useUKSequence)
            {
                // UK sequence: Red -> RedAmber -> Green -> Amber -> Red
                if (currentState == TrafficLightState.Red)
                {
                    return TrafficLightState.RedAmber;
                }
                else if (currentState == TrafficLightState.RedAmber)
                {
                    return TrafficLightState.Green;
                }
                else if (currentState == TrafficLightState.Green)
                {
                    return TrafficLightState.Amber;
                }
                else if (currentState == TrafficLightState.Amber)
                {
                    return TrafficLightState.Red;
                }
                else
                {
                    // Default fallback (should never happen)
                    return TrafficLightState.Red;
                }
            }
            else
            {
                // Simple sequence: Red -> Green -> Amber -> Red (skip RedAmber)
                if (currentState == TrafficLightState.Red)
                {
                    return TrafficLightState.Green;
                }
                else if (currentState == TrafficLightState.RedAmber)
                {
                    // In case we switched sequence mid-cycle, go to Green
                    return TrafficLightState.Green;
                }
                else if (currentState == TrafficLightState.Green)
                {
                    return TrafficLightState.Amber;
                }
                else if (currentState == TrafficLightState.Amber)
                {
                    return TrafficLightState.Red;
                }
                else
                {
                    // Default fallback (should never happen)
                    return TrafficLightState.Red;
                }
            }
        }

        /// <summary>
        /// Gets the duration (in seconds) for a given state.
        /// </summary>
        /// <param name="state">The traffic light state.</param>
        /// <param name="config">Configuration with timing settings.</param>
        /// <returns>Duration in seconds for that state.</returns>
        /// <remarks>
        /// RedAmber uses the same duration as Amber (typically short).
        /// </remarks>
        public static int GetDurationForState(TrafficLightState state, TrafficLightConfig config)
        {
            if (state == TrafficLightState.Red)
            {
                return config.RedSeconds;
            }
            else if (state == TrafficLightState.RedAmber)
            {
                // RedAmber is brief, like Amber
                return config.AmberSeconds;
            }
            else if (state == TrafficLightState.Green)
            {
                return config.GreenSeconds;
            }
            else if (state == TrafficLightState.Amber)
            {
                return config.AmberSeconds;
            }
            else
            {
                // Default fallback
                return config.RedSeconds;
            }
        }

        // ====================================================================
        // LAMP STATE HELPERS - Which lamps should be on?
        // ====================================================================

        /// <summary>
        /// Determines if the red lamp should be illuminated.
        /// </summary>
        /// <param name="state">Current traffic light state.</param>
        /// <returns>True if red lamp should be on.</returns>
        public static bool IsRedOn(TrafficLightState state)
        {
            // Red is on during Red and RedAmber states
            if (state == TrafficLightState.Red || state == TrafficLightState.RedAmber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the amber lamp should be illuminated.
        /// </summary>
        /// <param name="state">Current traffic light state.</param>
        /// <returns>True if amber lamp should be on.</returns>
        public static bool IsAmberOn(TrafficLightState state)
        {
            // Amber is on during Amber and RedAmber states
            if (state == TrafficLightState.Amber || state == TrafficLightState.RedAmber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the green lamp should be illuminated.
        /// </summary>
        /// <param name="state">Current traffic light state.</param>
        /// <returns>True if green lamp should be on.</returns>
        public static bool IsGreenOn(TrafficLightState state)
        {
            // Green is only on during Green state
            if (state == TrafficLightState.Green)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
