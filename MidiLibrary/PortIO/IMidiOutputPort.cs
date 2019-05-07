using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.PortIO
{
    /// <summary>
    /// Interface that all implementations of a midi output port must satisfy
    /// </summary>
    public interface IMidiOutputPort : IDisposable
    {
        // Name of the port
        string Name {  get; }

        // Volume
        uint Volume { get; set; }

        // Open the port
        string Open();

        // Open the port with an event handler
        string Open(EventHandler<IMidiEventArgs> handler);

        // Reset the port
        string Reset();

        // Send a message
        string Send(MidiMessage m);

        // Close the port
        string Close();
    }
}
