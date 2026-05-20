using Newtonsoft.Json.Linq;
using ShaderLibrary;
using ShaderLibrary.Helpers;
using ShaderLibrary.WiiU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShaderBuilderTool.Convert
{
    public class BfshaCreator
    {
        public class Args
        {
            public ushort VersionMajor = 7;
            public byte VersionMinor = 0;
            public byte VersionMicro = 0;
            public Platforms Platform = Platforms.NX;

            public ushort BnshVersionMajor = 2;
            public byte BnshVersionMinor = 1;
            public byte BnshVersionMicro = 2;

            public uint ShaderVersionMajor = 1;
            public uint ShaderVersionMinor = 5;

            // For sharing stage data from existing bfsha
            public string SharedStageBfsha;
            public int SharedVertexStageProgramIndex = -1;
            public int SharedPixelStageProgramIndex = -1;

            // Adds more variants with option overrides.
            public List<Dictionary<string, string>> OptionOverrides = new();

            // Variant options that compile shader programs based on macro choices
            [JsonIgnore]
            public List<Variant> Variants = new();

            public static Args Load(string path)
            {
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<Args>(File.ReadAllText(path));
                return new Args();
            }

            public void Save(string path)
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                      WriteIndented = true,  
                });
                File.WriteAllText(path, json);
            }
        }

        public class Variant
        {
            public string ShaderModel;
            public Dictionary<string, string> Options = new();
        }

        public enum Platforms
        {
            WiiU,
            NX,
            Ounce,
        }

        public static Args MakeGameConfig(BfshaFile bfsha)
        {
            Args args = new()
            {
                VersionMajor = bfsha.BinHeader.VersionMajor,
                VersionMinor = bfsha.BinHeader.VersionMinor,
                VersionMicro = bfsha.BinHeader.VersionMicro,
                Platform = bfsha.IsWiiU ? Platforms.WiiU : Platforms.NX,
            };
            if (!bfsha.IsWiiU)
            {
                args.BnshVersionMajor = bfsha.ShaderModels[0].BnshFile.BinHeader.VersionMajor;
                args.BnshVersionMinor = bfsha.ShaderModels[0].BnshFile.BinHeader.VersionMinor;
                args.BnshVersionMicro = bfsha.ShaderModels[0].BnshFile.BinHeader.VersionMicro;
                if (bfsha.ShaderModels[0].BnshFile.Variations.Count > 0)
                {
                    // Not used atm but this could be used for matching versions
                    // Games should support 1.5 and newer versions are not well documented
                    var variation = bfsha.ShaderModels[0].BnshFile.Variations[0].BinaryProgram.VertexShader;
                    var ctrlShader = new ControlShader(variation.ControlCode);
                  //  args.ShaderVersionMajor = ctrlShader.MajorVer;
                  //  args.ShaderVersionMinor = ctrlShader.MinorVer;
                }
            }
            return args;
        }

        public static BfshaFile Create(string folder, Args args)
        {
            // Load/create an intermediate shader.
            // This gets some necessary meta data from our code to process and create an accurate output
            var intermediateShader = IntermediateShader.CreateFromFolder(folder);
            intermediateShader.Save(Path.Combine(folder, $"IntermediateShader.xml"));
            return Create(intermediateShader, args);
        }

        public static BfshaFile Create(IntermediateShader intermediateShader, Args args)
        {
            // Create wii u or switch binary.
            // For switch 2, it will be the same structure wise but api version is 200 for bnsh
            var bfsha = args.Platform == Platforms.WiiU ?
                intermediateShader.CreateBfshaFileWiiU() :
                intermediateShader.CreateBfshaFileSwitch();

            var sharedBfsha = new BfshaFile();
            if (!string.IsNullOrEmpty(args.SharedStageBfsha) && File.Exists(args.SharedStageBfsha))
                sharedBfsha = new BfshaFile(args.SharedStageBfsha);

            bfsha.BinHeader.VersionMajor = args.VersionMajor;
            bfsha.BinHeader.VersionMicro = args.VersionMicro;
            bfsha.BinHeader.VersionMinor = args.VersionMinor;

            if (bfsha.BinHeader.VersionMajor >= 7)
            {
                // Remap indices
                foreach (var shaderModel in bfsha.ShaderModels.Values)
                {
                    //material, shape, skeleton, option block indices
                    var mat_idx = shaderModel.BlockIndices[0];
                    var shp_idx = shaderModel.BlockIndices[1];
                    var skel_idx = shaderModel.BlockIndices[2];
                    var opt_idx = shaderModel.BlockIndices[3];
                    // shape, skeleton, mat, option
                    shaderModel.BlockIndices[0] = shp_idx;
                    shaderModel.BlockIndices[1] = skel_idx;
                    shaderModel.BlockIndices[2] = mat_idx;
                    shaderModel.BlockIndices[3] = opt_idx;
                }
            }

            if (args.Platform == Platforms.WiiU)
                CreateWiiU(bfsha, intermediateShader, args, sharedBfsha);
            else
                CreateSwitch(bfsha, intermediateShader, args, sharedBfsha);

            return bfsha;
        }

        static void CreateSwitch(BfshaFile bfsha,  IntermediateShader intermediateShader, Args args, BfshaFile sharedBfsha)
        {
            // Load variation data
            foreach (var shaderModel in bfsha.ShaderModels.Values)
            {
                // This has to be -1 for MK8 or it crashes
                shaderModel.DefaultProgramIndex = -1; 
                shaderModel.BnshFile.Header.ApiVersion = (ushort)(args.Platform == Platforms.Ounce ? 200 : 0);

                shaderModel.BnshFile.BinHeader.VersionMajor = args.BnshVersionMajor;
                shaderModel.BnshFile.BinHeader.VersionMicro = args.BnshVersionMicro;
                shaderModel.BnshFile.BinHeader.VersionMinor = args.BnshVersionMinor;

                // This should always be present
                var intermediateShaderModel = intermediateShader.ShaderModels.FirstOrDefault(
                    x => x.Name == shaderModel.Name);
                Trace.Assert(intermediateShaderModel != null);
                // Symbol names (optional, not usually needed)
                SetupSymbols(shaderModel, intermediateShaderModel);

                // Update and ensure all used choice variants fit the key table
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    // options which can have multiple branched paths
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        // Update the option table with new choices if necessary
                        UpdateOptionChoices(shaderModel, options);
                    }
                }
                ShaderOptionCreator.SetupOptionKeyFlags(shaderModel);

                // Preprocess the amount of programs we will need to build 
                List<int[]> keys = new();
                int total = 0;
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        int[] keyTable = BuildKeyData(shaderModel, options);
                        // Prevent duplicate variants
                        if (keys.Any(x => x.SequenceEqual(keyTable)))
                            continue;
                        keys.Add(keyTable);
                        total++;
                    }
                }


                ProgressBar progress = new();
                Console.Write("Building variations..");
                bool failed = false;

                List<Tuple<int[], BfshaShaderProgram>> programs = new();
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    // options which can have multiple branched paths
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        if (failed)
                            continue;

                        // Build our unique program key based on the option choices/macros
                        int[] keyTable = BuildKeyData(shaderModel, options);
                        // Prevent duplicate variants
                        if (programs.Select(x => x.Item1).Any(x => x.SequenceEqual(keyTable)))
                            continue;

                        BnshCreator.VariantArg varArgs = new();
                        varArgs.ShaderVersionMajor = args.ShaderVersionMajor;
                        varArgs.ShaderVersionMinor = args.ShaderVersionMinor;
                        // Source data
                        // TODO tess data, not really important atm
                        varArgs.VertexShader = Encoding.UTF8.GetString(intermediateShader.VertexShaderSource);
                        varArgs.FragmentShader = Encoding.UTF8.GetString(intermediateShader.FragmentShaderSource);
                        varArgs.GeometryShader = Encoding.UTF8.GetString(intermediateShader.GeometryShaderSource);
                        varArgs.ComputeShader = Encoding.UTF8.GetString(intermediateShader.ComputeShaderSource);
                        // We need to update source based on the variant macros used
                        // Glsl macros will be different from shader options (ie can use true/false, or strings may be numbers)
                        var macros = GetGlslMacros(intermediateShaderModel, options);
                        varArgs.VertexShader = GlslUtility.ApplyMacros(macros, varArgs.VertexShader);
                        varArgs.FragmentShader = GlslUtility.ApplyMacros(macros, varArgs.FragmentShader);
                        varArgs.GeometryShader = GlslUtility.ApplyMacros(macros, varArgs.GeometryShader);
                        varArgs.ComputeShader = GlslUtility.ApplyMacros(macros, varArgs.ComputeShader);
                        // Build the bnsh variant and binary nvn data and store it
                        var bnshVariation = BnshCreator.CreateVariation(varArgs);
                        if (args.SharedVertexStageProgramIndex != -1 && sharedBfsha.ShaderModels.Count > 0)
                        {
                            ControlShader controlCode = new ControlShader(bnshVariation.Variation.BinaryProgram.VertexShader.ControlCode);
                            var sharedVar = sharedBfsha.ShaderModels[0].Programs[args.SharedVertexStageProgramIndex].VariationIndex;
                            var binprog = sharedBfsha.ShaderModels[0].BnshFile.Variations[sharedVar].BinaryProgram;

                            bnshVariation.Variation.BinaryProgram.VertexShader.ByteCode = binprog.VertexShader.ByteCode;

                            controlCode.ProgramSize = (uint)(binprog.VertexShader.ByteCode.Length - 240);
                        }
                        if (args.SharedPixelStageProgramIndex != -1 && sharedBfsha.ShaderModels.Count > 0)
                        {
                            var sharedVar = sharedBfsha.ShaderModels[0].Programs[args.SharedPixelStageProgramIndex].VariationIndex;
                            var binprog = sharedBfsha.ShaderModels[0].BnshFile.Variations[sharedVar].BinaryProgram;

                            bnshVariation.Variation.BinaryProgram.FragmentShader = binprog.FragmentShader;
                            bnshVariation.Variation.BinaryProgram.FragmentShaderReflection = binprog.FragmentShaderReflection;
                        }

                        // Failed to compile, skip
                        if (bnshVariation.CompilerVertex == null || bnshVariation.CompilerFragment == null)
                        {
                            failed = true;
                            break;
                        }

                        progress.Report((double)programs.Count / (total - 1));

                        shaderModel.BnshFile.Variations.Add(bnshVariation.Variation);
                        // Finally make the program data prepared with symbol data for location and binding
                        var program = CreateShaderProgram(shaderModel,
                            intermediateShaderModel, bnshVariation.Variation,
                            bnshVariation.CompilerVertex.Symbols,
                            bnshVariation.CompilerFragment.Symbols);
                        programs.Add(Tuple.Create(keyTable, program));
                    }
                }
                progress.Dispose();
                if (failed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to build variations!");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("\nFinished building variations!");
                // Sorting is done by bit order
                SortKeys(ref programs);
                // Add keys/programs
                shaderModel.KeyTable = programs.SelectMany(x => x.Item1).ToArray();
                foreach (var prog in programs)
                    shaderModel.Programs.Add(prog.Item2);
            }
        }

        static void CreateWiiU(BfshaFile bfsha, IntermediateShader intermediateShader, Args args, BfshaFile sharedBfsha)
        {
            // Wii U needs gsh compile atm
            // There is cemu shader compiler but it has bugs/inaccuracies but may be supported in the future
            // If a developer wants to support it as it contiues, run https://github.com/Exzap/CafeGLSL
            if (!GSHCompile.IsValid())
                throw new Exception($"gshCompile.exe not present in folder of the tool!");

            // Load variation data
            foreach (var shaderModel in bfsha.ShaderModels.Values)
            {
                shaderModel.BnshFile.Header.ApiVersion = (ushort)(args.Platform == Platforms.Ounce ? 200 : 0);

                // This should always be present
                var intermediateShaderModel = intermediateShader.ShaderModels.FirstOrDefault(
                    x => x.Name == shaderModel.Name);
                Trace.Assert(intermediateShaderModel != null);
                // Symbol names (optional, not usually needed)
                SetupSymbols(shaderModel, intermediateShaderModel);

                // Update and ensure all used choice variants fit the key table before we process the programs
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    // options which can have multiple branched paths
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        // Update the option table with new choices if necessary
                        UpdateOptionChoices(shaderModel, options);
                    }
                }
                ShaderOptionCreator.SetupOptionKeyFlags(shaderModel);

                // Preprocess the amount of programs we will need to build 
                List<int[]> keys = new();
                int total = 0;
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        int[] keyTable = BuildKeyData(shaderModel, options);
                        // Prevent duplicate variants
                        if (keys.Any(x => x.SequenceEqual(keyTable)))
                            continue;
                        keys.Add(keyTable);
                        total++;
                    }
                }

                ProgressBar progress = new();
                Console.Write("Building variations..");

                bool failed = false;

                List<Tuple<int[], BfshaShaderProgram>> programs = new();
                foreach (var variant in args.Variants.Where(x => x.ShaderModel == shaderModel.Name))
                {
                    // options which can have multiple branched paths
                    foreach (var options in ProcessBranches(intermediateShaderModel, variant.Options))
                    {
                        if (failed)
                            break; 

                        // Build our unique program key based on the option choices/macros
                        int[] keyTable = BuildKeyData(shaderModel, options);
                        // Prevent duplicate variants
                        if (programs.Select(x => x.Item1).Any(x => x.SequenceEqual(keyTable)))
                            continue;

                        progress.Report((double)programs.Count / (total - 1));

                        // Source data
                        var vertexShader = Encoding.UTF8.GetString(intermediateShader.VertexShaderSource);
                        var fragmentShader = Encoding.UTF8.GetString(intermediateShader.FragmentShaderSource);
                        var geometryShader = Encoding.UTF8.GetString(intermediateShader.GeometryShaderSource);
                        // We need to update source based on the variant macros used
                        // Glsl macros will be different from shader options (ie can use true/false, or strings may be numbers)
                        var macros = GetGlslMacros(intermediateShaderModel, options);
                        macros.TryAdd("IS_WII_U", "1");

                        vertexShader = GlslUtility.ApplyMacros(macros, vertexShader);
                        fragmentShader = GlslUtility.ApplyMacros(macros, fragmentShader);
                        geometryShader = GlslUtility.ApplyMacros(macros, geometryShader);
                        // Build a raw .gsh file to be compiled with gsh compiler
                        var gshRaw = GSHCompile.CompileStages(vertexShader, fragmentShader, geometryShader);
                        if (gshRaw.Length == 0)
                        {
                            failed = true;
                            break; // gsh failed
                        }
                        // Build the program data via the gsh file binary
                        var gsh = new GSHFile(new MemoryStream(gshRaw));
                        var program = new BfshaShaderProgram();
                        programs.Add(Tuple.Create(keyTable, program));
                        BfshaGX2ShaderImporter.Import(shaderModel, program, gsh.Shaders[0], intermediateShaderModel);
                    }
                }

                progress.Dispose();
                if (failed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to build variations!");
                    Console.ResetColor();
                    return;
                }

                SortKeys(ref programs);
                foreach (var prog in programs)
                    shaderModel.Programs.Add(prog.Item2);
                shaderModel.KeyTable = programs.SelectMany(x => x.Item1).ToArray();
            }
        }

        static void SortKeys(ref List<Tuple<int[], BfshaShaderProgram>> programs)
        {
            // Ordered by bit data
            // This definitely did not take hours of debugging to notice this is important
            programs.Sort((a, b) =>
            {
                var keyA = a.Item1;
                var keyB = b.Item1;

                int len = Math.Min(keyA.Length, keyB.Length);

                for (int i = 0; i < len; i++)
                {
                    uint ua = (uint)keyA[i];
                    uint ub = (uint)keyB[i];

                    if (ua != ub)
                        return ua < ub ? -1 : 1;
                }
                return keyB.Length.CompareTo(keyA.Length);
            });
        }

        // Branch options
        // Ie some bfshas, all choices may have a gsys_assign option with programs having material, gbuffer, and z buffer shaders per material
        private static List<Dictionary<string, string>> ProcessBranches(
            IntermediateShader.ShaderModelInfo intermediate,
            Dictionary<string, string> options)
        {
            var results = new List<Dictionary<string, string>> { options };
            foreach (var option in intermediate.DynamicOptions.Where(x => x.Branch))
            {
                // Process all the choices
                var choices = option.Choices.Any()
                    ? option.Choices
                    : new List<string> { option.DefaultChoice };

                results = results
                    .SelectMany(current => choices.Select(choice =>
                    {
                        var copy = new Dictionary<string, string>(current);
                        copy[option.ID] = option.GetOptionChoice(choice);
                        return copy;
                    })).ToList();
            }
            return results;
        }

        // Ensures the user configured option choices exist in the shader model
        static void UpdateOptionChoices(ShaderModel shaderModel, Dictionary<string, string> options)
        {
            foreach (var op in options)
            {
                if (shaderModel.StaticOptions.ContainsKey(op.Key))
                {
                    var staticOption = shaderModel.StaticOptions[op.Key];
                    if (!staticOption.Choices.ContainsKey(op.Value))
                        staticOption.Choices.Add(op.Value, null);
                }
                if (shaderModel.DynamicOptions.ContainsKey(op.Key))
                {
                    var dynamicOption = shaderModel.DynamicOptions[op.Key];
                    if (!dynamicOption.Choices.ContainsKey(op.Value))
                        dynamicOption.Choices.Add(op.Value, null);
                }
            }
        }

        // Converts bfsha options to usable glsl macros used for compilation
        private static Dictionary<string, string> GetGlslMacros(
          IntermediateShader.ShaderModelInfo intermediate,
          Dictionary<string, string> options)
        {
            Dictionary<string, string> glslMacros = new Dictionary<string, string>();
            foreach (IntermediateShader.OptionMacro staticOption in intermediate.StaticOptions)
            {
                if (options.ContainsKey(staticOption.ID))
                    glslMacros.TryAdd(staticOption.ID, staticOption.GetMacroChoice(options[staticOption.ID]));
            }
            foreach (IntermediateShader.OptionMacro dynamicOption in intermediate.DynamicOptions)
            {
                if (options.ContainsKey(dynamicOption.ID))
                    glslMacros.TryAdd(dynamicOption.ID, dynamicOption.GetMacroChoice(options[dynamicOption.ID]));
            }
            return glslMacros;
        }

        // Creates program data based on symbol location/binding information
        static BfshaShaderProgram CreateShaderProgram(ShaderModel shaderModel,
            IntermediateShader.ShaderModelInfo intermediate,
            BnshFile.ShaderVariation variation,
             UAMShaderCompiler.ShaderSymbolData vertexSymbols,
             UAMShaderCompiler.ShaderSymbolData fragSymbols)
        {
            var program = new BfshaShaderProgram();
            program.VariationIndex = shaderModel.BnshFile.Variations.IndexOf(variation);

            // Set locations
            for (int i = 0; i < shaderModel.Samplers.Count; i++)
            {
                // Bfsha name to glsl symbol
                string name = intermediate.GetSamplerSymbolName(shaderModel.Samplers.GetKey(i));
                program.SamplerIndices.Add(new ShaderIndexHeader()
                {
                    VertexLocation = vertexSymbols.GetSamplerLocation(name),
                    FragmentLocation = fragSymbols.GetSamplerLocation(name),
                });
            }

            for (int i = 0; i < shaderModel.UniformBlocks.Count; i++)
            {
                // Bfsha name to glsl symbol
                string name = intermediate.GetUniformBlockSymbolName(shaderModel.UniformBlocks.GetKey(i));
                program.UniformBlockIndices.Add(new ShaderIndexHeader()
                {
                    VertexLocation = vertexSymbols.GetUniformBlockLocation(name),
                    FragmentLocation = fragSymbols.GetUniformBlockLocation(name),
                });
            }

            for (int i = 0; i < shaderModel.StorageBuffers.Count; i++)
            {
                // Bfsha name to glsl symbol
                string name = intermediate.GetStorageBlockSymbolName(shaderModel.UniformBlocks.GetKey(i));
                program.StorageBufferIndices.Add(new ShaderIndexHeader()
                {
                    VertexLocation = vertexSymbols.GetStorageBlockLocation(name),
                    FragmentLocation = fragSymbols.GetStorageBlockLocation(name),
                });
            }

            for (int i = 0; i < shaderModel.Attributes.Count; i++)
            {
                // Bfsha name to glsl symbol
                string name = intermediate.GetAttributeSymbolName(shaderModel.Attributes.GetKey(i));
                program.SetAttribute(i, vertexSymbols.HasAttribute(name));
            }
            /*
            for (int i = 0; i < shaderModel.UniformBlocks.Count; i++)
                Console.WriteLine($"block {shaderModel.UniformBlocks.GetKey(i)} {program.UniformBlockIndices[i].VertexLocation}");
            for (int i = 0; i < shaderModel.UniformBlocks.Count; i++)
                Console.WriteLine($"block {shaderModel.UniformBlocks.GetKey(i)} {program.UniformBlockIndices[i].FragmentLocation}");

            for (int i = 0; i < shaderModel.Samplers.Count; i++)
                Console.WriteLine($"samp {shaderModel.Samplers.GetKey(i)} {program.SamplerIndices[i].FragmentLocation}");

            for (int i = 0; i < shaderModel.Attributes.Count; i++)
                Console.WriteLine($"attr {shaderModel.Attributes.GetKey(i)} {program.IsAttributeUsed(i)}");
            */
            return program;
        }

        // Simple setup to prepare glsl symbols
        static void SetupSymbols(ShaderModel shaderModel, IntermediateShader.ShaderModelInfo intermediate)
        {
            // Symbol table
            // These must match the shader model uniform/sampler/buffer lists in amount
            // Includes bfsha ID then glsl shader name.
            shaderModel.SymbolData = new SymbolData();
            foreach (var b in shaderModel.UniformBlocks)
            {
                shaderModel.SymbolData.UniformBlocks.Add(
                    new SymbolData.SymbolEntry(b.Key, intermediate.GetUniformBlockSymbolName(b.Key)));
            }
            foreach (var b in shaderModel.Samplers)
            {
                shaderModel.SymbolData.Samplers.Add(
                    new SymbolData.SymbolEntry(b.Key, intermediate.GetSamplerSymbolName(b.Key)));
            }
            foreach (var b in shaderModel.StorageBuffers)
            {
                shaderModel.SymbolData.StorageBuffers.Add(
                    new SymbolData.SymbolEntry(b.Key, intermediate.GetStorageBlockSymbolName(b.Key)));
            }
        }

        // Builds a int[] key lookup created from bit data with the shader options used to lookup the shader program
        static int[] BuildKeyData(ShaderModel shaderModel, Dictionary<string, string> options)
        {
            int[] keyTable = new int[shaderModel.StaticKeyLength + shaderModel.DynamicKeyLength];
            foreach (var staticOption in shaderModel.StaticOptions.Values)
            {
                string choice = staticOption.DefaultChoice;

                if (options.ContainsKey(staticOption.Name))
                    choice = options[staticOption.Name];

                SetOptionKey(shaderModel, staticOption, choice, ref keyTable);
            }
            foreach (var dynamicOption in shaderModel.DynamicOptions.Values)
            {
                string choice = dynamicOption.DefaultChoice;

                if (options.ContainsKey(dynamicOption.Name))
                    choice = options[dynamicOption.Name];

                SetOptionKey(shaderModel, dynamicOption, choice, ref keyTable);
            }
            return keyTable;
        }

        // Updates the key table from a given option with the set choice
        static void SetOptionKey(ShaderModel shaderModel, ShaderOption option, string choice, ref int[] keyTable)
        {
            //current choice
            int choiceIndex = option.Choices.Keys.ToList().IndexOf(choice);
            if (choiceIndex == -1)
                throw new Exception($"Invalid choice input ({choice}) for {option.Name}!");

            int key_idx = option.Bit32Index;

            option.SetKey(ref keyTable[key_idx], choiceIndex);

            var new_choiceIdx = option.GetChoiceIndex(keyTable[key_idx]);
            if (new_choiceIdx != choiceIndex)
                throw new Exception("Failed to set choice index!");
        }
    }
}
