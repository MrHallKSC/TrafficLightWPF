// ============================================================================
// MAINWINDOW.XAML.CS - The Code-Behind (Event Handlers and UI Updates)
// ============================================================================
//
// ============================================================================
// WPF ORIENTATION FOR CONSOLE PROGRAMMERS
// ============================================================================
//
// If you're used to console apps, here's how WPF is different:
//
// CONSOLE APP STRUCTURE:
//   static void Main() {
//       // Your code runs top-to-bottom
//       Console.WriteLine("Hello");
//       string input = Console.ReadLine();
//       // Process input...
//   }
//
// WPF APP STRUCTURE:
//   1. App starts and creates a Window
//   2. Window displays controls defined in XAML
//   3. App waits for user interaction (button clicks, typing, etc.)
//   4. When user does something, an EVENT fires
//   5. Your EVENT HANDLER code runs
//   6. You update the UI to reflect changes
//   7. Back to waiting...
//
// KEY CONCEPTS:
//
// 1. XAML BUILDS THE UI LAYOUT
//    The .xaml file defines WHAT controls exist and WHERE they appear.
//    Think of it as the "blueprint" for your window.
//
// 2. CODE-BEHIND HANDLES EVENTS
//    This .xaml.cs file runs code WHEN SOMETHING HAPPENS.
//    Button clicked? TextBox changed? Timer ticked? Handle it here.
//
// 3. x:Name CONNECTS XAML TO C#
//    In XAML: <TextBlock x:Name="StatusText" />
//    In C#: StatusText.Text = "Running";
//    The x:Name becomes a variable you can use in code!
//
// 4. DISPATCHERTIMER FOR REPEATED ACTIONS
//    In console apps, you might use Thread.Sleep() in a loop.
//    In WPF, NEVER use Thread.Sleep() - it freezes the UI!
//    Instead, use DispatcherTimer which:
//      - Ticks at regular intervals
//      - Runs on the UI thread (safe to update controls)
//      - Doesn't freeze the interface
//
// ============================================================================

using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TrafficLightWPF
{
    /// <summary>
    /// Code-behind for MainWindow.xaml.
    /// Handles button clicks, timer ticks, and updates the UI.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ====================================================================
        // FIELDS - Objects we need to keep track of
        // ====================================================================

        /// <summary>
        /// The traffic light controller - contains all the logic.
        /// This is the "console app part" that we're connecting to the UI.
        /// </summary>
        private readonly TrafficLightController _controller;

        /// <summary>
        /// Timer that ticks regularly to advance the simulation.
        /// Think of it like a heartbeat for the app.
        /// </summary>
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Tracks when the last tick occurred.
        /// Used to calculate elapsed time between ticks.
        /// </summary>
        private DateTime _lastTickTime;

        // ====================================================================
        // COLOUR CONSTANTS - Bright (ON) and Dim (OFF) colours for lamps
        // ====================================================================

        // In WPF, colours are represented by Brush objects.
        // SolidColorBrush fills a shape with a single colour.
        // Brushes class provides pre-defined common colours.
        // We can also create custom colours using Color.FromRgb(r, g, b).

        /// <summary>Bright red - lamp ON</summary>
        private static readonly Brush RedOn = Brushes.Red;

        /// <summary>Dim red - lamp OFF</summary>
        private static readonly Brush RedOff = new SolidColorBrush(Color.FromRgb(80, 0, 0));

        /// <summary>Bright amber/yellow - lamp ON</summary>
        private static readonly Brush AmberOn = Brushes.Orange;

        /// <summary>Dim amber - lamp OFF</summary>
        private static readonly Brush AmberOff = new SolidColorBrush(Color.FromRgb(120, 80, 0));

        /// <summary>Bright green - lamp ON</summary>
        private static readonly Brush GreenOn = Brushes.Lime;

        /// <summary>Dim green - lamp OFF</summary>
        private static readonly Brush GreenOff = new SolidColorBrush(Color.FromRgb(0, 60, 0));

        // ====================================================================
        // CONSTRUCTOR - Runs when the window is created
        // ====================================================================

        /// <summary>
        /// Constructor - initialises the window.
        /// </summary>
        public MainWindow()
        {
            // InitializeComponent() is AUTO-GENERATED from the XAML file.
            // It creates all the controls defined in XAML and makes them
            // available as fields (e.g., StartButton, RedLamp, etc.)
            // ALWAYS call this first!
            InitializeComponent();

            // Create our controller (the logic/state machine)
            _controller = new TrafficLightController();

            // Set up the timer
            // DispatcherTimer is WPF's way of doing repeated tasks safely
            _timer = new DispatcherTimer
            {
                // Tick every 250 milliseconds (4 times per second)
                // This gives smooth updates while the controller handles
                // the actual second-by-second countdown internally
                Interval = TimeSpan.FromMilliseconds(250)
            };

            // Connect the timer's Tick event to our handler method
            // When the timer ticks, Timer_Tick will be called
            _timer.Tick += Timer_Tick;

            // Set initial UI state
            UpdateUI();
        }

        // ====================================================================
        // HELPER METHOD - Read configuration from UI inputs
        // ====================================================================

        /// <summary>
        /// Reads the current values from the input controls and creates a config object.
        /// </summary>
        /// <returns>A TrafficLightConfig with values from the UI.</returns>
        /// <remarks>
        /// This bridges the UI (XAML controls) to the logic (controller).
        /// The config class handles parsing and validation.
        /// </remarks>
        private TrafficLightConfig GetCurrentConfig()
        {
            // Read text from TextBoxes and checkbox state
            // TrafficLightConfig.FromUserInput handles parsing safely
            return TrafficLightConfig.FromUserInput(
                RedDurationInput.Text,
                GreenDurationInput.Text,
                AmberDurationInput.Text,
                UKSequenceCheckbox.IsChecked ?? true  // ?? true means "if null, use true"
            );
        }

        // ====================================================================
        // BUTTON EVENT HANDLERS
        // ====================================================================
        //
        // Each button in XAML has Click="MethodName" which connects it to
        // one of these methods. When the button is clicked, the method runs.
        //
        // The pattern is always:
        //   1. Tell the controller what to do
        //   2. Update the UI to reflect the new state
        //   3. Start/stop the timer if needed
        // ====================================================================

        /// <summary>
        /// Handles the Start button click.
        /// Begins the simulation from Red state.
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Get current configuration from UI
            var config = GetCurrentConfig();

            // Tell the controller to start
            _controller.Start(config);

            // Record the time so we can calculate elapsed time on first tick
            _lastTickTime = DateTime.Now;

            // Start the timer
            _timer.Start();

            // Update the display
            UpdateUI();

            // Update button states
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Pause button click.
        /// Pauses the simulation (timer stops, state preserved).
        /// </summary>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            // Tell controller to pause
            _controller.Pause();

            // Stop the timer
            _timer.Stop();

            // Update display
            UpdateUI();
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Resume button click.
        /// Continues a paused simulation.
        /// </summary>
        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            // Tell controller to resume
            _controller.Resume();

            // Record time for next tick calculation
            _lastTickTime = DateTime.Now;

            // Restart timer
            _timer.Start();

            // Update display
            UpdateUI();
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Step button click.
        /// Advances exactly one state transition (works even when paused).
        /// </summary>
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            // Get current configuration
            var config = GetCurrentConfig();

            // Tell controller to step
            _controller.Step(config);

            // Update display (don't start timer - step is manual)
            UpdateUI();
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Reset button click.
        /// Returns to Red state, stops simulation.
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Get current configuration
            var config = GetCurrentConfig();

            // Tell controller to reset
            _controller.Reset(config);

            // Stop timer
            _timer.Stop();

            // Update display
            UpdateUI();
            UpdateButtonStates();
        }

        // ====================================================================
        // TIMER EVENT HANDLER
        // ====================================================================

        /// <summary>
        /// Called every time the timer ticks (every 250ms).
        /// Advances the simulation and updates the display.
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Calculate how much time has passed since last tick
            var now = DateTime.Now;
            var elapsed = now - _lastTickTime;
            _lastTickTime = now;

            // Get current configuration (in case user changed values)
            var config = GetCurrentConfig();

            // Tell controller time has passed
            _controller.Tick(elapsed, config);

            // Update the display
            UpdateUI();
        }

        // ====================================================================
        // UI UPDATE METHODS
        // ====================================================================

        /// <summary>
        /// Updates all visual elements based on controller state.
        /// This is where we "connect" the controller's state to the UI.
        /// </summary>
        /// <remarks>
        /// The controller knows WHAT state we're in.
        /// This method knows HOW to display that state.
        /// This separation keeps the controller UI-agnostic.
        /// </remarks>
        private void UpdateUI()
        {
            // Update the lamp colours based on current state
            UpdateLamps(_controller.CurrentState);

            // Update the state text (e.g., "RED", "GREEN")
            StateText.Text = GetStateDisplayText(_controller.CurrentState);

            // Update the countdown text
            CountdownText.Text = $"{_controller.RemainingSeconds}s remaining";

            // Update the status text
            StatusText.Text = GetStatusText();
        }

        /// <summary>
        /// Updates the lamp colours (Fill property) based on state.
        /// </summary>
        /// <param name="state">The current traffic light state.</param>
        private void UpdateLamps(TrafficLightState state)
        {
            // Use the controller's helper methods to determine which lamps are on
            // Then set the Fill property of each Ellipse accordingly

            // Red lamp: bright if on, dim if off
            if (TrafficLightController.IsRedOn(state))
            {
                RedLamp.Fill = RedOn;
            }
            else
            {
                RedLamp.Fill = RedOff;
            }

            // Amber lamp
            if (TrafficLightController.IsAmberOn(state))
            {
                AmberLamp.Fill = AmberOn;
            }
            else
            {
                AmberLamp.Fill = AmberOff;
            }

            // Green lamp
            if (TrafficLightController.IsGreenOn(state))
            {
                GreenLamp.Fill = GreenOn;
            }
            else
            {
                GreenLamp.Fill = GreenOff;
            }
        }

        /// <summary>
        /// Gets display text for a state (converts enum to readable string).
        /// </summary>
        private static string GetStateDisplayText(TrafficLightState state)
        {
            // Convert enum value to display text
            if (state == TrafficLightState.Red)
            {
                return "RED";
            }
            else if (state == TrafficLightState.RedAmber)
            {
                return "RED + AMBER";
            }
            else if (state == TrafficLightState.Green)
            {
                return "GREEN";
            }
            else if (state == TrafficLightState.Amber)
            {
                return "AMBER";
            }
            else
            {
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// Gets the current status text based on controller state.
        /// </summary>
        private string GetStatusText()
        {
            if (!_controller.HasStarted)
            {
                return "Stopped";
            }
            else if (_controller.IsRunning)
            {
                return "Running";
            }
            else
            {
                return "Paused";
            }
        }

        /// <summary>
        /// Enables/disables buttons based on current state.
        /// </summary>
        /// <remarks>
        /// This prevents invalid actions:
        /// - Can't Pause if not running
        /// - Can't Resume if already running
        /// - Can't Start if already started
        /// </remarks>
        private void UpdateButtonStates()
        {
            // Start: only enabled if not started yet
            StartButton.IsEnabled = !_controller.HasStarted;

            // Pause: only enabled if currently running
            PauseButton.IsEnabled = _controller.IsRunning;

            // Resume: only enabled if started but paused
            ResumeButton.IsEnabled = _controller.HasStarted && !_controller.IsRunning;

            // Step and Reset are always available
            StepButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
        }
    }
}
