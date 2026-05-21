using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeLibrary
{
    public class SAHT
    {
        public SAHT() { }

        public SAHT(string filePath) {
            Read(new BinaryDataReader(File.OpenRead(filePath)));
        }

        public SAHT(byte[] data) {
            Read(new BinaryDataReader(new MemoryStream(data)));
        }

        public Dictionary<uint, string> HashEntries = new Dictionary<uint, string>();

        private void Read(BinaryDataReader reader)
        {
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "SAHT")
                throw new Exception("Wrong magic");
            uint FileSize = reader.ReadUInt32();
            uint Offset = reader.ReadUInt32();
            if (Offset !=  0x10) //EFE uses big endian. WT uses little.
            {
                Offset = 0x10;
                reader.ByteOrder = ByteOrder.BigEndian;
            }


            uint EntryCount = reader.ReadUInt32();

            Console.WriteLine($"FileSize {FileSize} Offset {Offset} EntryCount {EntryCount}");

            reader.Seek(Offset, SeekOrigin.Begin);
            for (int i = 0; i < EntryCount; i++)
            {
                HashEntry entry = new HashEntry();
                entry.Read(reader);
                reader.Align(16);
                if (EntryCount == 4)
                Console.WriteLine($"{entry.Name} {entry.Hash} {EntryCount}");

                HashEntries.Add(entry.Hash, entry.Name);
            }
        }

        public class HashEntry
        {
            public uint Hash { get; set; }
            public string Name { get; set; }

            public void Read(BinaryDataReader reader)
            {
                Hash = reader.ReadUInt32();
                Name = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
        }
    }
}
