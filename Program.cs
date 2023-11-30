using System.IO;
using System.Text;

namespace SPORE__Creatures_extract
{
    class Program
    {
        public static BinaryReader rom;
        public static BinaryReader romFileNames;
        public static BinaryWriter bw;
        static void Main(string[] args)
        {
            using FileStream source = File.OpenRead(args[0]);
            long filelength = source.Length;
            rom = new BinaryReader(source);
            rom.BaseStream.Position = 0;
            romFileNames = new BinaryReader(File.OpenRead(args[1]));
            romFileNames.BaseStream.Position = 0;

            FILE file = GetFileData();

            file.subfile[^1].size = (uint)(source.Length - file.subfile[^1].offset);
            file.subfile[^1].nameSize = (uint)(source.Length - file.subfile[^1].nameOffset);

            for (int i = 0; i < 9151; i++)
            {
                string newFolder = Path.GetDirectoryName(source.Name) + "\\" + Path.GetFileNameWithoutExtension(source.Name) + file.newFolder;

                romFileNames.BaseStream.Position = file.subfile[i].nameOffset;
                file.subfile[i].name = Encoding.ASCII.GetString(romFileNames.ReadBytes((int)file.subfile[i].nameSize));

                Directory.CreateDirectory(newFolder);
                bw = new BinaryWriter(File.OpenWrite(newFolder + "\\" + file.subfile[i].name));
                rom.BaseStream.Position = file.subfile[i].offset;
                bw.Write(rom.ReadBytes((int)file.subfile[i].size));
                bw.Close();
            }
            //romFileNames.BaseStream.Position = file.subfile[i].offset;
            //file.subfile[i].name = Encoding.ASCII.GetString(rom.ReadBytes((int)file.subfile[i].size));
        }

        public static FILE GetFileData()
        {
            FILE file = new();
            file.subfile = new SUBFILE[9151];
            for (int i = 0; i < 9151; i++)
            {
                file.subfile[i].offset = rom.ReadUInt32();
                file.subfile[i].nameOffset = romFileNames.ReadUInt32();
            }
            for (int i = 0; i < 9151; i++)
            {
                rom.BaseStream.Position = file.subfile[i].offset;
                romFileNames.BaseStream.Position = file.subfile[i].nameOffset;
            }
            for (int i = 0; i < 9150; i++)
            {
                file.subfile[i].size = file.subfile[i + 1].offset - file.subfile[i].offset;
                file.subfile[i].nameSize = file.subfile[i + 1].nameOffset - file.subfile[i].nameOffset;
            }

            return file;
        }

        public class FILE
        {
            public string path;
            public string type;
            public string newFolder;
            public string magic;
            public ushort endian;
            public uint size;
            public uint subfileDataOffset;
            public uint numSubfiles;
            public uint numSubfolders;
            public SUBFILE[] subfile;
            public SUBFOLDER[] subfolder;
        }

        public struct SUBFOLDER
        {
            public uint nameOffset;
            public string Name;
            public uint numSubfiles;
            public SUBFOLDER[] subfolder;
            public SUBFILE[] subfile;
        }

        public struct SUBFILE
        {
            public uint nameOffset;
            public uint nameSize;
            public string name;
            public uint offset;
            public uint size;
        }
    }
}
