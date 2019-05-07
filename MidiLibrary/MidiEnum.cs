//
// Copyright 2016 Benoit J. Merlet
//


namespace MidiLibrary
{
    /// <summary>
    /// Midi commands
    /// </summary>
    public enum EMidiCommand
    {
        // Channel messages
        NoteOff = 0x80,
        NoteOn = 0x90,
        KeyAfterTouch = 0xA0,
        ControlChange = 0xB0,
        PatchChange = 0xC0,
        ChannelAfterTouch = 0xD0,
        PitchWheelChange = 0xE0,

        // System messages - Exclusive
        SystemExclusive = 0xF0,
        EndOfSystemExclusive = 0xF7,

        // System messages - Common
        SongPosition = 0xF2,
        SongSelect = 0xF3,
        TuneRequest = 0xF6,

        // System messages - Real time
        TimingClock = 0xF8,
        StartSequence = 0xFA,
        ContinueSequence = 0xFB,
        StopSequence = 0xFC,
        ActiveSensing = 0xFE,
        MetaEvent = 0xFF, // Is system reset on a midi bus, is a meta event in midi files

        SystemMessageMask = 0xF0,
    };

    // Possible actions on a note
    public enum EMidiNoteAction
    {
        On,
        VelChange,
        Off,
    };

    
    /// <summary>
    /// Midi controllers
    /// </summary>
    public enum EMidiController : uint
    {
        // Continuous controllers (0-63)
        BankSelectMSB = 0,
        Modulation = 1,
        Breath = 2,
        Foot = 4,
        PortamentoTime = 5,
        DataEntryMSB = 6,
        MainVolume = 7,
        Balance = 8,
        Pan = 10,
        Expression = 11,
        EffectControl1 = 12,
        EffectControl2 = 13,
        BankSelectLSB = 32,
        DataEntryLSB = 38,

        // Switches (64-96)
        Sustain = 64,
        Portamento = 65,
        Sostenuto = 66,
        SoftPedal = 67,
        LegatoFootSwitch = 68,

        // Not clearly defined
        Reverb = 91,
        Tremolo = 92,
        Chorus = 93,
        Detune = 94,
        Phaser = 95,
        DataIncrement = 96,
        DataDecrement = 97,

        // Registered and Non-registered Parameter numbers
        // Note on usage:
        // To change a RPN, send RpnMSB and RpnLSB, then DataEntryMSB (and optionally DataEntryLSB) 
        // To change a NRPN, send NrpnMSB and NrpnLSB, then DataEntryMSB (and optionally DataEntryLSB) 
        NrpnLSB = 98,
        NrpnMSB = 99,
        RpnLSB = 100,
        RpnMSB = 101,

        AllSoundOff = 120,
        ResetAllControllers = 121,

        // Channel mode (122-127)
        LocalControl = 122,
        AllNotesOff = 123,
        OmniModeOff = 124, // On: Omni mode off, Off: All notes off 
        OmniModeOn = 125, // On: Omni mode on, Off: All notes off
        MonoModeOn = 126,  // parameter = number of channels. Zero means as many as # voices
        PolyModeOn = 127,  // On: Poly mode on, Off: All notes off
    }

    /// <summary>
    /// Midi meta-events
    /// </summary>
    public enum EMetaEventType
    {
        /// <summary>Track sequence number</summary>
        TrackSequenceNumber = 0x00,

        /// <summary>Text event</summary>
        TextEvent = 0x01,

        /// <summary>Copyright</summary>
        Copyright = 0x02,

        /// <summary>Sequence track name</summary>
        SequenceTrackName = 0x03,

        /// <summary>Track instrument name</summary>
        TrackInstrumentName = 0x04,

        /// <summary>Lyric</summary>
        Lyric = 0x05,

        /// <summary>Marker</summary>
        Marker = 0x06,

        /// <summary>Cue point</summary>
        CuePoint = 0x07,

        /// <summary>Program (patch) name</summary>
        ProgramName = 0x08,

        /// <summary>Device (port) name</summary>
        DeviceName = 0x09,

        /// <summary>MIDI Channel (not official?)</summary>
        MidiChannel = 0x20,
        
        /// <summary>MIDI Port (not official?)</summary>
        MidiPort = 0x21,
        
        /// <summary>End track</summary>
        EndTrack = 0x2F,
        
        /// <summary>Set tempo</summary>
        SetTempo = 0x51,
        
        /// <summary>SMPTE offset</summary>
        SmpteOffset = 0x54,
        
        /// <summary>Time signature</summary>
        TimeSignature = 0x58,
        
        /// <summary>Key signature</summary>
        KeySignature = 0x59,
        
        /// <summary>Sequencer specific</summary>
        SequencerSpecific = 0x7F
    }

    /// <summary>
    /// Patch selection method for an instrument
    /// </summary>
    public enum EPatchSelectionMethod
    {
        Normal = 0,     // Use bank select MSB and LSB
        OnlyMSB = 1,    // Use bank select MSB only
        OnlyLSB = 2,    // Use bank select MSB only
        Patch = 3       // Use only patch change
    };

    /// <summary>
    /// How an instrument handles overlap of the same note on the same channel
    /// </summary>
    public enum EOverlapSupport
    {
        None = 0,                   // Not supported
        OneNoteOffPerNoteOn = 1,    // Supported
    };

}
