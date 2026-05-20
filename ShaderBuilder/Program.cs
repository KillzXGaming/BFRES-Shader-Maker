

using BfresLibrary;
using BlenderBfresConverter;
using CafeLibrary;
using CommandLine;
using Newtonsoft.Json;
using ShaderBuilderTool.Convert;
using ShaderLibrary;
using ShaderLibrary.IO;
using System;
using System.Diagnostics;
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
            /// The output shader folder of the adjusted bfsha (if not embedded).
            /// </summary>
            public string OutputShaderFolder { get; set; } = "Output";
            /// <summary>
            /// The output bfres folder of the adjusted bfres files.
            /// </summary>
            public string OutputBfresFolder { get; set; } = "Output";
            /// <summary>
            /// Determines if shader is embedded into the bfres.
            /// </summary>
            public bool IsEmbeddedBfsha { get; set; } = true;
            /// <summary>
            /// Determines to use Wii U bfsha.
            /// </summary>
            public bool IsWiiU { get; set; } = false;
            /// <summary>
            /// Determines to make a new sarc to store the bfsha shader.
            /// </summary>
            public bool ShaderSarc { get; set; } = false;
            /// <summary>
            /// The compression speed to compress the file data. 1 for fastest, 9 for slowest but smallest.
            /// </summary>
            public int Yaz0Level { get; set; } = 3;
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

        public class ArchiveFile
        {
            public SarcData Sarc;
            public bool IsCompressed { get; set; }
            public string OutputFilePath { get; set; }

            public void Save()
            {
                var saved = SARC_Parser.PackN(Sarc);
                var bytes = saved.Item2;
                if (IsCompressed)
                    bytes = YAZ0.Compress(saved.Item2, 3, (uint)saved.Item1);
                File.WriteAllBytes(this.OutputFilePath, bytes);
            }
        }

        public class BfresFile
        {
            public ResFile ResFile { get; set; }
            public string FilePath { get; set; }
            public bool IsCompressed { get; set; }
            public string OutputFilePath { get; set; }
            public SarcData Archive;
            public string ArchiveFileName;

            public void Save(int yaz0Level)
            {
                if (Archive != null) // .sarc (save just bfres to archive)
                {
                    // Shouldn't happen but just incase
                    MemoryStream mem = new MemoryStream();
                    this.ResFile.Save(mem);
                    Archive.Files[ArchiveFileName] = mem.ToArray();
                }
                else // .bfres or compressed .szs
                {
                    if (this.IsCompressed)
                    {
                        MemoryStream mem = new MemoryStream();
                        this.ResFile.Save(mem);

                        Stopwatch sw = Stopwatch.StartNew();
                        Console.WriteLine($"Compressing Yaz0 level {yaz0Level}");

                        byte[] bytes = YAZ0.Compress(mem.ToArray(), yaz0Level);
                        File.WriteAllBytes(this.OutputFilePath, bytes);

                        sw.Stop();
                        Console.WriteLine($"Compressed in {sw.Elapsed}");
                    }
                    else
                        this.ResFile.Save(this.OutputFilePath);
                }
            }
        }

        public static void Main(string[] args)
        {
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
            if (ExtractFiles(args, settings))
                return;

            if (!Directory.Exists(settings.ShaderFolder))
                throw new Exception($"Failed to find shader folder {settings.ShaderFolder}.");
            if (!Directory.Exists(settings.BfresFolder))
                throw new Exception($"Failed to find bfres folder {settings.BfresFolder}.");

            Directory.CreateDirectory(settings.OutputShaderFolder);
            Directory.CreateDirectory(settings.OutputBfresFolder);

            List<BfresFile> bfresFiles = new();

            // First create intermediate shader from folder of glsl code
            var shader = IntermediateShader.CreateFromFolder(settings.ShaderFolder);
            if (shader.ShaderModels.Count == 0)
            {
                throw new Exception($"Failed to load shaders in {settings.ShaderFolder}!");

            }
            // Process each bfres 
            List<ArchiveFile> archives = new();
            foreach (var file in Directory.GetFiles(settings.BfresFolder, "*", SearchOption.AllDirectories))
            {
                string subDir = Path.GetRelativePath(settings.BfresFolder, Path.GetDirectoryName(file));
                string outputFilePath = Path.Combine(settings.OutputBfresFolder, subDir, Path.GetFileName(file));
                bool isCompressed = false;

                try
                {
                    // Check for compression
                    Stream stream = File.OpenRead(file);
                    if (YAZ0.IsCompressed(stream))
                    {
                        stream?.Close();
                        isCompressed = true;
                        stream = new MemoryStream(YAZ0.Decompress(File.ReadAllBytes(file)));
                    }
                    if (SARC_Parser.IsSarc(stream))
                    {
                        var sarc = SARC_Parser.UnpackRamN(stream);
                        archives.Add(new ArchiveFile()
                        {
                            Sarc = sarc,
                            OutputFilePath = outputFilePath,
                            IsCompressed = isCompressed,
                        });

                        foreach (var f in sarc.Files)
                        {
                            if (f.Key.EndsWith(".bfres"))
                            {
                                bfresFiles.Add(new BfresFile()
                                {
                                    Archive = sarc,
                                    FilePath = f.Key,
                                    ArchiveFileName = f.Key,
                                    ResFile = new ResFile(new MemoryStream(f.Value)),
                                });
                            }
                        }
                    }
                    else if (IsBfres(stream))
                    {
                        bfresFiles.Add(new BfresFile()
                        {
                            FilePath = file,
                            IsCompressed = isCompressed,
                            OutputFilePath = outputFilePath,
                            ResFile = new ResFile(stream),
                        });
                    }
                    else // Not a valid bfres, skip
                        continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load bfres {file}. Error: \n{ex}");
                    continue;
                }
            }

            foreach (var bfres in bfresFiles)
            { 
                if (bfres.ResFile == null)
                    continue;

                // Look for injectable materials
                foreach (var model in bfres.ResFile.Models.Values)
                {
                    // Bfres name -> model name
                    // Users can add new materials or inject the existing
                    string dir = Path.Combine(settings.BfresFolder, Path.GetFileNameWithoutExtension(bfres.FilePath), model.Name);
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
                    bfres.Save(settings.Yaz0Level);
                }
            }

            // Single bfsha that is not per bfres
            if (!settings.IsEmbeddedBfsha)
            {
                var bfsha = SetupShaders(settings.BfshaSettings,
                    bfresFiles.SelectMany(x => x.ResFile.Models.Values), shader);

                if (settings.ShaderSarc)
                {
                    SarcData sarc = new()
                    {
                        endianness = settings.IsWiiU ? Syroot.BinaryData.ByteOrder.BigEndian : Syroot.BinaryData.ByteOrder.LittleEndian,
                        Files = new(),
                        HashOnly = false,
                    };
                    var mem = new MemoryStream();
                    bfsha.Save(mem);
                    sarc.Files.Add(bfsha.Name + ".bfsha", mem.ToArray());
                    var saved = SARC_Parser.PackN(sarc);
                    // Default to compressed
                    var compressed = YAZ0.Compress(saved.Item2, settings.Yaz0Level, (uint)saved.Item1);
                    File.WriteAllBytes(Path.Combine(settings.OutputShaderFolder, $"{bfsha.Name}.szs"), compressed);
                }
                else
                    bfsha.Save(Path.Combine(settings.OutputShaderFolder, bfsha.Name + ".bfsha"));
                // Save each bfres
                foreach (var bfres in bfresFiles)
                    bfres.Save(settings.Yaz0Level);
            }

            // Save each archive if loaded
            foreach (var archive in archives)
            {
                archive.Save();
            }
        }

        static bool ExtractFiles(string[] args, ArgSettings settings)
        {
            foreach (var arg in args)
            {
                // Dumpable materials that user can edit to inject back to
                if (arg.EndsWith(".bfres") || arg.EndsWith(".szs") || arg.EndsWith(".sbfres"))
                {
                    void DumpResFile(ResFile resFile)
                    {
                        foreach (var model in resFile.Models.Values)
                        {
                            // Bfres name -> model name
                            // Users can add new materials or inject the existing
                            string dir = Path.Combine(settings.BfresFolder, Path.GetFileNameWithoutExtension(arg), model.Name);
                            Directory.CreateDirectory(dir);

                            foreach (var mat in model.Materials.Values)
                                mat.Export(Path.Combine(dir, $"{mat.Name}.json"), resFile);
                        }
                    }
                    // Check for compression
                    Stream stream = File.OpenRead(arg);
                    if (YAZ0.IsCompressed(stream))
                    {
                        stream?.Close();
                        stream = new MemoryStream(YAZ0.Decompress(File.ReadAllBytes(arg)));
                    }
                    if (SARC_Parser.IsSarc(stream))
                    {
                        var sarc = SARC_Parser.UnpackRamN(stream);
                        foreach (var f in sarc.Files)
                        {
                            if (f.Key.EndsWith(".bfres"))
                                DumpResFile(new ResFile(new MemoryStream(f.Value)));
                        }
                    }
                    else if (IsBfres(stream))
                        DumpResFile(new ResFile(stream));

                    return true;
                }
                // Dumpable shaders for referencing settings and meta data for creating glsl code
                if (arg.EndsWith(".bfsha"))
                {
                    BfshaFile bfsha = new BfshaFile(arg);
                    var config = BfshaCreator.MakeGameConfig(bfsha);
                    var settings2 = new ArgSettings()
                    {
                        IsWiiU = config.Platform == BfshaCreator.Platforms.WiiU,
                        BfshaSettings = config
                    };
                    Directory.CreateDirectory(Path.Combine("Extracted", bfsha.Name));
                    settings2.Save(Path.Combine("Extracted", bfsha.Name, $"{bfsha.Name}Settings.json"));

                    foreach (var model in bfsha.ShaderModels.Values)
                        ShaderMetadata.DumpMetaData(model, Path.Combine("Extracted", bfsha.Name));
                    return true;
                }
            }
            return false;
        }

        static bool IsBfres(Stream stream)
        {
            using (var reader = new BinaryDataReader(stream, false, true))
            {
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                stream.Position = 0;
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
                    // Option overrides which will compile more variants based on user specified settings
                    if (args.OptionOverrides.Count > 0)
                    {
                        foreach (var dict in args.OptionOverrides)
                            SetupMaterial(args, material, shape.VertexSkinCount, intermediateShader, dict);
                    }
                    else
                        SetupMaterial(args, material, shape.VertexSkinCount, intermediateShader, new Dictionary<string, string>());
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
                        SetupMaterial(args, mat, skin_counts[i], intermediateShader, new Dictionary<string, string>());
                }
            }
            return BfshaCreator.Create(intermediateShader, args);
        }

        static void SetupMaterial(BfshaCreator.Args args, Material material, int skinCount, IntermediateShader intermediateShader,
            Dictionary<string, string> optionOverrides)
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
                    if (optionOverrides.ContainsKey(shaderOption.Key))
                        str = optionOverrides[shaderOption.Key];

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