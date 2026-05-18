using ShaderLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
