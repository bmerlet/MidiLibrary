﻿//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MidiLibrary.PortIO
{
    /// <summary>
    /// Open and closes midi input ports based on whether they are physically connected or not
    /// and whether their name is present or not in a list. An event is raised whenever an opened
    /// port receives input.
    /// </summary>
    public class MidiInputManager : IDisposable
    {
        #region Private members

        // List of opened ports
        private List<IMidiInputPort> openedPorts;

        #endregion

        #region Constructor

        public MidiInputManager()
        {
            this.openedPorts = new List<IMidiInputPort>();
            this.midiInput = null;
        }

        #endregion

        #region Events

        // Event raised when ports are opened or closed and supporting event arg class
        public event EventHandler<OpenPortsChangedEventArgs> OpenPortsChanged;

        public class OpenPortsChangedEventArgs : EventArgs
        {
            public OpenPortsChangedEventArgs(int openPortCount)
            {
                OpenPortCount = openPortCount;
            }

            public int OpenPortCount
            {
                get;
                private set;
            }
        }

        // Event raised when one of the opened ports receives midi data and event arg supporting class
        public event EventHandler<MidiInputEventArgs> midiInput = null;

        public class MidiInputEventArgs : EventArgs
        {
            public MidiInputEventArgs(MidiMessage message)
            {
                Message = message;
            }

            public MidiMessage Message
            {
                get;
                private set;
            }
        }

        #endregion

        #region Public methods

        // Function managing the ports. This is meant to be called in the background (e.g. on a timer)
        // to check for newly connected/disconnected ports. It should also be called when the list of
        // ports to open changes.
        public void Manage(StringCollection namesOfportsToOpen)
        {
            bool portsChanged = false;

            // Safety
            if (namesOfportsToOpen == null)
            {
                namesOfportsToOpen = new StringCollection();
            }

            var currentPorts = new List<IMidiInputPort>(PortEnumerator.InputPorts);

            // Look at all the opened ports and remove the ones that are not connected or
            // that are not in the list of ports to open
            var oldOpenedPorts = openedPorts;
            openedPorts = new List<IMidiInputPort>();
            foreach (var port in oldOpenedPorts)
            {
                if (!namesOfportsToOpen.Contains(port.Name))
                {
                    // This port does not belong to the list of ports to open - close it
                    Console.WriteLine("MidiInputManager: Closing Midi In port " + port.Name + ": " + port.Name + " because it is not on the list of ports to open");
                    port.Close();
                    portsChanged = true;
                }
                else if (currentPorts.Find(p => p.Name == port.Name) == null)
                {
                    // This port is disconnected - close it
                    Console.WriteLine("MidiInputManager: Closing Midi In port " + port.Name + ": " + port.Name + " because it is disconnected");
                    port.Close();
                    portsChanged = true;
                }
                else
                {
                    // This port is connected and is on the list of ports to open, keep it
                    openedPorts.Add(port);
                }
            }

            // Look at all the connected ports and open the ones that are to be opened
            foreach (var port in currentPorts)
            {
                // See if we need to open this port
                if (namesOfportsToOpen.Contains(port.Name))
                {
                    // Yes, we need to. See if this port is open
                    if (openedPorts.Find(p => p.Name == port.Name) == null)
                    {
                        // Port not open. We need to open it
                        port.MidiInputReceived += MidiInCallback;
                        var error = port.Open();
                        if (error == null)
                        {
                            Console.WriteLine("MidiInputManager: Opened Midi In port {0}", port.Name);
                            port.Start();
                            openedPorts.Add(port);
                            portsChanged = true;
                        }
                        else
                        {
                            Console.WriteLine("MidiInputManager: Failed to open Midi In port {0}: {1}", port.Name, error);
                        }
                    }
                }
            }

            if (portsChanged && OpenPortsChanged != null)
            {
                Console.WriteLine("MidiInputManager: Raising OpenedPortsChanged event");
                OpenPortsChanged.Invoke(this, new OpenPortsChangedEventArgs(openedPorts.Count));
            }
        }

        // Midi events callback: activate the midiInput event
        public void MidiInCallback(object sender, IMidiEventArgs e)
        {
            if (midiInput != null && e.MidiEvent.Message != null)
            {
                midiInput.Invoke(this, new MidiInputEventArgs(e.MidiEvent.Message));
            }
        }

        // Given a list of auto-open device, create 3 lists of device names:
        // - Devices that are connected and automatically opened
        // - Devices that are connected and not automatically opened
        // - Devices that are disconnected and automatically opened
        // This is used by UIs for midi input management dialogs.
        public static void GetDeviceNames(StringCollection midiInAutoOpen, out List<string> connectedAutoOpenDevices, out List<string> connectedNotAutoOpenDevices, out List<string> disconnectedAutoOpenDevices)
        {
            // Init lists
            connectedAutoOpenDevices = new List<String>();
            disconnectedAutoOpenDevices = new List<String>();
            connectedNotAutoOpenDevices = new List<String>();

            // Get the names of the connected devices
            var connectedDeviceNames = new List<string>();
            foreach (var d in PortEnumerator.InputPorts)
            {
                connectedDeviceNames.Add(d.Name);
            }

            // Sort the devices we open automatically into the connected and disconnected ones
            foreach (var autoOpenDev in midiInAutoOpen)
            {
                if (connectedDeviceNames.Contains(autoOpenDev))
                {
                    connectedAutoOpenDevices.Add(autoOpenDev);
                }
                else
                {
                    disconnectedAutoOpenDevices.Add(autoOpenDev);
                }
            }

            // Create the list of connected devices that are not automatically opened
            foreach (var d in connectedDeviceNames)
            {
                if (!connectedAutoOpenDevices.Contains(d))
                {
                    connectedNotAutoOpenDevices.Add(d);
                }
            }
        }

        // Reset: close all open ports
        public void Reset()
        {
            foreach (var port in openedPorts)
            {
                Console.WriteLine("MidiInputManager: closing Midi In port : " + port.Name + " because of disposition");
                port.Close();
            }

            openedPorts.Clear();
        }

        // Dispose: close all opened ports
        public void Dispose()
        {
            Reset();
        }

        #endregion
    }
}
