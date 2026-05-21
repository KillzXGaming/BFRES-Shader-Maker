using CommandLine;
using System.Runtime.InteropServices;

#nullable enable
namespace BlenderBfresConverter;

internal class CommandLineOptions
{
    [Option('o', "outuput", Required = true, HelpText = "The output file and also the bfres file to replace model/texture/shader data. Keeps original animations.")]
    public string OutputFile { get; set; } = "";

    //[Option('i', "input", Required = true, HelpText = "The blender xml to convert from.")]
    //public string InputBlenderXml { get; set; } = "";

    [Option('s', "shaders", Required = true, HelpText = "Selects the shader folder to process. Must have one .vert and one .frag shader inside with .glsl for shared code.")]
    public string ShaderFolder { get; set; } = "";

    [Option('a', "allign", Required = false, HelpText = "Sets bfres alignment.")]
    public int Alignment { get; set; } = 4096 /*0x1000*/;

    [Option('w', "wiiu", Required = false, HelpText = "Determines to make Wii U binaries. Wii U requires gshCompile.exe in folder of this tool.")]
    public bool IsWiiU { get; set; } = false;

    [Option('e', "embed", Required = false, HelpText = "Embeds the shader binary to the bfres.")]
    public bool EmbeddedShader { get; set; } = true;

    [Option('n', "internal_name", Required = false, HelpText = "The internal bfres name for new bfres. Will set the same as the model name.")]
    public string InternalName { get; set; } = "course_model";
}
