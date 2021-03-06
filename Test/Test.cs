﻿//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MidiLibrary;
using MidiLibrary.CommonMessages;
using MidiLibrary.SysexMessages;
using MidiLibrary.Instruments;
using MidiLibrary.FileIO;
using MidiLibrary.WindowsMultiMedia;
using MidiLibrary.Sequencer;
using MidiLibrary.PortIO;

namespace Test
{
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Start MidiLibrary tests");
            Test t = new Test();
            t.TestParser();
            t.TestInstrumentParser();
            t.TestMidiOut();
            t.TestMidiInOut();
            // Only if you have a kbd t.TestmidiIn();
            t.TestSequencer();
            Console.WriteLine("=== All tests done");
        }

        private MidiSequence sequence;
        private IMidiOutputPort output;
        private object lockObject = new object();

        private void TestParser()
        {
            Console.WriteLine("=== Test file parsing");
            string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + System.IO.Path.DirectorySeparatorChar;
            string origPath = basePath + "Inv8.mid";
            string destPath = basePath + "Inv8_identical.mid";
            sequence = MidiFileParser.ParseMidiFile(origPath);
            Console.WriteLine("  Parsed midi file");
            MidiFileWriter.WriteMidiFile(sequence, destPath);
            Console.WriteLine("  Wrote midi file");
            sequence = MidiFileParser.ParseMidiFile(destPath);
            Console.WriteLine("  Re-Parsed midi file");
        }

        private void TestInstrumentParser()
        {
            Console.WriteLine("=== Test instrument file parsing");
            var instruments = InstrumentFileParser.GetKnownInstruments();
            foreach (var ins in instruments)
            {
                Console.WriteLine("Found instrument: " + ins.Name);
                foreach (var sg in ins.SequenceGroups)
                {
                    Console.WriteLine("  Found sequence group: " + sg.Name);
                    foreach (var s in sg.Sequences)
                    {
                        Console.WriteLine("    Found sequence group: " + s.Name);
                        var msgs = s.MidiMessages;
                    }
                }
            }
        }

        private void TestMidiOut()
        {
            Console.WriteLine("=== Test output ports");
            IMidiOutputPort[] ports = PortEnumerator.OutputPorts;
            Console.WriteLine("  Found {0} output midi port(s)", ports.Length);
            foreach (var port in ports)
            {
                if (port is WindowsMidiOutputPort windowsPort)
                {
                    Console.WriteLine("     {0}: Port {1}, tech {2}, mId/pId {3}/{4}", windowsPort.Id, windowsPort.Name, windowsPort.Technology, windowsPort.ManufacturerId, windowsPort.ProductId);
                    if (windowsPort.Technology == WindowsMidiOutputPort.ETechnology.MidiMapper)
                    {
                        output = windowsPort;
                    }
                }
            }

            if ((output == null) && (ports.Length > 0))
            {
                output = ports[0];
            }
        }

        private bool loopGotShortMessage = false;
        private bool loopGotLongMessage = false;

        private void TestMidiInOut()
        {
            Console.WriteLine("=== Test midi in-out via LoopBe");

            var inPorts = PortEnumerator.InputPorts;

            var loopIn = inPorts.FirstOrDefault(p => p.Name.StartsWith("LoopBe"));
            if (loopIn == null)
            {
                Console.WriteLine("  No LoopBe input port found - skipping test");
                return;
            }
            Console.WriteLine("  Found LoopBe  input midi port: " + loopIn.Name);

            var outPorts = PortEnumerator.OutputPorts;
            var loopOut = outPorts.First(p => p.Name.StartsWith("LoopBe"));
            if (loopOut == null)
            {
                Console.WriteLine("  No LoopBe output port found - skipping test");
                return;
            }
            Console.WriteLine("  Found LoopBe output midi port: " + loopOut.Name);

            // Open the ports
            loopGotShortMessage = false;
            loopGotLongMessage = false;

            loopIn.MidiInputReceived += LoopInCallback;

            if (loopIn.Open() != null)
            {
                throw new InvalidOperationException("Cannot open loop input");
            }

            if (loopIn.Start() != null)
            {
                throw new InvalidOperationException("Cannot start loop input");
            }

            if (loopOut.Open() != null)
            {
                throw new InvalidOperationException("Cannot open loop output");
            }

            // Send a short message
            var message = new MidiNoteMessage(2, EMidiCommand.NoteOn, 63, 100);
            if (loopOut.Send(message) != null)
            {
                throw new InvalidOperationException("Cannot send short message to loop output");
            }

            // Wait until the input port gets it
            while (!loopGotShortMessage)
            {
                Thread.Sleep(10);
            }

            // Send a long message (sysex)
            var sysex = new MidiSysexMessage(new byte[] { 0xF0, 0x55, 0x15, 0x02, 0x19, 0x64, 0xF7 });

            if (loopOut.Send(sysex) != null)
            {
                throw new InvalidOperationException("Cannot send short message to loop output");
            }

            // Wait until the input port gets it
            while (!loopGotLongMessage)
            {
                Thread.Sleep(10);
            }

            // Close the ports
            if (loopIn.Close() != null)
            {
                throw new InvalidOperationException("Cannot close loop input");
            }

            if (loopOut.Close() != null)
            {
                throw new InvalidOperationException("Cannot close loop output");
            }

            Console.WriteLine("  Loop test completed successfully.");
        }

        private void LoopInCallback(object sender, IMidiEventArgs e)
        {
            var noteOn = e.MidiEvent.Message as MidiNoteMessage;

            if (noteOn != null && noteOn.Channel == 2 && noteOn.Command == EMidiCommand.NoteOn && noteOn.Note == 63 && noteOn.Velocity == 100)
            {
                loopGotShortMessage = true;
                return;
            }

            var sysex = e.MidiEvent.Message as MidiSysexMessage;
            var comp = new byte[] { 0xF0, 0x55, 0x15, 0x02, 0x19, 0x64, 0xF7 };

            if (sysex != null && sysex.Body.Length == comp.Length)
            {
                for (int i = 0; i < sysex.Body.Length; i++)
                {
                    if (comp[i] != sysex.Body[i])
                    {
                        throw new InvalidOperationException("Loop test: Unexpexted sysex data at offset " + i);
                    }
                }

                loopGotLongMessage = true;
                return;
            }

            throw new InvalidOperationException("Loop test: Got unknown message on loop input: " + e.MidiEvent.Message);
        }

        private void TestmidiIn()
        {
            Console.WriteLine("=== Test Midi In");

            var inPorts = PortEnumerator.InputPorts;
            if (inPorts.Length == 0)
            {
                Console.WriteLine("  No input port found - skipping test");
                return;
            }

            var inPort = inPorts[0];
            inPort.MidiInputReceived += MidiInTest_MidiInputReceived;
            inPort.Open();
            inPort.Start();

            Console.WriteLine("  Please key 3 notes on " + inPort.Name);

            while (numNoteOn < 1 && numNoteOff < 1)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("  That's one");

            while (numNoteOn < 2 && numNoteOff < 2)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("  That's two");

            while (numNoteOn < 3 && numNoteOff < 3)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("  And three!");

            inPort.Stop();
            inPort.Close();
        }

        private int numNoteOn;
        private int numNoteOff;

        void MidiInTest_MidiInputReceived(object sender, IMidiEventArgs e)
        {
            if (e.MidiEvent.Message is MidiNoteMessage mnm)
            {
                if (mnm.Action == EMidiNoteAction.On)
                {
                    numNoteOn += 1;
                }
                else if (mnm.Action == EMidiNoteAction.Off)
                {
                    numNoteOff += 1;
                }
            }
        }


        private void TestSequencer()
        {
            Console.WriteLine("=== Test sequencer");

            // Open output port
            if (output == null)
            {
                Console.WriteLine("  No output midi port fond - skipping");
                return;
            }
            output.Open();

            // Create sequencer
            MidiSequencer seq = new MidiSequencer(sequence);
            seq.OnSequencerEventPlayed += seq_OnSequencerEventPlayed;
            seq.StartPlayerThread();

            // Start playing
            Console.WriteLine("   Start playing...");
            seq.Play();

            // Wait for the end
            lock (lockObject)
            {
                Monitor.Wait(lockObject);
            }

            Console.WriteLine("   Done playing.");

            seq.Dispose();
        }

        void seq_OnSequencerEventPlayed(object sender, SequencerEventArg e)
        {
            switch (e.Type)
            {
                case SequencerEventArg.EType.Play:
                    output.Send((MidiMessage)e.Event.SequencerMessage);
                    break;
                case SequencerEventArg.EType.Reset:
                    output.Reset();
                    break;
                case SequencerEventArg.EType.End:
                    lock (lockObject)
                    {
                        Monitor.Pulse(lockObject);
                    }
                    break;
            }
        }
    }
}
