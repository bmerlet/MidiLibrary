//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.IO;
using System.Text;

namespace MidiLibrary.FileIO
{
    /// <summary>
    /// Adapter to read a file at midi file format 1.0 or 2.0 (.MID)
    /// </summary>
    internal class MidiFileFormatReader : IDisposable
    {
        #region Private members

        private string filename;
        private BinaryReader br;
        private uint bytesRead;

        #endregion

        #region Constructor

        public MidiFileFormatReader(string filename)
        {
            // Open the file for reading
            this.filename = filename;
            this.br = new BinaryReader(File.OpenRead(filename));
            bytesRead = 0;
        }

        #endregion

        #region Properties

        // File name
        public string Filename
        {
            get { return filename; }
        }

        // Number of bytes read
        public uint BytesRead
        {
            get { return bytesRead; }
        }

        // Reset byte read count
        public void ResetBytesRead()
        {
            bytesRead = 0;
        }

        #endregion

        #region Read entities from file

        // Read a string of <length> characters
        public string ReadString(uint length)
        {
            bytesRead += length;
            byte[] byteStr = br.ReadBytes((int)length);
            return Encoding.UTF8.GetString(byteStr);
        }

        // Read an int on 32 bits
        public uint ReadUInt32()
        {
            return (uint)((ReadUInt8() << 24) + (ReadUInt8() << 16) + (ReadUInt8() << 8) + ReadUInt8());
        }

        // Read an int on 24 bits
        public uint ReadUInt24()
        {
            return (uint)((ReadUInt8() << 16) + (ReadUInt8() << 8) + ReadUInt8());
        }

        // Read an int on 16 bits
        public ushort ReadUInt16()
        {
            return (ushort)((ReadUInt8() << 8) + ReadUInt8());
        }

        // Read a byte
        public byte ReadUInt8()
        {
            bytesRead += 1;
            return br.ReadByte();
        }

        // Read an int encoded in a variable number of bytes
        public uint ReadVarLengthQuantity()
        {
            uint result = 0;
            byte b;

            // Keep reading bytes and aggregating result as long the high bit is set
            do
            {
                b = ReadUInt8();
                result = (result << 7) + (uint)(b & 0x7f);
            } while ((b & 0x80) != 0);

            return result;
        }

        #endregion

        #region Destructor

        public void Dispose()
        {
            if (br != null)
            {
                br.Dispose();
                br = null;
            }
        }

        #endregion

    }
}
