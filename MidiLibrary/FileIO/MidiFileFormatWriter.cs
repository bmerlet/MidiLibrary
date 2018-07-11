//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.FileIO
{
    /// <summary>
    /// Adapter to write a file in midi file format 1.0 or 2.0 (.MID)
    /// </summary>
    internal class MidiFileFormatWriter : IDisposable
    {
        #region Private members

        private string filename;
        private BinaryWriter bw;
        private uint bytesWritten;
        private bool countingMode;

        #endregion

        #region Constructor

        public MidiFileFormatWriter(string filename)
        {
            // Open the file for writing
            this.filename = filename;
            this.bw = new BinaryWriter(File.OpenWrite(filename));
            this.bytesWritten = 0;
            this.countingMode = false;
        }

        #endregion

        #region Properties

        // File name
        public string Filename
        {
            get { return filename; }
        }

        // Number of bytes written
        public uint BytesWritten
        {
            get { return bytesWritten; }
        }

        // Reset byte read count
        public void ResetBytesWritten()
        {
            bytesWritten = 0;
        }

        // If we are just counting or actually writing
        public bool CountingMode
        {
            get { return countingMode; }
            set { countingMode = value; }
        }

        #endregion

        #region Write entities to file

        // Write a string of <length> characters
        public void WriteString(string str, bool writeStringLengthAsVariableLengthQuantity)
        {
            // Translate string to byte array
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            // Write out string length
            if (writeStringLengthAsVariableLengthQuantity)
            {
                WriteVarLengthQuantity((uint)bytes.Length);
            }

            // Write string
            if (countingMode)
            {
                bytesWritten += (uint)bytes.Length;
            }
            else
            {
                bw.Write(bytes);
            }
        }

        // Write an int on 32 bits
        public void WriteUInt32(uint val)
        {
            WriteUInt8((byte)(val >> 24));
            WriteUInt8((byte)(val >> 16));
            WriteUInt8((byte)(val >> 8));
            WriteUInt8((byte)val);
        }

        // Write an int on 24 bits
        public void WriteUInt24(uint val)
        {
            WriteUInt8((byte)(val >> 16));
            WriteUInt8((byte)(val >> 8));
            WriteUInt8((byte)val);
        }

        // Write an int on 16 bits
        public void WriteUInt16(ushort val)
        {
            WriteUInt8((byte)(val >> 8));
            WriteUInt8((byte)val);
        }

        // Write a byte
        public void WriteUInt8(byte b)
        {
            if (countingMode)
            {
                bytesWritten += 1;
            }
            else
            {
                bw.Write(b);
            }
        }

        // Write an int encoded in a variable number of bytes
        public void WriteVarLengthQuantity(uint val)
        {
            byte[] output = new byte[5];
            int startIx = output.Length;

            // Spread the value into 7-bit quantities
            do
            {
                // Put the 7 LSB out to the byte array
                output[--startIx] = (byte)(val & 0x7f);

                // Remove 7 LSB
                val >>= 7;

            } while(val != 0);

            // Set LSB on all bytes but the last one, and write them out
            for (int i = startIx; i < output.Length; i++)
            {
                if (i != (output.Length - 1))
                {
                    output[i] |= 0x80;
                }
                WriteUInt8(output[i]);
            }
        }

        #endregion

        #region Destructor

        public void Dispose()
        {
            if (bw != null)
            {
                bw.Dispose();
                bw = null;
            }
        }

        #endregion

    }
}
