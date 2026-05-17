using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

#nullable enable
namespace BlenderBfresConverter;

public class BlenderXml
{
    [XmlRoot("model")]
    public class Model
    {
        [XmlArray("dir_lights")]
        [XmlArrayItem("dir_light")]
        public List<DirectionalLight> DirectionalLights { get; set; } = new List<DirectionalLight>();

        [XmlArray("point_lights")]
        [XmlArrayItem("point_light")]
        public List<PointLight> PointLights { get; set; } = new List<PointLight>();

        [XmlArray("spot_lights")]
        [XmlArrayItem("spot_light")]
        public List<SpotLight> SpotLights { get; set; } = new List<SpotLight>();

        [XmlArray("bones")]
        [XmlArrayItem("bone")]
        public List<Bone> Bones { get; set; } = new List<Bone>();

        [XmlArray("materials")]
        [XmlArrayItem("material")]
        public List<Material> Materials { get; set; } = new List<Material>();

        [XmlArray("images")]
        [XmlArrayItem("image")]
        public List<Image> Images { get; set; } = new List<Image>();

        [XmlArray("meshes")]
        [XmlArrayItem("mesh")]
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();

        [XmlArray("materialAnimations")]
        [XmlArrayItem("materialAnimation")]
        public List<MaterialAnimation> MaterialAnimations { get; set; } = new List<MaterialAnimation>();

        public static Model Load(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
                return (Model)new XmlSerializer(typeof(Model)).Deserialize(fileStream);
        }

        public void Save(string filePath)
        {
            using (FileStream fileStream = File.Create(filePath))
                new XmlSerializer(typeof(Model)).Serialize(fileStream, this);
        }
    }

    public class DirectionalLight
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("dir")]
        public string dir { get; set; }

        [XmlAttribute("color")]
        public string color { get; set; }

        [XmlAttribute("intensity")]
        public float intensity { get; set; }
    }

    public class SpotLight
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("dir")]
        public string dir { get; set; }

        [XmlAttribute("color")]
        public string color { get; set; }

        [XmlAttribute("intensity")]
        public float intensity { get; set; }

        [XmlAttribute("radius")]
        public float radius { get; set; }
    }

    public class PointLight
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("color")]
        public string color { get; set; }

        [XmlAttribute("intensity")]
        public float intensity { get; set; }

        [XmlAttribute("radius")]
        public float radius { get; set; }
    }

    public class Bone
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("pos")]
        public string pos { get; set; }

        [XmlAttribute("scale")]
        public string scale { get; set; }

        [XmlAttribute("rotate")]
        public string rotate { get; set; }

        [XmlAttribute("parentIndex")]
        public int parentIndex { get; set; }
    }

    public class MaterialAnimation
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("material")]
        public string Material { get; set; }

        [XmlAttribute("frame_start")]
        public float FrameStart { get; set; }

        [XmlAttribute("frame_end")]
        public float FrameEnd { get; set; }

        [XmlAttribute("enableLoop")]
        public bool Loop { get; set; }

        [XmlElement("track")]
        public List<AnimationTrack> Tracks { get; set; } = new List<AnimationTrack>();
    }

    public class AnimationTrack
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("component")]
        public int Component { get; set; }

        [XmlElement("keyframe")]
        public List<KeyFrame> KeyFrames { get; set; } = new List<KeyFrame>();
    }

    public class KeyFrame
    {
        [XmlAttribute("frame")]
        public float Frame { get; set; }

        [XmlAttribute("value")]
        public float Value { get; set; }

        [XmlAttribute("in_slope")]
        public float InSlope { get; set; }

        [XmlAttribute("out_slope")]
        public float OutSlope { get; set; }
    }

    public class Mesh
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("skin_count")]
        public int SkinCount { get; set; }

        [XmlElement("bones")]
        public string Bones { get; set; }

        [XmlArray("attributes")]
        [XmlArrayItem("vertex_attribute")]
        public VertexAttribute[] Attributes { get; set; }

        [XmlArray("polygons")]
        [XmlArrayItem("polygon")]
        public Polygon[] Polygons { get; set; }
    }

    public class VertexAttribute
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("channel")]
        public int Channel { get; set; }

        [XmlAttribute("vertex_count")]
        public int VertexCount { get; set; }

        [XmlAttribute("element_count")]
        public int ElementCount { get; set; }

        [XmlElement("values")]
        public string ValuesRaw { get; set; }

        [XmlIgnore]
        public float[] Values => Array.ConvertAll(
            ValuesRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            float.Parse);
    }

    public class Polygon
    {
        [XmlAttribute("material")]
        public string Material { get; set; }

        [XmlElement("indices")]
        public string IndicesRaw { get; set; }

        [XmlIgnore]
        public int[] Indices => Array.ConvertAll(
                IndicesRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                int.Parse);
    }

    public class Image
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("format")]
        public string Format { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("sampler_type")]
        public string SamplerType { get; set; }

        [XmlAttribute("is_srgb")]
        public bool IsSrgb { get; set; }
    }

    public class Material
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("caustic_light")]
        public string CausticLight { get; set; } = "NONE";

        [XmlArray("macros")]
        [XmlArrayItem("macro")]
        public Macro[] Macros { get; set; }

        [XmlArray("samplers")]
        [XmlArrayItem("sampler")]
        public Sampler[] Samplers { get; set; }

        [XmlArray("uniformBlocks")]
        [XmlArrayItem("uniformBlock")]
        public UniformBlock[] UniformBlocks { get; set; }

        [XmlArray("renderInfos")]
        [XmlArrayItem("renderInfo")]
        public RenderInfo[] RenderInfos { get; set; }
    }

    public class Macro
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    public class Sampler
    {
        [XmlAttribute]
        public int anisoRatio;

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("texture")]
        public string Texture { get; set; }

        [XmlAttribute]
        public string minFilter { get; set; }

        [XmlAttribute]
        public string magFilter { get; set; }

        [XmlAttribute]
        public string wrapX { get; set; }

        [XmlAttribute]
        public string wrapY { get; set; }

        [XmlAttribute]
        public float minLod { get; set; }

        [XmlAttribute]
        public float maxLod { get; set; } = 13f;

        [XmlAttribute]
        public float lodBias { get; set; }
    }

    public class UniformBlock
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("uniform")]
        public List<Uniform> Uniforms { get; set; } = new List<Uniform>();
    }

    public class Uniform
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlElement("texSRT")]
        public TexSRT TexSrt { get; set; }
    }

    public class TexSRT
    {
        [XmlAttribute("mode")]
        public string Mode { get; set; }

        [XmlAttribute("rotate")]
        public float Rotate { get; set; }

        [XmlElement("scale")]
        public Vec2 Scale { get; set; }

        [XmlElement("translate")]
        public Vec2 Translate { get; set; }
    }

    public class Vec2
    {
        [XmlAttribute("x")]
        public float X { get; set; }

        [XmlAttribute("y")]
        public float Y { get; set; }
    }

    public class RenderInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
