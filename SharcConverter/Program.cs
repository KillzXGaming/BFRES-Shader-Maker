
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
            foreach (var arg in args)
            {
                if (arg.EndsWith(".sharc"))
                    SharcConvert.ExportSource(new SharcFile(arg));
                if (Directory.Exists(arg))
                {
                    Console.WriteLine($"Processing folder {arg}");

                    var sharc = SharcConvert.SourceFromFolder(arg);
                    if (sharc.FileHeader.ByteOrder == 1)
                    {
                        var sharcfb = SharcConvert.ToBinaryWiiU(sharc);
                        sharcfb.Save($"{sharcfb.Name}.sharcfb");
                    }
                    else
                    {
                        var sharcfb = SharcConvert.ToBinary(sharc);
                        sharcfb.Save($"{sharcfb.Name}.sharcfb");
                    }
                }
            }
        }
    }
}