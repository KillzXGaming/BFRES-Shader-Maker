
using ShaderBuilder;
using ShaderLibrary;
using ShaderLibrary.IO;
using ShaderLibrary.Sharc;
using ShaderLibrary.WiiU;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharcConverter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //  SharcFile sharc2 = new("agl_technique_lght.sharc");
            //  SharcfbFileWiiU sharc3 = new("UserLight.sharcfb");
            //SharcfbFile sharc4 = new("UserLight.sharcfb");
            SharcfbFileWiiU s = new SharcfbFileWiiU("ankouOG.sharcfb");

            var prog = new GSHFile.GX2VertexHeader(new MemoryStream(s.Binaries[0].Data), false);

            args = new[] { "ankou" };

            foreach (var arg in args)
            {
                if (arg.EndsWith(".sharc"))
                    SharcConvert.ExportSource(new SharcFile(arg));
                if (Directory.Exists(arg))
                {
                    Console.WriteLine($"Processing folder {arg}");

                    var sharc = SharcConvert.SourceFromFolder(arg);
                    var sharcfb = SharcConvert.ToBinaryWiiU(sharc);

                    var prog1 = new GSHFile.GX2VertexHeader(new MemoryStream(sharcfb.Binaries[0].Data), false);
                    var prog2 = new GSHFile.GX2PixelHeader(new MemoryStream(sharcfb.Binaries[1].Data), false);

                    sharcfb.Save($"{sharcfb.Name}.sharcfb");
                }
            }
        }
    }
}