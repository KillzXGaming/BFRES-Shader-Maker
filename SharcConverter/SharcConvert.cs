using ShaderBuilderTool;
using ShaderLibrary;
using ShaderLibrary.Sharc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ShaderBuilder
{
    public class SharcConvert
    {
        public class SharcProgramMetaData
        {
            [XmlAttribute]
            public string name { get; set; }
            [XmlAttribute]
            public string vertex_shader { get; set; }
            [XmlAttribute]
            public string fragment_shader { get; set; }
            [XmlArray("Variations")]
            [XmlArrayItem("Variation")]
            public List<SharcProgramMacro> macros { get; set; } = new();
        }

        public class SharcProgramMacro
        {
            [XmlAttribute]
            public string name { get; set; }
            [XmlAttribute]
            public List<string> values { get; set; } = new();
        }

        public class SharcMetaData
        {
            [XmlArray("Programs")]
            [XmlArrayItem("Program")]
            public List<SharcProgramMetaData> Programs { get; set; } = new();

            public string ToXml()
            {
                var serializer = new XmlSerializer(typeof(SharcMetaData));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, this);
                    return writer.ToString();
                }
            }
            public static SharcMetaData FromXml(string text)
            {
                var serializer = new XmlSerializer(typeof(SharcMetaData));
                using var reader = new StringReader(text);

                return (SharcMetaData)serializer.Deserialize(reader);
            }

        }

        public static void ExportSource(SharcFile sharc)
        {
            Directory.CreateDirectory(sharc.Name);


            var vertexShaderIndices = sharc.Programs.Select(x => x.VertexShaderIndex).ToList();
            var fragShaderIndices = sharc.Programs.Select(x => x.FragmentShaderIndex).ToList();

            bool hasSharedShaders = vertexShaderIndices.Count != vertexShaderIndices.Distinct().ToList().Count ||
                                    fragShaderIndices.Count != fragShaderIndices.Distinct().ToList().Count;

            // Meta data to load sharcfb program data
            SharcMetaData meta = new();
            foreach (var prog in sharc.Programs)
            {
                var metaProgram = new SharcProgramMetaData()
                {
                    name = prog.Name,
                    vertex_shader = prog.VertexShaderIndex != -1 ? sharc.Sources[prog.VertexShaderIndex].Name : "",
                    fragment_shader = prog.FragmentShaderIndex != -1 ? sharc.Sources[prog.FragmentShaderIndex].Name : "",
                };
                meta.Programs.Add(metaProgram);

                // Rather than automating macros from glsl code, store them as meta data as the sources are shared between programs
                if (hasSharedShaders) {
                    foreach (var variant in prog.VariationMacros) {
                        metaProgram.macros.Add(new SharcProgramMacro()
                        {
                            name = variant.Name,
                            values = variant.Values,
                        });
                    }
                }
            }
            // Export meta xml
            File.WriteAllText(Path.Combine(sharc.Name, "meta.xml"), meta.ToXml());
            // Export sources
            foreach (var src in sharc.Sources)
                File.WriteAllText(Path.Combine(sharc.Name, src.Name), src.GetCode());
        }

        public static SharcFile SourceFromFolder(string folder)
        {
            SharcFile sharc = new SharcFile();
            sharc.Name = new DirectoryInfo(folder).Name;

            // Load meta if exists
            SharcMetaData meta = new();
            string metaPath = Path.Combine(folder, "meta.xml");
            if (File.Exists(metaPath))
                meta = SharcMetaData.FromXml(File.ReadAllText(metaPath));

            // Load source files
            foreach (var file in Directory.GetFiles(folder))
            {
                if (!file.EndsWith(".xml"))
                {
                    sharc.Sources.Add(new SharcFile.ShaderSource(
                        File.ReadAllText(file),
                        Path.GetFileName(file)));
                }
            }

            // Load programs and variants via macro metas
            foreach (var program in meta.Programs)
            {
                var sharedShader = program.vertex_shader == program.fragment_shader;
                var vertSource = sharc.Sources.FirstOrDefault(x => x.Name == program.vertex_shader);
                var fragSource = sharc.Sources.FirstOrDefault(x => x.Name == program.fragment_shader);
                if (vertSource == null || fragSource == null)
                {
                    if (vertSource == null)
                        Console.WriteLine($"Failed to find shader source {vertSource}");
                    if (fragSource == null)
                        Console.WriteLine($"Failed to find shader source {fragSource}");
                    continue;
                }

                // Load macros. Use meta data macros if present instead
                List<SharcFile.VariationMacro> variationMacros = new();
                if (program.macros.Count > 0)
                {
                    foreach (var macro in program.macros)
                        variationMacros.Add(new SharcFile.VariationMacro()
                        {
                            Name = macro.name,
                            Values = macro.values,
                            Data = new byte[1],
                        });
                }
                else
                {
                    if (sharedShader)
                        variationMacros.AddRange(LoadMacros(vertSource.GetCode(), sharc));
                    else
                    {
                        variationMacros.AddRange(LoadMacros(vertSource.GetCode(), sharc));
                        variationMacros.AddRange(LoadMacros(fragSource.GetCode(), sharc));
                    }
                }

                sharc.Programs.Add(new SharcFile.ShaderProgram()
                {
                    VertexShaderIndex = sharc.Sources.FindIndex(x => x.Name == vertSource.Name),
                    FragmentShaderIndex = sharc.Sources.FindIndex(x => x.Name == fragSource.Name),
                    VariationMacros = variationMacros,
                    Name = program.name,
                });
            }
            return sharc;
        }

        private static readonly Regex DefineRegex = new Regex(
            @"^\s*#define\s+(?<name>\w+)\s*(?<value>\S.*?)?\s*//\s*@@\s*(?<metadata>.*)$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private static readonly Regex MetaRegex = new Regex(
            @"(?<key>\w+)=""(?<value>[^""]*)""",
            RegexOptions.IgnoreCase);

        static List<SharcFile.VariationMacro> LoadMacros(string code, SharcFile sharc)
        {
            code = ProcessIncludes(code, sharc);

            List<SharcFile.VariationMacro> variationMacros = new();

            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                var match = DefineRegex.Match(line);
                if (!match.Success)
                    continue;

                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value.Trim();
                string metadata = match.Groups["metadata"].Value;

                SharcFile.VariationMacro variant = new();
                variant.Name = name;
                variant.Data = new byte[1];
                variationMacros.Add(variant);

                // Parse key="value" pairs
                foreach (Match m in MetaRegex.Matches(metadata))
                {
                    string key = m.Groups["key"].Value.ToLower();
                    string v = m.Groups["value"].Value;

                    switch (key)
                    {
                        case "choice":
                            if (v == "bool")
                                variant.Values = new List<string>() { "0", "1" };
                            else
                                throw new Exception($"Choice {v} not supported yet!");
                            break;
                        case "default":
                            break;
                    }
                }
            }

            return variationMacros;
        }

        public static SharcfbFile ToBinary(SharcFile sharc)
        {
            SharcfbFile sharcfb = new();
            sharcfb.Name = sharc.Name;
            sharcfb.IsSwitch = true;

            foreach (var prog in sharc.Programs)
            {
                var sharcfbProj = new SharcfbFile.ShaderProgram();
                sharcfbProj.Name = prog.Name;
                sharcfb.Programs.Add(sharcfbProj);
                sharcfbProj.BaseIndex = sharcfb.Variations.Count;

                string vertexShader = sharc.Sources[prog.VertexShaderIndex].GetCode();
                string fragShader = sharc.Sources[prog.FragmentShaderIndex].GetCode();
                vertexShader = ProcessIncludes(vertexShader, sharc);
                fragShader = ProcessIncludes(fragShader, sharc);
                vertexShader = CompileSource(vertexShader, 0, 0);
                fragShader = CompileSource(fragShader, 1, 0);

                string[] sources = new[]
                {
                    vertexShader, fragShader
                };
                List<SharcFile.MacroDefine>[] defines = new[]
                {
                    prog.VertexMacros , prog.FragmentMacros
                };
                SharcfbFile.ShaderType[] stageTypes = new[]
                {
                    SharcfbFile.ShaderType.Vertex,
                    SharcfbFile.ShaderType.Pixel,
                };

                foreach (var var in prog.VariationMacros)
                    sharcfbProj.VariationMacros.Add(var);

                ProgressBar progress = new();

                // Compile all combinations
                var allVariationCombinations = prog.GetAllVariationCombinations().ToList();
                foreach (var macros in allVariationCombinations)
                {
                    // One per stage
                    for (int i = 0; i < stageTypes.Length; i++)
                    {
                        progress.Report((double)sharcfb.Variations.Count / (allVariationCombinations.Count * 2));
                        var variation = new SharcfbFile.ShaderVariation();
                        foreach (var def in defines[i])
                            macros[def.Name] = def.Value;

                        var binary = UAMShaderCompiler.CompileByText(sources[i], i + UAMShaderCompiler.Kind.vert, macros);
                        if (binary.ShaderCode == null || binary.Control == null)
                            continue;

                        variation.Type = stageTypes[i];
                        variation.ControlShader = binary.Control;
                        variation.ByteCode = binary.ShaderCode;
                        sharcfb.Variations.Add(variation);

                        if (variation.Type == SharcfbFile.ShaderType.Vertex) // vertex
                        {
                            foreach (var attr in binary.Symbols.inputs.Where(x => x.location != -1))
                            {
                                variation.Attributes.Add(new SharcfbFile.Symbol()
                                {
                                    Name = attr.name,
                                    Location = attr.location,
                                });
                            }
                        }

                        // Uniform blocks
                        foreach (var block in binary.Symbols.uniformBlocks.Where(x => x.binding != 0))
                        {
                            if (block.uniforms == null)
                                continue;

                            variation.UniformBlocks.Add(new SharcfbFile.SymbolUniformBlock()
                            {
                                Name = block.name,
                                Size = (uint)block.size,
                                Location = (int)(block.index),
                            });
                            // Uniforms
                            foreach (var uniform in block.uniforms)
                            {
                                variation.Uniforms.Add(new SharcfbFile.Symbol()
                                {
                                    Name = uniform.name,
                                    Location = uniform.offset,
                                });
                            }
                        }

                        // Samplers
                        foreach (var sampler in binary.Symbols.samplers.Where(x => x.location != -1))
                        {
                            variation.Samplers.Add(new SharcfbFile.Symbol()
                            {
                                Name = sampler.name,
                                Location = sampler.location,
                            });
                        }
                    }
                }
                progress.Dispose();
            }
            return sharcfb;
        }

        static string CompileSource(string text, int type, int location)
        {
            // Remove all version data
            string pattern = @"#version.*";
            text = Regex.Replace(text, pattern, string.Empty);

            int platform = 0;

            string[] targets = new[]
            {
                // NVN
                "#define AGL_TARGET_NVN",
                // PC
                "#define AGL_TARGET_GL",
                // Wii U
                "#define AGL_TARGET_GX2",
            };

            string[] macros = new[]
            {
                // PC
                "#version 400\n" +
                "#extension GL_ARB_texture_cube_map_array : enable\n" +
                "#extension GL_ARB_shading_language_420pack : enable\n" +
                "#define AGL_VARYING out\n",
                // GX2
                "#version 330\n" +
                "#extension GL_ARB_texture_cube_map_array : enable\n"
            };

            string[] stages = new[]
            {
                "#define AGL_VERTEX_SHADER",
                "#define AGL_FRAGMENT_SHADER",
                "#define AGL_GEOMETRY_SHADER"
            };

            StringBuilder sb = new();
            sb.AppendLine(macros[platform]);
            sb.AppendLine("// ----- These macros are auto defined by AGL.-----");
            sb.AppendLine(stages[type]);
            sb.AppendLine(targets[platform]);
            sb.AppendLine("// ------------------------------------------------");
            if (location != -1)
                sb.Append(ConvertLooseUniformsToBlock(text, "RegisterUBO", location));

            return sb.ToString();
        }

        private static readonly Regex IncludeRegex = new Regex(@"#include\s+""(.+?)""", RegexOptions.Compiled);

        public static string ProcessIncludes(string shaderSource, SharcFile sharc)
        {
            StringBuilder processedShader = new StringBuilder();

            foreach (string line in shaderSource.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                Match match = IncludeRegex.Match(line);
                if (match.Success)
                {
                    bool foundSource = false;

                    string includeFile = match.Groups[1].Value;
                    foreach (var src in sharc.Sources)
                    {
                        if (includeFile.Contains(src.Name))
                        {
                            processedShader.Append(ProcessIncludes(src.GetCode(), sharc));
                            foundSource = true;
                        }
                    }

                    if (!foundSource)
                    {
                        throw new FileNotFoundException($"Included file not found: {includeFile}");
                    }
                }
                else
                {
                    processedShader.AppendLine(line);
                }
            }

            return processedShader.ToString();
        }

        // Convert uniforms into block data
        private static readonly Regex UniformRegex = new Regex(
            @"^\s*uniform\s+(?<type>(?!sampler)\w+)\s+(?<name>\w+)\s*;\s*(?<comment>//.*)?$",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public static string ConvertLooseUniformsToBlock(
            string shaderCode,
            string blockName = "RegisterUBO",
            int binding = 0)
        {
            var matches = UniformRegex.Matches(shaderCode);

            if (matches.Count == 0)
                return shaderCode;

            var blockLines = new List<string>();
            List<string> uniforms = new();

            foreach (Match match in matches)
            {
                string type = match.Groups["type"].Value;
                string name = match.Groups["name"].Value;
                string comment = match.Groups["comment"].Value;

                if (uniforms.Contains(name))
                    continue;

                uniforms.Add(name);

                blockLines.Add(
                    $"    {type}\t{name};" +
                    $"{(string.IsNullOrWhiteSpace(comment) ? "" : "\t" + comment)}");
            }

            string block =
                $@"layout(std140, binding = {binding}) uniform {blockName}
            {{
            {string.Join(Environment.NewLine, blockLines)}
            }};";

            // Remove loose uniforms
            string result = UniformRegex.Replace(shaderCode, "").Trim();

            // Insert block near top
            return block + Environment.NewLine + Environment.NewLine + result;
        }
    }
}
