using System;

namespace MidiLibrary.PortIO
{
    /// <summary>
    /// Interface that all implementations of a midi output port must satisfy
    /// </summary>
    public interface IMidiInputPort : IDisposable
    {
        // Event activated when an event is received
        event EventHandler<IMidiEventArgs> MidiInputReceived;

        // Name of the port
        string Name { get; }

        // Open the port
        string Open();

        // Start the port
        string Start();

        // Stop the port
        string Stop();

        // Reset the port
        string Reset();

        // Close the port
        string Close();
    }
}
