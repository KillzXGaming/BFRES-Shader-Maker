using ShaderLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlenderBfresConverter.BlenderXml;

namespace ShaderBuilder
{
    public class ShaderMetadata
    {
        public static void DumpMetaData(ShaderModel shader, string folder)
        {
            StreamWriter wr = new StreamWriter(Path.Combine(folder, $"{shader.Name}_options.txt"));
            wr.WriteLine("//-----------------------------------");
            wr.WriteLine("//Static Options");
            wr.WriteLine("//-----------------------------------");
            foreach (ShaderOption option in shader.StaticOptions.Values)
                WriteOption(option, false);
            wr.WriteLine("//-----------------------------------");
            wr.WriteLine("//Dynamic Options");
            wr.WriteLine("//-----------------------------------");
            foreach (ShaderOption option in shader.DynamicOptions.Values)
                WriteOption(option, true);
            wr.Close();

            void WriteOption(ShaderOption option, bool dynamic)
            {
                string value = "0";
                // Option is likely a boolean but we cannot be too certain
                // Set it as boolean and the user can change it later if needed
                if (string.Join(" ", option.Choices.Keys) == "0 1")
                    value = option.DefaultChoice == "1" ? "true" : "false";
                else
                {
                    // Int type, ensure it can be parsed
                    int result;
                    if (int.TryParse(option.DefaultChoice, out result))
                        value = result.ToString();
                }
                bool isBoolean = new string[] { "true", "false" }.Contains(value);
                string macro = $"#define {option.Name} {value} //@";
                // Toggle branch type
                if (dynamic)
                    macro += " branch=\"dynamic\"";
                // Add choice list
                if (!isBoolean)
                    macro = $"{macro} choices=\"{string.Join(" ", option.Choices.Keys)}\"";
                wr.WriteLine(macro);
            }

            wr = new StreamWriter(Path.Combine(folder, $"{shader.Name}_blocks.txt"));
            foreach (var block in shader.UniformBlocks)
            {
                string comment = $"//@ id=\"{block.Key}\" size=\"{block.Value.Size}\"";
                if (block.Value.Type == BfshaUniformBlock.BlockType.Shape) comment += " type=\"shape\"";
                if (block.Value.Type == BfshaUniformBlock.BlockType.Material) comment += " type=\"material\"";
                if (block.Value.Type == BfshaUniformBlock.BlockType.Option) comment += " type=\"skeleton\"";
                if (block.Value.Type == BfshaUniformBlock.BlockType.Num) comment += " type=\"option\"";

                Console.WriteLine((int)block.Value.Type + " " + block.Key);

                wr.WriteLine($"layout(std140, binding = {block.Value.Index}) uniform {block.Key} {comment}");
                wr.WriteLine("{");
                wr.WriteLine("  vec4 data[4096];");
                wr.WriteLine("}" + block.Key.ToLower() + ";");
            }
            wr.Close();


            wr = new StreamWriter(Path.Combine(folder, $"{shader.Name}_attributes.txt"));
            foreach (var attr in shader.Attributes)
                wr.WriteLine($"layout (location = {attr.Value.Index}) in vec3 a{attr.Key}; //@ id=\"{attr.Key}\"");
            wr.Close();

            wr = new StreamWriter(Path.Combine(folder, $"{shader.Name}_samplers.txt"));
            foreach (var attr in shader.Samplers)
                wr.WriteLine($"layout (binding = {attr.Value.Index}) uniform sampler2D {attr.Key}; //@ id=\"{attr.Key}\"");
            wr.Close();
        }
    }
}
