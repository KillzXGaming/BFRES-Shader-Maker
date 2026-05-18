

using BfresLibrary;
using BlenderBfresConverter;
using CommandLine;
using Newtonsoft.Json;
using ShaderBuilderTool.Convert;
using ShaderLibrary;
using ShaderLibrary.IO;
using System;
using System.IO;
using System.Text;

namespace ShaderBuilder
{
    public static class Program
    {
        public class ArgSettings // For setting automatic defaults
        {
            /// <summary>
            /// Shader folder to process glsl shaders
            ///  Must have one .vert, one .frag, extra need to be .glsl
            /// If multiple sub folders, creates a shader model per folder.
            /// </summary>
            public string ShaderFolder { get; set; } = "Turbo";
            /// <summary>
            /// The folder to process bfres files to combine material settings into one uber shader.
            /// </summary>
            public string BfresFolder { get; set; } = "Bfres";
            /// <summary>
            /// The output folder of the adjusted bfres files and bfsha (if not embedded).
            /// </summary>
            public string OutputFolder { get; set; } = "Output";
            /// <summary>
            /// Determines if shader is embedded into the bfres.
            /// </summary>
            public bool IsEmbeddedBfsha { get; set; } = true;
            /// <summary>
            /// Determines to use Wii U bfsha.
            /// </summary>
            public bool IsWiiU { get; set; } = false;
            /// <summary>
            /// Bfsha settings and shader creation info.
            /// </summary>
            public BfshaCreator.Args BfshaSettings { get; set; } = new();

            public void Save(string name = "Settings.json")
            {
                File.WriteAllText(name, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            public static ArgSettings Load(string name = "Settings.json")
            {
                if (!File.Exists(name))
                    return new ArgSettings();

                return JsonConvert.DeserializeObject<ArgSettings>(File.ReadAllText(name));
            }
        }

        public class BfresFile
        {
            public ResFile ResFile { get; set; }
            public string FilePath { get; set; }
            public bool IsCompressed { get; set; }
            public string OutputFilePath { get; set; }
        }

        public static void Main(string[] args)
        {
            // args = new[] { "-e", @"E:\0 BACKUP 11 4 2025\yuzu\mk8 proto\TestProto\romfs\Driver\Turbo_UBEROG.bfsha" };

            ArgSettings settings = ArgSettings.Load();
            // Settings override
            if (args.Any(x => x.Contains(".json")))
            {
                foreach (var arg in args)
                {
                    if (arg.Contains(".json"))
                        settings = ArgSettings.Load(arg);
                }
            }   
            if (settings.IsWiiU)
                settings.BfshaSettings.Platform = BfshaCreator.Platforms.WiiU;

            // Extract attempt which is expecting a drag/drop of a bfres or bfsha.
            foreach (var arg in args)
            {
                // Dumpable materials that user can edit to inject back to
                if (arg.EndsWith(".bfres") || arg.EndsWith(".szs") || arg.EndsWith(".sbfres"))
                {
                    ResFile resFile = new ResFile();
                    // Check for compression
                    if (YAZ0.IsCompressed(arg))
                        resFile = new ResFile(new MemoryStream(szs.Decode(File.ReadAllBytes(arg))));
                    else
                        resFile = new ResFile(arg);

                    foreach (var model in resFile.Models.Values)
                    {
                        // Bfres name -> model name
                        // Users can add new materials or inject the existing
                        string dir = Path.Combine(settings.BfresFolder, Path.GetFileNameWithoutExtension(arg), model.Name);
                        Directory.CreateDirectory(dir);

                        foreach (var mat in model.Materials.Values)
                            mat.Export(Path.Combine(dir, $"{mat.Name}.json"), resFile);
                    }
                    return;
                }
                if (arg.EndsWith(".bfsha"))
                {
                    BfshaFile bfsha = new BfshaFile(arg);
                    var config = BfshaCreator.MakeGameConfig(bfsha);
                    var settings2 = new ArgSettings()
                    {
                        BfshaSettings = config
                    };
                    settings2.Save($"{bfsha.Name}.json");

                    Directory.CreateDirectory(Path.Combine("Extracted", bfsha.Name));
                    foreach (var model in bfsha.ShaderModels.Values)
                        ShaderMetadata.DumpMetaData(model, Path.Combine("Extracted", bfsha.Name));
                    return;
                }
            }

            if (!Directory.Exists(settings.ShaderFolder))
                throw new Exception($"Failed to find shader folder {settings.ShaderFolder}.");
            if (!Directory.Exists(settings.BfresFolder))
                throw new Exception($"Failed to find bfres folder {settings.BfresFolder}.");

            Directory.CreateDirectory(settings.OutputFolder);

            List<BfresFile> bfresFiles = new();

            // First create intermediate shader from folder of glsl code
            var shader = IntermediateShader.CreateFromFolder(settings.ShaderFolder);
            // Process each bfres 
            foreach (var file in Directory.GetFiles(settings.BfresFolder, "*", SearchOption.AllDirectories))
            {
                BfresFile bfres = new BfresFile();
                try
                {
                    // Check for compression
                    // Todo add SARC support
                    if (YAZ0.IsCompressed(file))
                    {
                        bfres.IsCompressed = true;
                        bfres.ResFile = new ResFile(new MemoryStream(szs.Decode(File.ReadAllBytes(file))));
                    }
                    else if (IsBfres(file))
                    {
                        bfres.ResFile = new ResFile(file);
                    }
                    else // Not a valid bfres, skip
                        continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load bfres {file}. Error: \n{ex}");
                    continue;
                }
                bfresFiles.Add(bfres);

                // Look for injectable materials
                foreach (var model in bfres.ResFile.Models.Values)
                {
                    // Bfres name -> model name
                    // Users can add new materials or inject the existing
                    string dir = Path.Combine(settings.BfresFolder, Path.GetFileNameWithoutExtension(file), model.Name);
                    if (!Directory.Exists(dir))
                        continue;

                    foreach (var jsonPath in Directory.GetFiles(dir, "*.json"))
                    {
                        string name = Path.GetFileNameWithoutExtension(jsonPath);
                        // Import material
                        try
                        {
                            Console.WriteLine($"Importing material {name}");
                            if (model.Materials.ContainsKey(name))
                                model.Materials[name].Import(jsonPath, bfres.ResFile);
                            else // Make new if not present
                            {
                                var mat = new Material();
                                mat.Import(jsonPath, bfres.ResFile);
                                mat.Name = name;
                                model.Materials.Add(name, mat);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to import material {name}. {ex}");
                        }

                    }
                }

                string subDir = Path.GetRelativePath(settings.BfresFolder, Path.GetDirectoryName(file));
                bfres.OutputFilePath = Path.Combine(settings.OutputFolder, subDir, Path.GetFileName(file));

                if (settings.IsEmbeddedBfsha)
                {
                    var bfsha = SetupShaders(settings.BfshaSettings,
                        bfres.ResFile.Models.Values, shader);

                    // Embed in the bfsha
                    // Remove any bfsha as these will be computed into one new bfsha
                    foreach (var key in bfres.ResFile.ExternalFiles.Keys.ToList())
                    {
                        if (key.EndsWith(".bfsha"))
                            bfres.ResFile.ExternalFiles.RemoveKey(key);
                    }

                    var bfsha_mem = new MemoryStream();
                    bfsha.Save(bfsha_mem);
                    bfres.ResFile.ExternalFiles.Add(bfsha.Name + ".bfsha", new ExternalFile()
                    {
                        Name = bfsha.Name + ".bfsha",
                        Data = bfsha_mem.ToArray(),
                    }); 

                    if (bfres.IsCompressed)
                    {
                        MemoryStream mem = new MemoryStream();
                        bfres.ResFile.Save(mem);
                        byte[] bytes = YAZ0.Compress(mem.ToArray());
                        File.WriteAllBytes(bfres.OutputFilePath, bytes);

                        bfsha.Save(bfres.OutputFilePath + ".bfsha");
                    }
                    else
                        bfres.ResFile.Save(bfres.OutputFilePath);
                }
            }

            // Single bfsha that is not per bfres
            if (!settings.IsEmbeddedBfsha)
            {
                var bfsha = SetupShaders(settings.BfshaSettings,
                    bfresFiles.SelectMany(x => x.ResFile.Models.Values), shader);

                bfsha.Save(Path.Combine(settings.OutputFolder, bfsha.Name + ".bfsha"));
            }
        }

        static bool IsBfres(string filePath)
        {
            using (var reader = new BinaryDataReader(File.OpenRead(filePath)))
            {
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                return magic == "FRES";
            }
        }

        // Prepares and builds the bfsha 
        private static BfshaFile SetupShaders(BfshaCreator.Args args, IEnumerable<Model> models, IntermediateShader intermediateShader)
        {
            foreach (var model in models)
            {
                foreach (Shape shape in model.Shapes.Values)
                {
                    Material material = model.Materials[(int)shape.MaterialIndex];
                    SetupMaterial(args, material, shape.VertexSkinCount, intermediateShader);
                }
                // Check for unassigned materials
                // The game materials shouldn't do this but custom models could sometimes have loose materials that may cause errors if the user reassigns after
                var skin_counts = model.Shapes.Values.Select(x => x.VertexSkinCount).Distinct().ToList();
                foreach (Material mat in model.Materials.Values)
                {
                    int index = model.Materials.IndexOf(mat);
                    if (model.Shapes.Values.Any(x => x.MaterialIndex == index))
                        continue;

                    // Add a variant to all usable skin counts if the user decides to reassign the material to either mesh
                    for (int i = 0; i < skin_counts.Count; i++)
                        SetupMaterial(args, mat, skin_counts[i], intermediateShader);
                }
            }
            return BfshaCreator.Create(intermediateShader, args);
        }

        static void SetupMaterial(BfshaCreator.Args args, Material material, int skinCount, IntermediateShader intermediateShader)
        {
            IntermediateShader.ShaderModelInfo shaderModel = intermediateShader.ShaderModels[0];
            BfshaCreator.Variant variant = new BfshaCreator.Variant();
            variant.ShaderModel = shaderModel.Name;
            args.Variants.Add(variant);
            // Skin count can be controlled by variations too, it must match the bfres shape skin count
            string skinningMacro = shaderModel.GetSkinningMacro();
            if (!string.IsNullOrEmpty(skinningMacro))
                variant.Options.Add(skinningMacro, skinCount.ToString());

            material.ShaderAssign.ShaderArchiveName = intermediateShader.Name;
            material.ShaderAssign.ShadingModelName = shaderModel.Name;

            foreach (var shaderOption in material.ShaderAssign.ShaderOptions)
            {
                // Try to map bfres material option values to the shader
                IntermediateShader.OptionMacro optionMacro = shaderModel.StaticOptions.FirstOrDefault(x => x.Symbol == shaderOption.Key);
                if (optionMacro == null)
                {
                    Console.WriteLine($"Warning no lookup macro set for {shaderOption.Key}. Maybe missing //@ comment in shader?");
                }
                else
                {
                    string str = (string)shaderOption.Value;
                    // Render info can toggle macro settings
                    if (!string.IsNullOrEmpty(optionMacro.RenderInfo) &&
                        material.RenderInfos.ContainsKey(optionMacro.RenderInfo))
                    {
                        string renderInfoChoice = material.GetRenderInfoString(optionMacro.RenderInfo);
                        str = optionMacro.GetMacroChoiceByRenderInfo(renderInfoChoice);
                    }

                    // V10 using boolean but bfsha still need 0/1
                    if ((string)shaderOption.Value == "True")
                        str = "1";
                    if ((string)shaderOption.Value == "False")
                        str = "0";

                    if (args.Platform == BfshaCreator.Platforms.WiiU && material.RenderState != null)
                    {
                        // Render state options
                        switch (optionMacro.RenderStatePropertyWiiU)
                        {
                            case "ALPHA_TEST":
                                str = material.RenderState.AlphaControl.AlphaTestEnabled ? "1" : "0";
                                break;
                            case "RENDER_STATE":
                                switch (material.RenderState.FlagsMode)
                                {
                                    case RenderStateFlagsMode.Opaque: str = "0"; break;
                                    case RenderStateFlagsMode.AlphaMask: str = "1"; break;
                                    case RenderStateFlagsMode.Translucent: str = "2"; break;
                                    case RenderStateFlagsMode.Custom: str = "3"; break;
                                }
                                // Temp, need to add a way to filter wii u/switch macros
                                // Switch/Wii U shaders are shared, wii u does not use this option
                                // If gsys_alpha_test_enable is present, the shader viewer tools fail to load it unless AlphaTestEnabled is matching
                                if (material.RenderState.AlphaControl.AlphaTestEnabled)
                                    variant.Options.TryAdd("gsys_alpha_test_enable", "1");
                                break;
                            case "ALPHA_FUNC":
                                switch (material.RenderState.AlphaControl.AlphaFunc)
                                {
                                    case BfresLibrary.GX2.GX2CompareFunction.Never:
                                        str = "0";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.Less:
                                        str = "1";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.LessOrEqual:
                                        str = "3";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.Greater:
                                        str = "4";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.NotEqual:
                                        str = "5";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.GreaterOrEqual:
                                        str = "6";
                                        break;
                                    case BfresLibrary.GX2.GX2CompareFunction.Equal:
                                        str = "7";
                                        break;
                                }
                                break;
                        }
                    }
                    variant.Options.TryAdd(optionMacro.ID, str);
                }
            }
        }
    }
}