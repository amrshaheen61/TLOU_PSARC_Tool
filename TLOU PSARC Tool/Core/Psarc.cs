using Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TLOU_PSARC_Tool.Core
{
    //reference: http://aluigi.altervista.org/bms/brink.bms
    public class Psarc : IDisposable
    {

        const int HeaderSize = 32;
        public class Header
        {
            public string idstring;
            public short U1;
            public short U2;
            public string CompressionFormat;
            public int headerSize;
            public int entrySize;
            public int entryCount;
            public int DataBlockSize;
            public int Flags;

            public void Read(IStream stream)
            {
                idstring = stream.GetStringValue(4);
                U1 = stream.GetShortValue();
                U2 = stream.GetShortValue();
                CompressionFormat = stream.GetStringValue(4);
                headerSize = stream.GetIntValue();
                entrySize = stream.GetIntValue();
                entryCount = stream.GetIntValue();
                DataBlockSize = stream.GetIntValue();
                Flags = stream.GetIntValue();
                if (idstring != "PSAR")
                {
                    throw new Exception("Invalid 'PSAR' file");
                }
            }

            public void Write(IStream stream)
            {
                stream.SetStringValue(idstring);
                stream.SetShortValue(U1);
                stream.SetShortValue(U2);
                stream.SetStringValue(CompressionFormat);
                stream.SetIntValue(headerSize);
                stream.SetIntValue(entrySize);
                stream.SetIntValue(entryCount);
                stream.SetIntValue(DataBlockSize);
                stream.SetIntValue(Flags);
            }

        }




        public class Entry
        {
            public byte[] Md5Hash { get; set; }
            public string Name { get; set; }
            public int CompressedBlockSizeIndex { get; set; }
            public byte Temp1 { get; set; }
            public int UncompressedSize { get; set; }
            public byte Temp2 { get; set; }
            public long Offset { get; set; }

            public List<DecmpressBlock> DempressBlocks = new List<DecmpressBlock>();
            public class DecmpressBlock
            {
                public long Offset { get; set; }
                public int CompressSize { get; set; }
                public int DecompressSize { get; set; }
            }

            public void Read(Psarc psarc)
            {
                Md5Hash = psarc.Stream.GetBytes(16);
                Name = psarc.GetName(BitConverter.ToString(Md5Hash, 0).Replace("-", ""));
                CompressedBlockSizeIndex = psarc.Stream.GetIntValue();
                Temp1 = psarc.Stream.GetByteValue();
                UncompressedSize = psarc.Stream.GetIntValue();
                Temp2 = psarc.Stream.GetByteValue();
                Offset = psarc.Stream.GetIntValue();
                CalcFileSize(psarc);
            }

            public void Write(IStream stream)
            {
                stream.SetBytes(Md5Hash);
                stream.SetIntValue(CompressedBlockSizeIndex);
                stream.SetByteValue(Temp1);
                stream.SetIntValue(UncompressedSize);
                stream.SetByteValue(Temp2);
                stream.SetIntValue((int)Offset);
            }

            //private int GetFileUSize()
            //{
            //    int usize = 0;
            //    foreach (var block in DempressBlocks) usize += block.DecompressSize;
            //    return usize;
            //}

            public void CalcFileSize(Psarc psarc)
            {
                int uncompressedSize = UncompressedSize;
                int compressedBlockSizeIndex = CompressedBlockSizeIndex;
                long offset = Offset;

                if (uncompressedSize == psarc.ArraySize[compressedBlockSizeIndex] || uncompressedSize < psarc.header.DataBlockSize)
                {
                    DempressBlocks.Add(new DecmpressBlock()
                    {
                        Offset = offset,
                        DecompressSize = uncompressedSize,
                        CompressSize = psarc.ArraySize[compressedBlockSizeIndex]
                    });
                    return;
                }

                int bufferCount = (uncompressedSize / psarc.header.DataBlockSize) + 1;

                for (int i = 0; i < bufferCount; i++)
                {
                    int size = psarc.ArraySize[compressedBlockSizeIndex] == 0 ? psarc.header.DataBlockSize : psarc.ArraySize[compressedBlockSizeIndex];


                    if ((size == 0 || size == psarc.header.DataBlockSize) && i == 0)
                    {
                        size = 0;

                        for (int n = 0; n < bufferCount; n++)
                        {
                            int bufferSize = psarc.ArraySize[compressedBlockSizeIndex];

                            if (bufferSize == 0)
                            {
                                bufferSize = psarc.header.DataBlockSize;
                            }

                            size += bufferSize;
                            compressedBlockSizeIndex++;
                            i++;
                        }
                    }

                    if (uncompressedSize == size)
                    {
                        DempressBlocks.Add(new DecmpressBlock()
                        {
                            Offset = offset,
                            DecompressSize = uncompressedSize,
                            CompressSize = size
                        });
                    }
                    else if (uncompressedSize > psarc.header.DataBlockSize)
                    {
                        DempressBlocks.Add(new DecmpressBlock()
                        {
                            Offset = offset,
                            DecompressSize = psarc.header.DataBlockSize,
                            CompressSize = size
                        });
                        uncompressedSize -= psarc.header.DataBlockSize;
                    }
                    else
                    {
                        DempressBlocks.Add(new DecmpressBlock()
                        {
                            Offset = offset,
                            DecompressSize = uncompressedSize,
                            CompressSize = size
                        });

                    }

                    compressedBlockSizeIndex++;
                    offset += size;
                }
            }



            public void CalcBlocksSize(Psarc psarc)
            {
                DempressBlocks = new List<DecmpressBlock>();

                if (UncompressedSize < psarc.header.DataBlockSize)
                {
                    DempressBlocks.Add(new DecmpressBlock()
                    {
                        CompressSize = UncompressedSize,
                        DecompressSize = UncompressedSize
                    });
                    return;
                }
                int uncompressedSize = UncompressedSize;
                int bufferCount = (UncompressedSize / psarc.header.DataBlockSize) + 1;

                for (int i = 0; i < bufferCount; i++)
                {

                    if (uncompressedSize >= psarc.header.DataBlockSize)
                    {
                        DempressBlocks.Add(new DecmpressBlock()
                        {
                            CompressSize = psarc.header.DataBlockSize,
                            DecompressSize = psarc.header.DataBlockSize
                        });
                        uncompressedSize -= psarc.header.DataBlockSize;
                    }
                    else
                    {
                        DempressBlocks.Add(new DecmpressBlock()
                        {
                            CompressSize = uncompressedSize,
                            DecompressSize = uncompressedSize
                        });
                    }
                }
            }







        }

        Dictionary<string, string> HashNames = new Dictionary<string, string>();
        public Dictionary<string, Entry> Entries = new Dictionary<string, Entry>();
        public string FileName { get; set; }
        public IStream Stream { get; set; }
        public Header header { get; set; }
        public int[] ArraySize { get; set; }
        public long StartOffset { get; set; }

        public static Psarc Load(string path)
        {
            var psarc = new Psarc();
            psarc.Stream = FStream.Open(path, FileMode.Open, FileAccess.ReadWrite);
            psarc.FileName = path;
            psarc.Load();
            return psarc;
        }



        public void Load()
        {
            Stream.Endian = Endian.Big;
            //read header values
            header = new Header();
            header.Read(Stream);

            StartOffset = Stream.GetPosition();
            ArraySize = ReadSizeArray();


            //decode names table, should be the first entry
            Entry Namesentry = new Entry()
            {
                CompressedBlockSizeIndex = Stream.GetIntValue(false, 48),
                UncompressedSize = Stream.GetIntValue(false, 53),
                Offset = Stream.GetIntValue(false, 58)
            };
            Namesentry.CalcFileSize(this);

            MStream mstream = new MStream(GetFileRaw(Namesentry));
            while (!mstream.EndofFile())
            {
                var name = mstream.GetStringValueN();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }


                if (name.Contains("\n"))
                {
                    foreach (var str in name.Split(new[] { '\n', '\r' }))
                    {
                        var hash = BitConverter.ToString(Md5.Calc(str), 0).Replace("-", "");
                        if (!HashNames.ContainsKey(hash))
                        {
                            HashNames.Add(hash, str);
                        }
                    }
                    continue;
                }


                var md5hash = BitConverter.ToString(Md5.Calc(name), 0).Replace("-", "");
                if (!HashNames.ContainsKey(md5hash))
                {
                    HashNames.Add(md5hash, name);
                }
            }

            Stream.Seek(StartOffset);
            for (int i = 0; i < header.entryCount; i++)
            {
                Entry entry = new Entry();
                entry.Read(this);
                Entries.Add(entry.Name, entry);
            }

        }


        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Close();
                Entries.Clear();
                header = null;
            }

        }



        ~Psarc()
        {
            Dispose();
        }



        private string GetName(string Name)
        {
            if (HashNames.ContainsKey(Name))
            {
                return HashNames[Name];
            }
            return "0x" + Name;
        }

        protected void AddSign()
        {
            Stream.SetBytes(Encoding.ASCII.GetBytes("\u0054\u004c\u004f\u0055 \u0050\u0053\u0041\u0052\u0043 \u0054\u006f\u006f\u006c \u0042\u0079 \u0041\u006d\u0072 \u0053\u0068\u0061\u0068\u0065\u0065\u006e\u0028\u0061\u006d\u0072\u0073\u0068\u0061\u0068\u0065\u0065\u006e\u0036\u0031\u0029"));
        }

        private int[] ReadSizeArray()
        {
            Stream.Seek((StartOffset + (header.entryCount * header.entrySize)));

            //calc size bytes length
            int ByteLength = 0;
            for (int i = 1; i < header.DataBlockSize; i <<= 8)
            {
                ByteLength++;
            }

            List<int> values = new List<int>();

            while (Stream.Position != header.headerSize)
            {
                switch (ByteLength)
                {
                    case 1:
                        values.Add(Stream.GetByteValue());
                        break;
                    case 2:
                        values.Add(Stream.GetUShortValue());
                        break;
                    case 30://24 bit
                        break;
                    case 40:
                        values.Add(Stream.GetIntValue());
                        break;
                    case 80://64 bit
                        break;
                    default:
                        values.Add(Stream.GetIntValue());
                        break;
                }
            }

            return values.ToArray();
        }

        private byte[] GetFileRaw(Entry entry)
        {
            MStream uncompressfile = new MStream();
            foreach (var Block in entry.DempressBlocks)
            {
                Stream.Seek(Block.Offset);

                if (Block.DecompressSize == Block.CompressSize)
                {
                    uncompressfile.SetBytes(Stream.GetBytes(Block.CompressSize));
                }
                else
                {
                    uncompressfile.SetBytes(Compression.Decompress(Stream.GetBytes(Block.CompressSize), Block.DecompressSize, header.CompressionFormat));
                }
            }
            return uncompressfile.ToArray();
        }




        public byte[] GetFile(string EntryName)
        {
            return GetFileRaw(Entries[EntryName]);
        }


        private void BuildHaeder()
        {
            //make array size
            List<int> ListSize = new List<int>();

            foreach (var Entry in Entries)
            {

                if (Entry.Value.DempressBlocks.Count == 0)
                {
                    throw null;
                }


                Entry.Value.CompressedBlockSizeIndex = ListSize.Count;
                foreach (var block in Entry.Value.DempressBlocks)
                {
                    if (block.CompressSize != header.DataBlockSize)
                    {
                        ListSize.Add((ushort)block.CompressSize);
                    }
                    else
                    {
                        ListSize.Add(0);
                    }
                }
            }

            MStream mstream = new MStream(Endian.Big);
            mstream.Skip(HeaderSize);//header
            mstream.Skip(header.entryCount * header.entrySize);//Entries

            foreach (var block in ListSize)
            {
                mstream.SetUShortValue((ushort)block);
            }

            //time to resize file :\
            Stream.Position = 0;
            int ExtraSize;
            if (mstream.Length > header.headerSize)
            {
                ExtraSize = (int)mstream.Length - header.headerSize;
                Stream.InsertBytes(new byte[ExtraSize], false);
            }
            else
            {
                ExtraSize = header.headerSize - (int)mstream.Length;
                Stream.DeleteBytes(ExtraSize);
                ExtraSize = (int)mstream.Length - header.headerSize;
            }


            if (ExtraSize != 0)
            {
                foreach (var Entry in Entries)
                {
                    Entry.Value.Offset += ExtraSize;
                }
            }

            header.headerSize = (int)mstream.Length;
            ArraySize = ListSize.ToArray();
            mstream.Seek(0);
            header.Write(mstream);
            foreach (var Entry in Entries)
            {
                Entry.Value.Write(mstream);
            }

            Stream.SetBytes(mstream.ToArray());
        }


        public void ImportFiles(string[] Entrys, byte[][] FileBytes)
        {

            int Index = 0;
            foreach (string EntryName in Entrys)
            {
                ImportRaw(EntryName, FileBytes[Index++]);
            }
        }

        private void ImportRaw(string EntryName, byte[] FileBytes)
        {
            Entry entry = Entries[EntryName];
            MStream mStream = new MStream(FileBytes);

            Stream.Seek(0, SeekOrigin.End);
            AddSign();
            entry.Offset = (int)Stream.Position;
            entry.UncompressedSize = (int)mStream.Length;
            entry.CalcBlocksSize(this);

            foreach (var block in entry.DempressBlocks)
            {
                Stream.SetBytes(mStream.GetBytes(block.CompressSize));
            }
        }

        public void ImportFile(string EntryName, byte[] FileBytes)
        {
            ImportRaw(EntryName, FileBytes);
        }
        public void Save()
        {
            BuildHaeder();
        }
    }
}
