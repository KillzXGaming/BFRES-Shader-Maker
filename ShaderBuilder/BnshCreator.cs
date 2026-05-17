using ShaderLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderBuilderTool.Convert
{
    public class BnshCreator
    {
        public class Args
        {
            public List<VariantArg> Variants = new();
            // Versions:
            // MK8, ARMS, SMM2, BOTW 2.1.2
            // Pokemon Sword/Shield 2.1.11
            // SMO, SP2, SMP, MPS, MTA, CTTT 2.1.5
            // SP3 Pokemon Scarlet/Violet 2.1.12
            // TOTK, SMW 2.2.1
            // Ounce, SMPJ 2.3.1
            public ushort VersionMajor = 2;
            public byte VersionMinor = 1;
            public byte VersionMicro = 2;

            public string Name = "dummy";

            public ushort ApiType = 4; // Always 4
            public ushort ApiVersion = 0; // 200 for Ounce

            public uint CompilerVersion = 131330;
            public ulong Unknown = 4785147618590735;
        }

        public class VariantArg
        {
            public string VertexShader;
            public string FragmentShader;
            public string GeometryShader;
            public string ComputeShader;
            public string TessellationControl;
            public string TessellationEval;

            public uint ShaderVersionMajor = 1;
            public uint ShaderVersionMinor = 9;
            public uint Flag = 2;
        }

        public class VariantOutput
        {
            public BnshFile.ShaderVariation Variation;

            public UAMShaderCompiler.ShaderOutput CompilerVertex;
            public UAMShaderCompiler.ShaderOutput CompilerFragment;
            public UAMShaderCompiler.ShaderOutput CompilerGeometry;
            public UAMShaderCompiler.ShaderOutput CompilerCompute;
            public UAMShaderCompiler.ShaderOutput CompilerTessE;
            public UAMShaderCompiler.ShaderOutput CompilerTessC;
        }

        public static BnshFile Create(Args args)
        {
            BnshFile bnsh = new();
            bnsh.BinHeader.VersionMajor = args.VersionMajor;
            bnsh.BinHeader.VersionMicro = args.VersionMicro;
            bnsh.BinHeader.VersionMinor = args.VersionMinor;
            bnsh.Header.ApiType = args.ApiType;
            bnsh.Header.ApiVersion = args.ApiVersion;
            bnsh.Name = args.Name;
            bnsh.Header.CompilerVersion = args.CompilerVersion;
            bnsh.Header.Unknown2 = args.Unknown;

            foreach (var variant in args.Variants)
                bnsh.Variations.Add(CreateVariation(variant).Variation);
            return bnsh;
        }

        public static VariantOutput CreateVariation(VariantArg args)
        {
            VariantOutput output = new VariantOutput();

            BnshFile.ShaderVariation shaderVariation = new BnshFile.ShaderVariation();
            shaderVariation.BinaryProgram = new BnshFile.BnshShaderProgram(); 
            shaderVariation.BinaryProgram.header.Flags = (byte)args.Flag;
            // Compile stages that are used
            // Store compile info for symbol data
            output.CompilerVertex = CompileStage(shaderVariation, args.VertexShader, args, UAMShaderCompiler.Kind.vert);
            output.CompilerFragment = CompileStage(shaderVariation, args.FragmentShader, args, UAMShaderCompiler.Kind.frag);
            output.CompilerGeometry = CompileStage(shaderVariation, args.GeometryShader, args, UAMShaderCompiler.Kind.geom);
            output.CompilerCompute = CompileStage(shaderVariation, args.ComputeShader, args, UAMShaderCompiler.Kind.comp);
            output.CompilerTessC = CompileStage(shaderVariation, args.TessellationControl, args, UAMShaderCompiler.Kind.tesc);
            output.CompilerTessE = CompileStage(shaderVariation, args.TessellationEval, args, UAMShaderCompiler.Kind.tese);
            output.Variation = shaderVariation;
            return output;
        }

        public static UAMShaderCompiler.ShaderOutput CompileStage(BnshFile.ShaderVariation variation,
            string code, VariantArg args, UAMShaderCompiler.Kind kind)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var compiled = UAMShaderCompiler.CompileByText(code, kind);
            if (compiled.ShaderCode.Length == 0) // Failed, return
                return null;

            ControlShader controlCode = new ControlShader(compiled.Control);
            controlCode.MajorVer = args.ShaderVersionMajor;
            controlCode.MinorVer = args.ShaderVersionMinor;

            var binShaderCode = new BnshFile.ShaderCode()
            {
                ControlCode = controlCode.ToBytes(),
                ByteCode = compiled.ShaderCode,
            };
            var reflection = SetReflection(compiled.Symbols);
            switch (kind)
            {
                case UAMShaderCompiler.Kind.vert:
                    variation.BinaryProgram.VertexShader = binShaderCode;
                    variation.BinaryProgram.VertexShaderReflection = reflection;
                    break;
                case UAMShaderCompiler.Kind.frag:
                    variation.BinaryProgram.FragmentShader = binShaderCode;
                    variation.BinaryProgram.FragmentShaderReflection = reflection;
                    break;
                case UAMShaderCompiler.Kind.geom:
                    variation.BinaryProgram.GeometryShader = binShaderCode;
                    variation.BinaryProgram.GeometryShaderReflection = reflection;
                    break;
                case UAMShaderCompiler.Kind.comp:
                    variation.BinaryProgram.ComputeShader = binShaderCode;
                    variation.BinaryProgram.ComputeShaderReflection = reflection;
                    break;
                case UAMShaderCompiler.Kind.tesc:
                    variation.BinaryProgram.TessellationControlShader = binShaderCode;
                    variation.BinaryProgram.TessellationControlShaderReflection = reflection;
                    break;
                case UAMShaderCompiler.Kind.tese:
                    variation.BinaryProgram.TessellationEvalShader = binShaderCode;
                    variation.BinaryProgram.TessellationEvalShaderReflection = reflection;
                    break;
            }
            return compiled;
        }

        // Prepares location mapping and reflection data via shader symbols
        static BnshFile.ShaderReflectionData SetReflection(UAMShaderCompiler.ShaderSymbolData symbols)
        {
            BnshFile.ShaderReflectionData reflect = new();
            foreach (var sampler in symbols.samplers.Where(x => x.location != -1))
                reflect.Samplers.TryAdd(sampler.name, new ResUint32((uint)sampler.location));
            foreach (var input in symbols.inputs.Where(x => x.location != -1))
                reflect.Inputs.Add(input.name, new ResUint32((uint)input.location));
            foreach (var output in symbols.outputs.Where(x => x.location != -1))
                reflect.Outputs.Add(output.name, new ResUint32((uint)output.location));
            foreach (var buffer in symbols.uniformBlocks)
                reflect.UniformBuffers.Add(buffer.name, new ResUint32((uint)(buffer.binding - 1)));
            foreach (var buffer in symbols.storageBlocks)
                reflect.StorageBuffers.Add(buffer.name, new ResUint32((uint)buffer.binding));
            reflect.UpdateSlots();
            return reflect; 
        }
    }
}
