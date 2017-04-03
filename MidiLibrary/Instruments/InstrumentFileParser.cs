//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.Instruments
{
    /// <summary>
    /// Parses an instrument definition file in the cakewalk ".INS" format.
    /// </summary>
    static public class InstrumentFileParser
    {
        #region Entry points

        // Parse an instrument file from a stream
        public static Instrument[] Parse(Stream stream)
        {
            return Parse(new StreamReader(stream));
        }

        // Parse an instrument file from a reader
        public static Instrument[] Parse(TextReader reader)
        {
            var items = ParseTextFile(reader);

            var instruments = ConvertItemsToInstruments(items);

            return instruments.ToArray();
        }

        // Parse all known instruments (embedded and in $USER/MidiInstruments)
        public static Instrument[] GetKnownInstruments()
        {
            string parsingErrors;
            return GetKnownInstruments(out parsingErrors);
        }

        // Parse all known instruments (embedded and in $USER/MidiInstruments), return any parsing error in a string
        public static Instrument[] GetKnownInstruments(out string parsingErrors)
        {
            const string MidInstrumentDirectory = "MidiInstruments";

            var result = new List<Instrument>();
            parsingErrors = null;

            // Generic GM instrument definition
            result.AddRange(ParseEmbeddedResourceInstrumentFile("GM"));

            // Motif-Rack ES
            result.AddRange(ParseEmbeddedResourceInstrumentFile("MotifRackES"));

            // Read all the instruments in $USER/MidiInstruments
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                Path.DirectorySeparatorChar + MidInstrumentDirectory;

            if (Directory.Exists(path))
            {
                foreach(var file in Directory.EnumerateFiles(path, "*.ins"))
                {
                     try
                     {
                         StreamReader reader = new StreamReader(file);
                         using (reader)
                         {
                             result.AddRange(Parse(reader));
                             reader.Close();
                         }
                     }
                     catch (Exception e)
                     {
                         if (parsingErrors == null)
                         {
                             parsingErrors = "";
                         }
                         else
                         {
                             parsingErrors += Environment.NewLine;
                         }
                         parsingErrors += "Error parsing instrument file: " + file + ": " + e.Message;
                     }
                }
            }

            return result.ToArray();
        }

        // Utility to get a stream for an embedded instrument file 
        public static Instrument[] ParseEmbeddedResourceInstrumentFile(string name)
        {
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = myAssembly.GetName().Name + ".Resources." + name + ".ins";
            var stream = myAssembly.GetManifestResourceStream(resourceName);

            return Parse(stream);
        }

        #endregion

        #region Parser

        private enum Section { None, Patches, Notes, Controllers, RPNs, NRPNs, Sequences, Instrument }

        // This function reads the whole file into a list of "ParsedItem" objects
        private static List<ParsedItem>  ParseTextFile(TextReader reader)
        {
            var items = new List<ParsedItem>();

            using (reader)
            {
            Section section = Section.None;
                string str;

                while ((str = reader.ReadLine()) != null)
                {
                    str = str.Trim();

                    // Skip blank lines
                    if (str.Length == 0)
                    {
                        continue;
                    }

                    // Skip comments
                    if (str[0] == ';')
                    {
                        continue;
                    }

                    // Section definition
                    if (str[0] == '.')
                    {
                        section = ParseSection(str.Substring(1));
                        continue;
                    }

                    // Item name
                    if (str[0] == '[')
                    {
                        string itemName = str.Substring(1, str.Length - 2);
                        var item = new ParsedItem(section, itemName);
                        items.Add(item);
                        continue;
                    }

                    // The rest is in the form <keyword or number>=<string>
                    if (items.Count == 0)
                    {
                        throw new InvalidDataException("Entry outside of section/item in .INS file: " + str);
                    }

                    var split = str.Split('=');
                    if (split.Length != 2)
                    {
                        throw new InvalidDataException("Malformed entry in .INS file: " + str);
                    }

                    items[items.Count - 1].AddValue(split[0], split[1]);
                }
            }

            return items;
        }

        private static Section ParseSection(string str)
        {
            switch (str)
            {
                case "Patch Names": return Section.Patches;
                case "Note Names": return Section.Notes;
                case "Controller Names": return Section.Controllers;
                case "RPN Names": return Section.RPNs;
                case "NRPN Names": return Section.NRPNs;
                case "Sequences": return Section.Sequences;
                case "Instrument Definitions": return Section.Instrument;
            }

            throw new InvalidDataException("Unknown section in INS file: " + str);
        }

        // Traverses the list of parsed items to build the instruments
        private static List<Instrument> ConvertItemsToInstruments(List<ParsedItem> items)
        {
            var instruments = new List<Instrument>();

            // Process all instruments
            ParsedItem instrumentItem;
            while ((instrumentItem = ExtractParsedItemBySection(items, Section.Instrument)) != null)
            {
                var drumBanks = new List<int>();

                // First pass over the attributes to retreive the bank select method and which banks are drum
                var patchSelMethod = EPatchSelectionMethod.Normal;
                var overlapSupport = EOverlapSupport.None;
                bool supportsPitchBend = false;
                bool supportsAfterTouch = false;

                foreach (var kvp in instrumentItem.Values)
                {
                    if (kvp.Key == "BankSelMethod")
                    {
                        patchSelMethod = (EPatchSelectionMethod)int.Parse(kvp.Value);
                    }
                    else if (kvp.Key == "OverlapSupport")
                    {
                        overlapSupport = (EOverlapSupport)int.Parse(kvp.Value);
                    }
                    else if (kvp.Key == "SupportsPitchBend")
                    {
                        supportsPitchBend = int.Parse(kvp.Value) != 0;
                    }
                    else if (kvp.Key == "SupportsAfterTouch")
                    {
                        supportsPitchBend = int.Parse(kvp.Value) != 0;
                    }
                    else if (kvp.Key.StartsWith("Drum[") && kvp.Value == "1")
                    {
                        // Note: we only support whole banks defined as drum, not individual patches within a bank
                        var drumargs = kvp.Key.Substring(5);
                        var split = drumargs.Split(new char[] { ',', ']' });
                        int bank = (split[0] == "*") ? -1 : int.Parse(split[0]);
                        // ignore split[1], which is the patch number. Assume it is '*'
                        drumBanks.Add(bank);
                    }
                }

                // Create the instrument
                var instrument = new Instrument(instrumentItem.Name, patchSelMethod, overlapSupport, supportsAfterTouch, supportsPitchBend);

                // Populate it based on the name/value entries
                foreach (var kv in instrumentItem.Values)
                {
                    string kw = kv.Key;
                    string val = kv.Value;

                    if (kw.StartsWith("Drum["))
                    {
                        // Already processed above
                        continue;
                    }

                    if (kw.StartsWith("Patch["))
                    {
                        var numStr = kw.Substring(6);
                        numStr = numStr.Split(']')[0];

                        if (numStr == "*")
                        {
                            // Patch[*] defines the default bank for unknown bank numbers.
                            // It can be either a bank name or a special string like 0...127
                            // We ignore it.
                        }
                        else
                        {
                            uint bankNumber = uint.Parse(numStr);
                            string bankName = val;

                            // Find if this is a drum bank
                            bool drumBank = drumBanks.Contains(-1) || drumBanks.Contains((int)bankNumber);

                            // Create the bank
                            var bank = new Bank(instrument, bankName, bankNumber, drumBank);

                            // Populate the patches
                            BuildBank(bank, bankName, items);

                            // Add it to the instrument
                            instrument.Banks.Add(bank);
                        }

                        continue;
                    }

                    if (kw.StartsWith("Sequence["))
                    {
                        var numStr = kw.Substring(9);
                        numStr = numStr.Split(']')[0];
                        uint seqGrpNum = uint.Parse(numStr);

                        var sequenceGroup = new SequenceGroup(instrument, val, seqGrpNum);
                        instrument.SequenceGroups.Add(sequenceGroup);

                        BuildSequences(sequenceGroup, val, items);

                        continue;
                    }

                    switch (kw)
                    {
                        case "Control":
                            BuildControllers(instrument, val, items);
                            break;
                        case "RPN":
                            BuildProgramNumbers(instrument, val, items, true);
                            break;
                        case "NRPN":
                            BuildProgramNumbers(instrument, val, items, false);
                            break;
                        case "Patch":
                            BuildControllers(instrument, val, items);
                            break;
                        case "BankSelMethod":
                        case "OverlapSupport":
                        case "SupportsPitchBend":
                        case "SupportsAfterTouch":
                            // Already processed above
                            break;
                        default:
                            throw new InvalidDataException("Unknown directive in instrument section: " + kw);
                    }
                }

                // Add it to the collection
                instruments.Add(instrument);
            }

            return instruments;
        }

        private static void BuildBank(Bank bank, string bankName, List<ParsedItem> items)
        {
            // Find the bank definition by name
            var bankItem = items.Find(pi => pi.Section == Section.Patches && pi.Name == bankName);
            if (bankItem == null)
            {
                throw new InvalidDataException("Reference to unknown bank name: " + bankName);
            }

            // See if we have a "BasedOn" directive
            foreach (var kvp in bankItem.Values)
            {
                if (kvp.Value == "BasedOn")
                {
                    // Recurse!
                    BuildBank(bank, kvp.Value, items);
                }
            }

            // Create each patch
            foreach (var kv in bankItem.Values)
            {
                if (kv.Value == "BasedOn")
                {
                    continue;
                }

                uint num = uint.Parse(kv.Key);
                string name = kv.Value;

                var patch = new Patch(bank, name, num);

                // Look to see if this patch has note names defined
                BuildNoteNames(patch, name, items);

                // In case a patch with this number was already added by a BasedOn directive
                bank.Patches.RemoveAll(p => p.Number == num);

                bank.Patches.Add(patch);
            }
        }

        private static void BuildNoteNames(Patch patch, string patchName, List<ParsedItem> items)
        {
            var noteNamesItem = items.Find(pi => pi.Section == Section.Notes && pi.Name == patchName);

            if (noteNamesItem != null)
            {
                // See if we have a "BasedOn" directive
                foreach (var kvp in noteNamesItem.Values)
                {
                    if (kvp.Value == "BasedOn")
                    {
                        // Recurse!
                        BuildNoteNames(patch, kvp.Value, items);
                    }
                }

                foreach (var kv in noteNamesItem.Values)
                {
                    uint num = uint.Parse(kv.Key);
                    string name = kv.Value;

                    var noteName = new NoteName(patch, name, num);

                    // In case a note name with this number was already added by a BasedOn directive
                    patch.NoteNames.RemoveAll(p => p.Number == num);

                    patch.NoteNames.Add(noteName);
                }
            }
        }

        private static void BuildSequences(SequenceGroup sequenceGroup, string sequenceGroupName, List<ParsedItem> items)
        {
            var sequenceItem = items.Find(pi => pi.Section == Section.Sequences && pi.Name == sequenceGroupName);
            if (sequenceItem == null)
            {
                throw new InvalidDataException("Reference to unknown sequence group name " + sequenceGroupName);
            }

            foreach (var kv in sequenceItem.Values)
            {
                string name = kv.Key;
                string bytes = kv.Value;

                var sequence = Sequence.BuildSequence(sequenceGroup, name, bytes);
                sequenceGroup.Sequences.Add(sequence);
            }
        }

        private static void BuildControllers(Instrument instrument, string controllerSetName, List<ParsedItem> items)
        {
            var controllerItem = items.Find(pi => pi.Section == Section.Controllers && pi.Name == controllerSetName);
            if (controllerItem == null)
            {
                throw new InvalidDataException("Reference to unknown controller set name " + controllerSetName + " in instrument " + instrument.Name);
            }

            foreach (var kv in controllerItem.Values)
            {
                uint num = uint.Parse(kv.Key);
                string name = kv.Value;

                var controller = new Controller(instrument, name, num);
                instrument.Controllers.Add(controller);
            }
        }


        private static void BuildProgramNumbers(Instrument instrument, string programNumberSetName, List<ParsedItem> items, bool registered)
        {
            var section = registered ? Section.RPNs : Section.NRPNs;
            var programNumberItem = items.Find(pi => pi.Section == section && pi.Name == programNumberSetName);
            if (programNumberItem == null)
            {
                throw new InvalidDataException("Reference to unknown RPN or NRPN name " + programNumberSetName + " in instrument " + instrument.Name);
            }

            foreach (var kv in programNumberItem.Values)
            {
                uint num = uint.Parse(kv.Key);
                string name = kv.Value;

                var pn = new ProgramNumber(instrument, registered, name, num);
                if (registered)
                {
                    instrument.RPNs.Add(pn);
                }
                else
                {
                    instrument.NRPNs.Add(pn);
                }
            }
        }

        private static ParsedItem ExtractParsedItemBySection(List<ParsedItem> items, Section section)
        {
            ParsedItem result = null;

            foreach (var pi in items)
            {
                if (pi.Section == section)
                {
                    items.Remove(pi);
                    result = pi;
                    break;
                }
            }

            return result;
        }

        private class ParsedItem
        {
            private Section section;
            private string name;
            private List<KeyValuePair<string, string>> values;

            public ParsedItem(Section section, string name)
            {
                this.section = section;
                this.name = name;
                this.values = new List<KeyValuePair<string, string>>();
            }

            public Section Section
            {
                get { return section; }
            }

            public string Name
            {
                get { return name; }
            }

            public List<KeyValuePair<string, string>> Values
            {
                get { return values; }
            }

            public void AddValue(string key, string val)
            {
                values.Add(new KeyValuePair<string, string>(key, val));
            }
        }
        #endregion
    }
}
