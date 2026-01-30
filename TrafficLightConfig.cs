// ============================================================================
// TRAFFICLIGHTCONFIG.CS - Configuration Class
// ============================================================================
//
// FOR STUDENTS:
// This class stores the configuration settings for the traffic light simulation.
// Again, this is plain C# - no WPF types here!
//
// WHY A SEPARATE CONFIG CLASS?
// Instead of passing lots of individual parameters around (redSeconds,
// greenSeconds, amberSeconds, useUK...), we bundle them into one object.
//
// Benefits:
//   1. Cleaner method signatures: Start(config) vs Start(red, green, amber, uk)
//   2. Easy to add new settings later without changing method signatures
//   3. Validation logic lives in one place
//   4. Easy to save/load settings (not implemented here, but possible)
//
// VALIDATION:
// User input is always risky! Someone might type "abc" instead of "6".
// This class includes helpers to parse strings safely and clamp values
// to sensible ranges.
// ============================================================================

namespace TrafficLightWPF
{
    /// <summary>
    /// Stores configuration settings for the traffic light simulation.
    /// All timing values are in seconds.
    /// </summary>
    public class TrafficLightConfig
    {
        // ====================================================================
        // CONSTANTS - Define sensible limits
        // ====================================================================

        /// <summary>
        /// Minimum duration for any light phase (1 second).
        /// Anything less would be too fast to see!
        /// </summary>
        public const int MinimumDuration = 1;

        /// <summary>
        /// Maximum duration for any light phase (60 seconds).
        /// Prevents silly values like 9999 seconds.
        /// </summary>
        public const int MaximumDuration = 60;

        /// <summary>
        /// Default duration for the red light (seconds).
        /// </summary>
        public const int DefaultRedSeconds = 6;

        /// <summary>
        /// Default duration for the green light (seconds).
        /// </summary>
        public const int DefaultGreenSeconds = 6;

        /// <summary>
        /// Default duration for the amber light (seconds).
        /// </summary>
        public const int DefaultAmberSeconds = 2;

        // ====================================================================
        // PROPERTIES - The actual settings
        // ====================================================================

        /// <summary>
        /// How long the red light stays on (seconds).
        /// </summary>
        public int RedSeconds { get; set; } = DefaultRedSeconds;

        /// <summary>
        /// How long the green light stays on (seconds).
        /// </summary>
        public int GreenSeconds { get; set; } = DefaultGreenSeconds;

        /// <summary>
        /// How long the amber light stays on (seconds).
        /// Also used for RedAmber in UK sequence.
        /// </summary>
        public int AmberSeconds { get; set; } = DefaultAmberSeconds;

        /// <summary>
        /// If true, use UK sequence (Red -> RedAmber -> Green -> Amber).
        /// If false, use simple sequence (Red -> Green -> Amber).
        /// </summary>
        public bool UseUKSequence { get; set; } = true;

        // ====================================================================
        // VALIDATION HELPERS - Safe parsing and clamping
        // ====================================================================

        /// <summary>
        /// Safely parses a string to an integer, returning a default value if parsing fails.
        /// </summary>
        /// <param name="text">The text to parse (e.g., from a TextBox).</param>
        /// <param name="defaultValue">Value to return if parsing fails.</param>
        /// <returns>The parsed integer, or defaultValue if parsing failed.</returns>
        /// <remarks>
        /// This is much safer than int.Parse() which throws an exception on bad input.
        /// int.TryParse() returns false instead of crashing.
        /// </remarks>
        public static int ParseSafely(string text, int defaultValue)
        {
            // TryParse returns true if successful, and puts the result in 'result'
            // If it fails (e.g., text is "abc"), it returns false and result is 0
            if (int.TryParse(text, out int result))
            {
                return result;
            }

            // Parsing failed - return the safe default
            return defaultValue;
        }

        /// <summary>
        /// Clamps a value to be within the allowed range [MinimumDuration, MaximumDuration].
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value, constrained to the valid range.</returns>
        /// <remarks>
        /// Clamping means: if value is too low, use minimum; if too high, use maximum.
        /// Example: Clamp(0) -> 1, Clamp(100) -> 60, Clamp(5) -> 5
        /// </remarks>
        public static int ClampDuration(int value)
        {
            // Math.Clamp is a handy method: Clamp(value, min, max)
            // Returns min if value < min, max if value > max, otherwise value
            return Math.Clamp(value, MinimumDuration, MaximumDuration);
        }

        /// <summary>
        /// Creates a config object by parsing text from UI input fields.
        /// Handles invalid input gracefully by using defaults and clamping.
        /// </summary>
        /// <param name="redText">Red duration text from TextBox.</param>
        /// <param name="greenText">Green duration text from TextBox.</param>
        /// <param name="amberText">Amber duration text from TextBox.</param>
        /// <param name="useUKSequence">Whether to use UK sequence.</param>
        /// <returns>A valid TrafficLightConfig object.</returns>
        public static TrafficLightConfig FromUserInput(
            string redText,
            string greenText,
            string amberText,
            bool useUKSequence)
        {
            // Parse each value, using defaults for invalid input
            int red = ParseSafely(redText, DefaultRedSeconds);
            int green = ParseSafely(greenText, DefaultGreenSeconds);
            int amber = ParseSafely(amberText, DefaultAmberSeconds);

            // Clamp to valid range (prevents 0-second or 999-second lights)
            red = ClampDuration(red);
            green = ClampDuration(green);
            amber = ClampDuration(amber);

            // Create and return the config object
            return new TrafficLightConfig
            {
                RedSeconds = red,
                GreenSeconds = green,
                AmberSeconds = amber,
                UseUKSequence = useUKSequence
            };
        }
    }
}
