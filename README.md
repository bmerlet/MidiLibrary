# MidiLibrary

The MidiLibrary project is a C# library (.DLL) to support projects using of MIDI on windows. It provides:

- Types for all midi messages, as well as tracks and sequences
- Reading and writing of midi files
- Interface to the windows multimedia MIDI subsystem, including support for Sysex messages
- Basic event-based MIDI sequencer

In addition, this library provides a parser of instrument definition files (.INS) in the SONAR
format. The instrument definition file provides synthesizer-specific definitions, such as patch
names and available controllers. MidiLibrary parses this information and makes it available to
your application.

This project is built using Microsoft VisualStudio 2013.

Feel free to use, complain, submit pull requests, etc...

