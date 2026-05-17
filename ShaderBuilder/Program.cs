

using BfresLibrary;
using BlenderBfresConverter;
using CommandLine;
using Newtonsoft.Json;
using ShaderBuilderTool.Convert;
using ShaderLibrary;
using System;
using System.IO;
using static ShaderBuilder.Program;

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
            /// Bfsha settings and shader creation info.
            /// </summary>
            public BfshaCreator.Args BfshaSettings { get; set; } = new();

            public void Save()
            {
                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            public static ArgSettings Load()
            {
                if (!File.Exists("Settings.json"))
                    return new ArgSettings();

                return JsonConvert.DeserializeObject<ArgSettings>(File.ReadAllText("Settings.json"));
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
            // Extract attempt
            if (args.Contains("-e"))
            {
                foreach (var arg in args)
                {
                    if (arg.EndsWith(".bfsha"))
                    {
                        BfshaFile bfsha = new BfshaFile(arg);
                        Directory.CreateDirectory(Path.Combine("Extracted", bfsha.Name));
                        foreach (var model in bfsha.ShaderModels.Values)
                            ShaderMetadata.DumpMetaData(model, Path.Combine("Extracted", bfsha.Name));
                    }
                }
                return;
            }

            ArgSettings settings = ArgSettings.Load();

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
                    else
                    {
                        bfres.ResFile = new ResFile(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load bfres {file}. Error: \n{ex}");
                }

                bfresFiles.Add(bfres);

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

        // Prepares and builds the bfsha 
        private static BfshaFile SetupShaders(BfshaCreator.Args args, IEnumerable<Model> models, IntermediateShader intermediateShader)
        {
            foreach (var model in models)
            {
                foreach (Shape shape in model.Shapes.Values)
                {
                    Material material = model.Materials[(int)shape.MaterialIndex];
                    IntermediateShader.ShaderModelInfo shaderModel = intermediateShader.ShaderModels[0];
                    BfshaCreator.Variant variant = new BfshaCreator.Variant();
                    variant.ShaderModel = shaderModel.Name;
                    args.Variants.Add(variant);
                    // Skin count can be controlled by variations too, it must match the bfres shape skin count
                    string skinningMacro = shaderModel.GetSkinningMacro();
                    if (!string.IsNullOrEmpty(skinningMacro))
                        variant.Options.Add(skinningMacro, shape.VertexSkinCount.ToString());

                    material.ShaderAssign.ShaderArchiveName = intermediateShader.Name;
                    material.ShaderAssign.ShadingModelName = shaderModel.Name;

                    foreach (var shaderOption in material.ShaderAssign.ShaderOptions)
                    {
                        // Try to map bfres material option values to the shader
                        IntermediateShader.OptionMacro optionMacro = shaderModel.StaticOptions.FirstOrDefault(x => x.Symbol == shaderOption.Key);
                        if (optionMacro == null)
                        {
                            Console.WriteLine($"Warning no lookup macro set for {shaderOption.Key}. Maybe missing a //@ comment in shader?");
                        }
                        else
                        {
                            string str = (string)shaderOption.Value;
                            // V10 using boolean but bfsha still need 0/1
                            if ((string)shaderOption.Value == "True")
                                str = "1";
                            if ((string)shaderOption.Value == "False")
                                str = "0";
                            variant.Options.Add(optionMacro.ID, str);
                        }
                    }
                }
            }
            return BfshaCreator.Create(intermediateShader, args);
        }
    }
}