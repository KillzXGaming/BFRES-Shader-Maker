using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ShaderLibrary;
using System.ComponentModel;

namespace ShaderBuilderTool
{
    /// <summary>
    /// A variation file to configure shader options for a given material.
    /// This determines what program settings to compile as to make a material shader.
    /// </summary>
    public class VariationFile
    {
        public List<Option> StaticOptions { get; set; } = new List<Option>();
        public List<Option> DynamicOptions { get; set; } = new List<Option>();

        public VariationFile() { }

        public void Add(GLSLParser glsl)
        {
            // Make a default variation from the glsl code
            foreach (var option in glsl.StaticOptions)
            {
                if (StaticOptions.Any(x => x.Name == option.Key))
                    continue;

                this.StaticOptions.Add(new Option()
                {
                    Name = option.Key,
                    Desc = option.Value.Description,
                    Value = option.Value.Branch ? option.Value.Choices :
                    new List<string>() { option.Value.DefaultChoice },
                });
            }
            foreach (var option in glsl.DynamicOptions)
            {
                if (DynamicOptions.Any(x => x.Name == option.Key))
                    continue;

                this.DynamicOptions.Add(new Option()
                {
                    Name = option.Key,
                    Desc = option.Value.Description,
                    Value = option.Value.Branch ? option.Value.Choices :
                    new List<string>() { option.Value.DefaultChoice },
                });
            }
        }

        public string ToXml()
        {
            using (var writer = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(typeof(VariationFile));
                serializer.Serialize(writer, this);
                writer.Flush();

                return writer.ToString();
            }
        }

        public static VariationFile FromXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(VariationFile));
            using (var reader = new System.IO.StringReader(xml))
            {
                return (VariationFile)serializer.Deserialize(reader);
            }
        }

        public class Option
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string Desc { get; set; }
            [XmlAttribute]
            public List<string> Value { get; set; } = new List<string>();
        }
    }
}
