# BFRES-Shader-Maker
A tool for building bfsha/bnsh shader binaries from a given bfres and glsl code.
For making brand new bfres directly from blender, there will be a future update with blender bfres xml.

## Requirements
- devkitPro https://github.com/devkitPro/installer/releases
- net8.0 runtime https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Usage
Edit settings .json to your needed settings. Default one works for MK8 shaders.
For SMO, download the releases zip for SMO which will have SMO settings.

Add your own .bfres files to the "Bfres" folder. 
When you run the tool, it will process the bfres with the updated shaders to the output folder.

## How it works (advanced)
A bfsha is made of common shader elements.
- Samplers
- Uniform blocks/uniforms
- Vertex attributes

To map these for original naming and info, I add in glsl meta data comments.

Attribute example:
`layout (location = 0) in vec4 vPosition; //@ id="_p0"`

Sampler example:
`layout (binding = 0) uniform sampler2D albedo_texture; //@ id="_a0"`

Blocks can include size, default param data, and other info.
```
layout (binding = 4, std140) uniform MyMaterial //@ id="game_material" type="material"
{
    vec4 shadow_color; //@ id="shadow_color" default_value="0 0 0 0"
    vec4 ao_color; //@ id="ao_color" default_value="0 0 0 0"
    vec4 lighting; //@ id="lighting" default_value="1 1 1 1"
    vec4 lighting_specular; //@ id="lighting_specular" default_value="1 1 1 1"
    vec4 light_prepass_param; //@ id="light_prepass_param" default_value="1 1 1 1"
    vec4 exposure; //@ id="exposure" default_value="1 1 1 1"
}material;
```

Make sure the `type` property is one of these if it is a material, skeleton, or shape block, else no type property is needed.
- skeleton
- material
- shape

For these shaders to work on a game, make sure the block, samplers, and attribute data is accurately replicated. 

Shader option example:
`#define game_renderstate 0 //@ choices="0 1 2 3"`

A shader option determines what code to set during compile.
These values get set in the bfres material so it knows what compiled shader to use.

To generate a list of existing macros and meta data, run the shader tool with arguments:
`ShaderBuilder -e Shader.bfsha`

Better extraction options will be added in a future update to help make new game shaders.

