//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiLibrary.CommonMessages;

namespace MidiLibrary.Instruments
{
    /// <summary>
    /// Immutable class representing a specific patch in a bank of an instrument
    /// </summary>
    public class Patch
    {
        #region Private members

        // Bank this patch belongs to
        private Bank bank;

        // Name of the patch
        private string name;

        // Patch number
        private uint number;

        // Note names (if any)
        private List<NoteName> noteNames;

        #endregion

        #region Constructor

        public Patch(Bank bank, string name, uint number)
        {
            this.bank = bank;
            this.name = name;
            this.number = number;
            this.noteNames = new List<NoteName>();
        }

        #endregion

        #region Properties

        public Bank Bank
        {
            get { return bank; }
        }

        public string Name
        {
            get { return name; }
        }

        public uint Number
        {
            get { return number; }
        }

        public List<NoteName> NoteNames
        {
            get { return noteNames; }
        }

        #endregion

        #region Services

        public MidiMessage[] GetPatchSelectSequence(uint channel)
        {
            var messages = new List<MidiMessage>();

            switch(bank.Instrument.PatchSelectionMethod)
            {
                case EPatchSelectionMethod.Normal:
                    messages.Add(new MidiControlChangeMessage(channel, EMidiController.BankSelectMSB, bank.Number / 128));
                    messages.Add(new MidiControlChangeMessage(channel, EMidiController.BankSelectLSB, bank.Number % 128));
                    break;
                case EPatchSelectionMethod.OnlyLSB:
                    messages.Add(new MidiControlChangeMessage(channel, EMidiController.BankSelectLSB, bank.Number % 128));
                    break;
                case EPatchSelectionMethod.OnlyMSB:
                    messages.Add(new MidiControlChangeMessage(channel, EMidiController.BankSelectMSB, bank.Number / 128));
                    break;
                default:
                    break;
            }

            messages.Add(new MidiPatchChangeMessage(channel, number));

            return messages.ToArray();
        }

        public override string ToString()
        {
            return "Patch " + number + ": " + name;
        }

        #endregion
    }
}
