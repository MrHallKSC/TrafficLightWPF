// ============================================================================
// APP.XAML.CS - Application Code-Behind
// ============================================================================
//
// FOR STUDENTS:
// This is the "code-behind" file for App.xaml. Every XAML file can have a
// matching .cs file that contains the C# code for that XAML.
//
// The link between them:
//   - App.xaml says: x:Class="TrafficLightWPF.App"
//   - This file defines: public partial class App
//   - The "partial" keyword means the class is split across two files
//     (the XAML generates some code, and you write the rest here)
//
// For this simple app, we don't need any custom startup code here.
// WPF handles everything based on the StartupUri in App.xaml.
//
// In more complex apps, you might override OnStartup() to:
//   - Load configuration
//   - Set up logging
//   - Handle command-line arguments
// ============================================================================

using System.Windows;

namespace TrafficLightWPF
{
    /// <summary>
    /// The main Application class for the WPF app.
    /// Inherits from System.Windows.Application which provides
    /// all the plumbing for running a WPF application.
    /// </summary>
    public partial class App : Application
    {
        // No custom code needed for this simple example.
        // The base Application class handles everything!
    }
}
