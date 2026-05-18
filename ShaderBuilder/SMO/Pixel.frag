#version 450 core

#include "UniformBlocks.glsl"

#if (BLENDER_RENDER == 0)
    
layout (binding = 0) uniform sampler2D cTextureBaseColor; //@ id="_a0" label="Base Color Texture" group="Base Color"
layout (binding = 1) uniform sampler2D cTextureNormal;  //@ id="_n0" label="Normal Map Texture" group="Normals"
layout (binding = 2) uniform sampler2D cTextureUniform0; //@ id="_u0" label="Uniform 0 Texture" group="Uniform 0"
layout (binding = 3) uniform sampler2D cTextureUniform1; //@ id="_u1" label="Uniform 1 Texture" group="Uniform 1"
layout (binding = 4) uniform sampler2D cTextureUniform2; //@ id="_u2" label="Uniform 2 Texture" group="Uniform 2"
layout (binding = 5) uniform sampler2D cTextureUniform3; //@ id="_u3" label="Uniform 3 Texture" group="Uniform 3"
layout (binding = 6) uniform samplerCube cTexCubeMapRoughness; //@ id="_r0"

layout (binding = 9) uniform samplerCube cTextureMaterialLightCube; //@ id="_m0"
layout (binding = 22) uniform sampler2D cTextureMaterialLightSphere; //@ id="_m1"
layout (binding = 23) uniform sampler2D cExposureTexture; // bindless 
layout (binding = 24) uniform sampler2D cTextureProcTexture2D; // bindless 
layout (binding = 25) uniform sampler3D cTextureProcTexture3D; // bindless 
layout (binding = 27) uniform sampler2D cTextureUniform4; //@ id="_u4" label="Uniform 4 Texture" group="Uniform 4"
layout (binding = 12) uniform sampler2D cFrameBufferTex; //@ id="_stcol0"
layout (binding = 7) uniform sampler2D cTextureLinearDepth; //@ id="linear_depth"

layout (location = 0) in vec4 fNormalsDepth; //Normals xyz, depth
layout (location = 1) in vec4 fTangents; //Tangents xyz. bitagents x
layout (location = 2) in vec4 fBitangents; //Bitangents yz
layout (location = 3) in vec4 fViewPos; //Screen coords?, then view pos xy
layout (location = 4) in vec4 fLightColorVPosZ; //Light RGB, view pos z
layout (location = 5) in vec4 fTexCoords01; //Tex coords 0 -> 1 xy
layout (location = 6) in vec4 fVertexColor;
layout (location = 7) in vec4 fTexCoords23;
layout (location = 8) in vec4 fIrradianceVertex;
layout (location = 9) in vec2 fSphereCoords;

layout (location = 11) in vec4 fVertexPos; 
layout (location = 12) in vec4 fPreviousPos;
layout (location = 13) in noperspective vec4 fPerspDiv;

layout (location = 0) out vec4 oLightBuf;
layout (location = 1) out vec4 oWorldNrm;
layout (location = 2) out vec4 oNormalizedLinearDepth;
layout (location = 3) out vec4 oBaseColor;
layout (location = 4) out vec4 oMotionVec;

#endif


//Variables for setting blend for outputs later
vec4 BLEND0_OUTPUT= vec4(0.0);
vec4 BLEND1_OUTPUT= vec4(0.0);
vec4 BLEND2_OUTPUT= vec4(0.0);
vec4 BLEND3_OUTPUT= vec4(0.0);
vec4 BLEND4_OUTPUT= vec4(0.0);
vec4 BLEND5_OUTPUT= vec4(0.0);

vec4 fIndirectCoords = vec4(0.0);

const float PI = 3.14159265359;

vec3 saturate(vec3 v)
{
    return clamp(v, 0.0, 1.0);
}

float saturate(float v)
{
    return clamp(v, 0.0, 1.0);
}

vec2 GetScreenCoordinates()
{
	vec2 screenCoord = fPerspDiv.xy * 0.5 + 0.5;
    screenCoord.y = 1.0 - screenCoord.y;
    return screenCoord;
}

struct Light
{
    vec3 I; //eye
    vec3 N; //normal
    vec3 V; //view
    vec3 H; //half
    vec3 L; //light
    vec3 R; //reflect
    float NV; //dot normal view
};

Light SetupLight(vec3 N, vec3 view_pos)
{
    Light light;

    vec3 dir = -normalize(view_pos);
    vec3 view_normal = normalize(N * mat3(mdlEnvView.cView));
    vec3 cubemap_coords = reflect(-dir, view_normal.rgb) * mat3(mdlEnvView.cViewInv);

    light.N = N;
    light.I = vec3(0,0,1) * mat3(mdlEnvView.cView);
    light.V = normalize(light.I); // view
    light.L = normalize(mdlEnvView.Dir.xyz ); // Light
    light.H = normalize(light.V + light.L); // half angle
    light.R = vec3(cubemap_coords.x, cubemap_coords.y, -cubemap_coords.z); // reflection
    light.NV = saturate(dot(light.N, light.V));

    return light;
}

vec4 EncodeBaseColor(vec3 baseColor, float roughness, float metalness, vec3 normal)
{
    float encoded = float(uint(int(uint(max(trunc(roughness * 15.0), 0.0))) << 4 | 
                               int(uint(max(trunc(metalness * 15.0), 0.0)))) | 
                               uint(step(0.0, normal.z))) * 0.00392156886;
    return vec4(baseColor, encoded);
}

vec4 DecodeCubemap(samplerCube cube, vec3 n, float lod)
{
    vec4 tex = textureLod(cube, n, lod);

    float scale = pow(tex.a, hdr.Power) * hdr.Range;
    return vec4(tex.rgb * scale, scale);
}

vec2 CalcSphereCoords(vec3 n)
{
    //view normal
    vec3 view_n = (normalize(n.xyz) * mat3(mdlEnvView.cView)).xyz;
    //center the uvs
    return view_n.xy * vec2(0.5) + vec2(0.5,-0.5);
}

//Blend have different components for some reason 
vec4 GetCompBlend(vec4 v, int comp_mask)
{
    if      (comp_mask == 10)  return v.rgba;
    else if (comp_mask == 20)  return v.rrrr;
    else if (comp_mask == 30)  return v.gggg;
    else if (comp_mask == 40)  return v.bbbb;
    else if (comp_mask == 50)  return v.aaaa;
    else if (comp_mask == 11)  return 1.0 - v.rgba;
    else if (comp_mask == 21)  return 1.0 - v.rrrr;
    else if (comp_mask == 31)  return 1.0 - v.gggg;
    else if (comp_mask == 41)  return 1.0 - v.bbbb;
    else if (comp_mask == 51)  return 1.0 - v.aaaa;
    return v.rgba;
}

vec4 GetComp(vec4 v, int comp_mask)
{
    if      (comp_mask == 10)  return v.rgba;
    else if (comp_mask == 30)  return v.rrrr;
    else if (comp_mask == 40)  return v.gggg;
    else if (comp_mask == 50)  return v.bbbb;
    else if (comp_mask == 60)  return v.aaaa;
    else if (comp_mask == 70)  return clamp(1.0 - v.rrrr, 0.0, 1.0);
    else if (comp_mask == 80)  return clamp(1.0 - v.gggg, 0.0, 1.0);
    else if (comp_mask == 90)  return clamp(1.0 - v.bbbb, 0.0, 1.0);
    else if (comp_mask == 100) return clamp(1.0 - v.aaaa, 0.0, 1.0);

    return v.rgba;
}

vec2 SelectTexCoord(int mtx_select)
{
    if (mtx_select == 10)  //tex coord 0
        return fTexCoords01.xy;
    else if  (mtx_select == 11) //tex coord 1
        return fTexCoords01.zw;
    else if  (mtx_select == 12) //tex coord 2
        return fTexCoords23.xy;
    else if  (mtx_select == 13) //tex coord 3
        return fTexCoords23.zw;
    else if  (mtx_select == 20) //indirect coord 0
        return fIndirectCoords.xy;
    else if  (mtx_select == 21) //indirect coord 1
        return fIndirectCoords.zw;
    else if  (mtx_select == 30) //sphere mapping
        return fSphereCoords.xy;
    else //TODO 50 - 54 are proj texture types
        return fTexCoords01.xy;
}

float CalulateSphereLight()
{
    vec3 vertex_normal = normalize(fNormalsDepth.xyz);
    if (is_use_back_face_lighting)
         vertex_normal = 1.0 - vertex_normal;

    vec3 view_normal = normalize(vertex_normal.xyz * mat3(mdlEnvView.cView));
    vec3 view_pos = vec3(fViewPos.zw, fLightColorVPosZ.w);

    vec3 dir = normalize(view_pos);

    return clamp(fma(dir.z, -view_normal.z,
                    fma(dir.x, -view_normal.x, 
                    dir.y * -view_normal.y)), 0.0, 1.0);
}

vec4 CalculateSphereConstColor(int sphere_color_type, vec4 const_color, float sphere_rate_color)
{
    float cosTheta  = CalulateSphereLight();

    if (sphere_color_type == 1) //inverted fresnel effect
    {
        float amount = clamp(exp2(log2(cosTheta) * sphere_rate_color), 0.0, 1.0);
        return amount * const_color;
    }
    else if (sphere_color_type == 2) //fresnel effect
    {
        float amount = clamp(exp2(log2(1.0 - cosTheta) * sphere_rate_color), 0.0, 1.0);
        return amount * const_color;
    }
    else
        return const_color; //type 0 defaults to const color
}

vec4 CalculateUniform(sampler2D cTexture, int uv_selector, bool enable, vec4 mul_color,
    bool enable_mul_color, bool enable_mul_vtx_color, bool enable_roughness_lod, vec2 tex_bias)
{
    vec4 uniform_output = vec4(1.0);
    if (enable) //Todo third argument uses MdlEnvView.data[0x12A].x, a global LOD value
         uniform_output = texture(cTexture, SelectTexCoord(uv_selector) + tex_bias);
    if (enable_roughness_lod)
         uniform_output = textureLod(cTexture, SelectTexCoord(uv_selector), 0.0);
    if (enable_mul_color)
        uniform_output *= mul_color;
    if (enable_mul_vtx_color)
        uniform_output *= fVertexColor;

    return uniform_output;
}

vec4 CalculateBaseColor(vec2 tex_bias)
{
    vec4 basecolor_output = vec4(1.0);
    if (enable_base_color) //Todo third argument uses MdlEnvView.data[0x12A].x, a global LOD value
         basecolor_output = texture(cTextureBaseColor, SelectTexCoord(base_color_fuv_selector) + tex_bias);
    if (enable_base_color_mul_color)
        basecolor_output *= mat.base_color_mul_color;
    if (vtxcolor_type == VTX_COLOR_TYPE_DIFFUSE)
        basecolor_output.rgb *= fVertexColor.rgb;
    else if (vtxcolor_type == VTX_COLOR_TYPE_DIFFUSE_BLEND)
       basecolor_output.rgb *= (1.0 - fVertexColor.rgb * fVertexColor.rgb);

    return basecolor_output;
}

vec4 CalculateProcTexture2D()
{
    if (!enable_proc_texture_2d)
        return vec4(0.0);

    vec2 tex_coords =  SelectTexCoord(proc_texture_2d_fuv_selector);
    vec4 proc_tex = GetComp(texture(cTextureProcTexture2D, tex_coords), proc_texture_2d_component);

    if (enable_proc_texture_2d_mul_color)
        proc_tex *= mat.proc_texture_3d_mul_color.xyzw;

    if (enable_proc_texture_2d_mul_vtxcolor)
        proc_tex *= fVertexColor.xyzw;

    return proc_tex;
}

vec4 CalculateProcTexture3D()
{
    if (!enable_proc_texture_3d)
        return vec4(0.0);

    vec3 tex_coords_3d = fVertexPos.xyz * mat.proc_texture_3d_scale.xyz;

    if      (proc_texture_3d_fuv_offset == 60) tex_coords_3d += mat.const_color0.xyz;
    else if (proc_texture_3d_fuv_offset == 61) tex_coords_3d += mat.const_color1.xyz;
    else if (proc_texture_3d_fuv_offset == 62) tex_coords_3d += mat.const_color2.xyz;
    else if (proc_texture_3d_fuv_offset == 63) tex_coords_3d += mat.const_color3.xyz;

    vec4 proc_tex = GetComp(texture(cTextureProcTexture3D, tex_coords_3d), proc_texture_3d_component);

    if (enable_proc_texture_3d_mul_color)
        proc_tex *= mat.proc_texture_3d_mul_color.xyzw;

    if (enable_proc_texture_3d_mul_vtxcolor)
        proc_tex *= fVertexColor.xyzw;

    return proc_tex;
}

#define CALCULATE_UNIFORM(num, bias) \
    CalculateUniform(cTextureUniform##num, \
        uniform##num##_fuv_selector, \
        enable_uniform##num, \
        mat.uniform##num##_mul_color,  \
        enable_uniform##num##_mul_color, \
        enable_uniform##num##_mul_vtxcolor, \
        enable_uniform##num##_roughness_lod, bias) \

#define CALCULATE_CONST_COLOR(num) \
    CalculateSphereConstColor(sphere_const_color##num, \
                            mat.const_color##num, \
                            mat.sphere_rate_color##num) \

vec4 CalculateOutput(int flag)
{
    if (flag == 10)      return CalculateBaseColor(vec2(0.0));
    else if (flag == 15) return fVertexColor;
    else if (flag == 20) return texture(cTextureNormal, SelectTexCoord(normal_fuv_selector));
    else if (flag == 30) return vec4(fNormalsDepth.rgb, 0.0); //used in normals when no normal map present
    else if (flag == 50) return CALCULATE_UNIFORM(0, vec2(0.0));
    else if (flag == 51) return CALCULATE_UNIFORM(1, vec2(0.0));
    else if (flag == 52) return CALCULATE_UNIFORM(2, vec2(0.0));
    else if (flag == 53) return CALCULATE_UNIFORM(3, vec2(0.0));
    else if (flag == 54) return CALCULATE_UNIFORM(4, vec2(0.0));
    else if (flag == 60) return CALCULATE_CONST_COLOR(0); //sphere rate 0
    else if (flag == 61) return CALCULATE_CONST_COLOR(1); //sphere rate 1
    else if (flag == 62) return CALCULATE_CONST_COLOR(2); //sphere rate 2
    else if (flag == 63) return CALCULATE_CONST_COLOR(3); //sphere rate 3
    else if (flag == 70) return texture(cFrameBufferTex, GetScreenCoordinates());
    else if (flag == 71) return vec4(0.0); //cGBufferBaseColorTex TODO
    else if (flag == 72) return vec4(0.0); //cGBufferNormalTex TODO
    else if (flag == 73) return vec4(0.0); //gbuffer decode from base color TODO
    else if (flag == 74) return vec4(0.0); //gbuffer decode from base color TODO
    else if (flag == 78) return texture(cTextureLinearDepth, GetScreenCoordinates()); //linear depth TODO

    else if (flag == 80) return BLEND0_OUTPUT; //blend 0
    else if (flag == 81) return BLEND1_OUTPUT; //blend 1
    else if (flag == 82) return BLEND2_OUTPUT; //blend 2
    else if (flag == 83) return BLEND3_OUTPUT; //blend 3
    else if (flag == 84) return BLEND4_OUTPUT; //blend 4
    else if (flag == 85) return BLEND5_OUTPUT; //blend 5

    else if (flag == 110) return vec4(mat.const_single0); //Mat.const_single0
    else if (flag == 111) return vec4(mat.const_single1); //Mat.const_single1
    else if (flag == 112) return vec4(mat.const_single2); //Mat.const_single2
    else if (flag == 113) return vec4(mat.const_single3); //Mat.const_single3
    else if (flag == 115) return vec4(0.0); //constant
    else if (flag == 116) return vec4(1.0); //constant
    else if (flag == 140) return vec4(modelInfo.uv_offset, 0, 0); //ModelAdditionalInfo.uv_offset
    else if (flag == 160) return CalculateProcTexture2D(); //proc texture 2d
    else if (flag == 170) return CalculateProcTexture3D(); //proc texture 3d

    return vec4(0.0);
}

vec4 GetTransparentTexOutput(int flag, float bias_x, float bias_y) //Has seperate output which no texture maps used (unless type 20)
{
    vec2 bias = vec2(bias_x, bias_y);

    if (flag == 10)      return CalculateBaseColor(bias);
    else if (flag == 50) return CALCULATE_UNIFORM(0, bias);
    else if (flag == 51) return CALCULATE_UNIFORM(1, bias);
    else if (flag == 52) return CALCULATE_UNIFORM(2, bias);
    else if (flag == 53) return CALCULATE_UNIFORM(3, bias);
    else if (flag == 54) return CALCULATE_UNIFORM(4, bias);
    return vec4(0.0);
}

vec4 CalculateCofBlendOutput(int flag, int cof_map) //Has seperate output which no texture maps used (unless type 20)
{
    if (flag == 10)      return fVertexColor;
    else if (flag == 20) return CalculateOutput(cof_map); //cof_map

    else if (flag == 30) return vec4(mat.const_single0); //Mat.const_single0
    else if (flag == 31) return vec4(mat.const_single1); //Mat.const_single1
    else if (flag == 32) return vec4(mat.const_single2); //Mat.const_single2
    else if (flag == 33) return vec4(mat.const_single3); //Mat.const_single3

    else if (flag == 60) return CALCULATE_CONST_COLOR(0); //Mat.const_color0
    else if (flag == 61) return CALCULATE_CONST_COLOR(1); //Mat.const_color1
    else if (flag == 62) return CALCULATE_CONST_COLOR(2); //Mat.const_color2
    else if (flag == 63) return CALCULATE_CONST_COLOR(3); //Mat.const_color3

    else if (flag == 115) return vec4(0.0); //constant
    else if (flag == 116) return vec4(1.0); //constant

    return vec4(0.0);
}

float BlendCompareComponent(float src, float dst, float cof)
{
    float cmp = (src < 0.5) ? 0.0 : 1.0;
    float n = -2.0 * src + 2.0;
    float func = 2.0 * src * dst * cof + 2.0 * src - src * cof;
    return func + cmp * (-func - dst * cof * (-n) + cmp);
}

vec4 CalculateBlend(bool enable, int src_id, int dst_id, int cof_id, int cof_map,
       int src_ch, int dst_ch, int cof_ch, int ind, int equation)
{
    if (!enable)
        return vec4(0.0);

    vec4 src = GetCompBlend(CalculateOutput(src_id), src_ch);
    vec4 dst = GetCompBlend(CalculateOutput(dst_id), dst_ch);
    vec4 cof = GetCompBlend(CalculateCofBlendOutput(cof_id, cof_map), cof_ch);

    if      (equation == 0) return fma(src - dst, cof, dst);
    else if (equation == 1) return fma(dst, cof, src);
    else if (equation == 2) return dst * cof * src;
    else if (equation == 3) return fma(dst, 0.0 - cof, src); 
    else if (equation == 4) return dst + cof + src; 
    else if (equation == 5) return fma(dst * cof, 0.0 - src, dst * cof) + src;
    else if (equation == 6) //Compare func 
    {
        return vec4(
            BlendCompareComponent(src.x,dst.x,cof.x),
            BlendCompareComponent(src.y,dst.y,cof.y),
            BlendCompareComponent(src.z,dst.z,cof.z),
            BlendCompareComponent(src.w,dst.w,cof.w)
        );
    }
    else if (equation == 7) return (src + dst) * cof; 
    else if (equation == 8) return (src - dst) * cof; 

    return src;
}

#define CALCULATE_BLEND(num) \
    CalculateBlend(enable_blend##num,\
           blend##num##_src,\
           blend##num##_dst, \
           blend##num##_cof, \
           blend##num##_cof_map, \
           blend##num##_src_ch, \
           blend##num##_dst_ch, \
           blend##num##_cof_ch, \
           blend##num##_indirect_map, \
           blend##num##_eq) \

//Updates any blend variables referenced by flag

void TryCalculateReferencedBlend(int flag)
{
    if (     flag == 80) BLEND0_OUTPUT = CALCULATE_BLEND(0); //blend 0
    else if (flag == 81) BLEND1_OUTPUT = CALCULATE_BLEND(1); //blend 1
    else if (flag == 82) BLEND2_OUTPUT = CALCULATE_BLEND(2); //blend 2
    else if (flag == 83) BLEND3_OUTPUT = CALCULATE_BLEND(3); //blend 3
    else if (flag == 84) BLEND4_OUTPUT = CALCULATE_BLEND(4); //blend 4
    else if (flag == 85) BLEND5_OUTPUT = CALCULATE_BLEND(5); //blend 5
}

#define CHECK_CALC_BLEND_REFS(num) \
    TryCalculateReferencedBlend(blend##num##_src);\
    TryCalculateReferencedBlend(blend##num##_dst);\
    TryCalculateReferencedBlend(blend##num##_cof_map);\

void CalculateIndirectCoordinates()
{
    ///TODO this is slightly incorrect. Figure out how to apply indirect#_tgt_uv
    if (enable_indirect0)
    {   
        vec2 tex_coord_target = SelectTexCoord(indirect0_tgt_uv);
        vec2 ind_map = CalculateOutput(indirect0_src_map).xy;
        vec2 ind_offset = (ind_map - 0.5) *  mat.indirect0_scale;

        fIndirectCoords.xy = tex_coord_target + ind_offset;
    }
    if (enable_indirect1)
    {
        vec2 tex_coord_target = SelectTexCoord(indirect1_tgt_uv);
        vec2 ind_map = CalculateOutput(indirect1_src_map).xy;
        vec2 ind_offset = (ind_map - 0.5) *  mat.indirect1_scale;

        fIndirectCoords.zw = tex_coord_target + ind_offset;
    }
}

// Shader code adapted from learnopengl.com's PBR tutorial:
// https://learnopengl.com/PBR/Theory

vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

vec3 calcSpecularGGX(float roughness, vec3 f0, vec3 N, vec3 V, vec3 L, vec3 H)
{
	float N_H = saturate(dot(N, H));
	float L_H = saturate(dot(L, H));
	float N_V = saturate(dot(N, V));
	float N_L = saturate(dot(N, L));

    float D = DistributionGGX(N, H, roughness);
    vec3 kS = FresnelSchlick(max(dot(N, H), 0.0), f0);

	return f0 * kS * (N_L * D);
}

vec3 ReconstructNormal(in vec2 t_NormalXY) {
    float t_NormalZ = sqrt(clamp(1.0 - dot(t_NormalXY.xy, t_NormalXY.xy), 0.0, 1.0));
    return vec3(t_NormalXY.xy, t_NormalZ);
}

vec3 CalculateNormals(vec3 normals, vec2 normal_map)
{
    if (!enable_normal) //use vertex normals
        return normals;

    vec3 N = vec3(normals);
    vec3 T = vec3(fTangents.xyz);
    vec3 B = vec3(fTangents.w, fBitangents.xy);

    mat3 tbn_matrix = mat3(T, B, N);

    vec3 tangent_normal = N;
    if (enable_normal)
    {
        tangent_normal = ReconstructNormal(normal_map);
    }
    return normalize(tbn_matrix * tangent_normal).xyz;
}

vec3 CalculateEmissionScale(vec3 emission, int scale_type, vec4 irradiance)
{
    //Emission scale
    if      (scale_type == 1) //emission * irradiance, max by emission
        emission = max(irradiance.rgb * emission.rgb, emission.rgb);
    else if (scale_type == 2) //emission * irradiance, max by 1.0
        emission = emission * max(irradiance.rgb, 1.0);
    else if (scale_type == 3) //emission * irradiance color
        emission = emission * irradiance.rgb;
    else if (scale_type == 4) //emission * irradiance scale
        emission = emission * irradiance.w;
    else if (scale_type == 5) //max irradiance light amount
    {
        float max_scale = max(max(irradiance.g, irradiance.b), irradiance.r);
        emission = max_scale * emission;
    }
    else if (scale_type == 6) //max irradiance light amount maxed by emission amount
    {
        float max_scale = max(max(irradiance.g, irradiance.b), irradiance.r);
        emission = max(vec3(max_scale) * emission, emission);
    }
    else if (scale_type == 7) //exposure scale    
    {   
        float exposure = texture(cExposureTexture, vec2(0.0, 0.0)).a;
        emission *= 1.0 / exposure * mdlEnvView.Exposure.x;
    }
    return emission;
}

vec3 CalculateEmission(vec4 irradiance)
{
    vec3 emission = vec3(0.0);

    if (enable_emission)
    {
        emission = GetComp(CalculateOutput(o_emission), emission_component).rgb;

        if (vtxcolor_type == VTX_COLOR_TYPE_EMISSION)
            emission *= fVertexColor.rgb;

        //Emission scale
        emission = CalculateEmissionScale(emission, emission_scale_type, irradiance);
    }
    return emission;
}

vec3 CalculateClothEmission(vec4 irradiance)
{
    vec3 emission = CalculateOutput(o_cloth_emission_map).rgb * mat.cloth_nov_emission_scale0;
    //Emission scale
    return CalculateEmissionScale(emission, cloth_nov_emission_scale_type, irradiance);
}

vec3 CalculateMetalFlakeEmission(float refract_bias_x, float refract_bias_y, vec4 irradiance)
{
    //transparent tex with refract bias
    float transparent_tex = GetTransparentTexOutput(o_transparent_tex, refract_bias_x, refract_bias_y).r;
    //metal flake power
    float metal_flake_power = GetComp(CalculateOutput(o_metal_flake_power), metal_flake_power_component).x;

    float v = metal_flake_power * log2(CalulateSphereLight());
    vec3 emission_flake = vec3(transparent_tex * exp2(v));

    emission_flake = CalculateEmissionScale(emission_flake, metal_flake_emission_scale_type, irradiance );
        
    vec3 refract_color = CalculateOutput(o_refract_color).rgb;
    return refract_color * emission_flake;
}

void SetupBlend()
{
    //Calc blending. Compute any references first
#if enable_blend0
    CHECK_CALC_BLEND_REFS(0);
    BLEND0_OUTPUT = CALCULATE_BLEND(0);
#endif
#if enable_blend1
    CHECK_CALC_BLEND_REFS(1);
    BLEND1_OUTPUT = CALCULATE_BLEND(1);
#endif
#if enable_blend2
    CHECK_CALC_BLEND_REFS(2);
    BLEND2_OUTPUT = CALCULATE_BLEND(2);
#endif
#if enable_blend3
    CHECK_CALC_BLEND_REFS(3);
    BLEND3_OUTPUT = CALCULATE_BLEND(3);
#endif
#if enable_blend4
    CHECK_CALC_BLEND_REFS(4);
    BLEND4_OUTPUT = CALCULATE_BLEND(4);
#endif
#if enable_blend5
    CHECK_CALC_BLEND_REFS(5);
    BLEND5_OUTPUT = CALCULATE_BLEND(5);
#endif
}

float CalculateDirectionalLight(vec3 N)
{
    return saturate(dot(N, mdlEnvView.Dir.xyz)) * (1.0 / PI);
}

float CalculateDirectionalLightWrap(vec3 N)
{
    float NV = dot(vec3(N), mdlEnvView.Dir.xyz);

    float lighting_factor = clamp(fma(NV, 0.5, 0.5) * fma(NV, 0.5, 0.5) - clamp(NV, 0.0, 1.0), 0.0, 1.0);
    return lighting_factor * clamp(-0.0 + mat.wrap_coef, 0.0, 1.0);
}


vec4 CalculateDiffuseIrradianceLight(Light light)
{
    vec4 irradiance = vec4(0.0, 0.0, 0.0, 1.0);
    //Z seems flipped
    vec3 dir = vec3(light.N.x, light.N.y, -light.N.z);

    //irradiance lighting
    if (is_apply_irradiance_pixel)
    {
        if (enable_material_light) //use material light cubemap
        {
            const float MAX_LOD = 5.0;
            vec4 irradiance_cubemap = DecodeCubemap(cTextureMaterialLightCube, dir, MAX_LOD);
            irradiance.rgba = irradiance_cubemap.rgba * mdlEnvView.Exposure.y;
        }
        else //use material roughness cubemap
        {
            const float MAX_LOD = 5.0;
            vec4 irradiance_cubemap = DecodeCubemap(cTexCubeMapRoughness, dir, MAX_LOD);
            irradiance.rgba = irradiance_cubemap.rgba * mdlEnvView.Exposure.y;
        }
        //add sphere light if enabled
        if (enable_material_sphere_light)
        {
	        vec2 sphereCoords = light.N.xy * vec2(0.5) + vec2(0.5,0.5);
            vec4 sphere_light = textureLod(cTextureMaterialLightSphere, sphereCoords, 1.0).xyzw;
            irradiance.rgba += sphere_light.rgba * mdlEnvView.Exposure.y;
        }
    }
    else //calculated per vertex
    {
        //By vertex color
        if (vtxcolor_type == VTX_COLOR_TYPE_IRRADIANCE)
            irradiance.rgba = fVertexColor;
        else //Calculated in vertex shader
            irradiance.rgba = fIrradianceVertex.rgba;
    }
    return irradiance;
}

vec3 CalculateBrdf(vec3 view_normal, vec3 dir, float roughness, vec3 f0)
{
    float r = (1.0 - roughness);
    float a = r * r;
    float a2 = a * a;

    float nv = dot(view_normal, -dir);

    float s = clamp(min(a2 * fma(a2, 1.895, -0.1688), 
        fma(nv, fma(nv, fma(nv, -5.069, 8.404), -4.853), 0.9903)) + 0.0, 0.0, 1.0);

    float b = clamp(fma(nv, fma(nv, 0.1939, -0.5228), a2 *
        (fma(a2, fma(a2, 2.661, -3.603), nv * 1.404) + 1.699)) + 0.6045, 0.0, 1.0) - s;

    return f0.rgb * b + s * saturate(f0.g * 50.0);
}

void main()
{
    CalculateIndirectCoordinates();
    SetupBlend();

    vec4 base_color           = CalculateOutput(o_base_color);
    vec2 normal_map           = CalculateOutput(o_normal).rg;
    float metalness   = GetComp(CalculateOutput(o_metalness), metalness_component).r;
    float roughness   = GetComp(CalculateOutput(o_roughness), roughness_component).r;
    vec4 sss                  = CalculateOutput(o_sss);
    vec4 ao                   = CalculateOutput(o_ao);
    float alpha      = GetComp(CalculateOutput(o_alpha), alpha_component).w;

    bool has_transparent_tex = enable_transparent && transparent_type == TRANS_TYPE_TEX;

    vec3 eye_to_pos = vec3(fViewPos.zw, fLightColorVPosZ.w);
    vec3 dir = normalize(eye_to_pos);

    vec3 specularTerm = vec3(0.0);
    vec3 light_color = fLightColorVPosZ.xyz;

    //view tangents
    vec3 view_tangent = vec3(1, 0, 0);
    vec3 view_bitangent = vec3(1, 0, 1);
    if (o_normal != 30) //has tangents used
    {
        vec3 tangent = vec3(fTangents.xyz);
        vec3 bitangent = vec3(fTangents.w, fBitangents.xy);

        view_tangent = tangent.xyz * mat3(mdlEnvView.cView);
        view_bitangent = bitangent.xyz * mat3(mdlEnvView.cView);
    }

    //Metalness adjust
    metalness = saturate(metalness);

    //Roughness adjust
    roughness *= mat.force_roughness;
    roughness = saturate(roughness);

    vec3 vertex_normal = fNormalsDepth.rgb;
  //  if (is_use_back_face_lighting)
     //    vertex_normal = 1.0 - vertex_normal;

    //Normals
    vec3 N = CalculateNormals(vertex_normal.rgb, normal_map);
    N.x *= modelInfo.normal_axis_x_scale; //unsure what this is used for, 2D sections?

    vec3 view_normal = normalize(N * mat3(mdlEnvView.cView));

    //Normal to eye
    float N_I = clamp(fma(view_normal.z, -dir.z,
                fma(view_normal.x,  -dir.x, 
                    view_normal.y * -dir.y)), 0.0, 1.0);

    //Extra base color calculations

    //Dirt stain
    if (enable_add_stain_proc_texture_3d)
    {
        vec3 stain_texcoord = fVertexPos.xyz * mat.stain_uv_scale;
        float stain_intensity = texture(cTextureProcTexture3D, stain_texcoord).x * mat.stain_rate;

        base_color.rgb = clamp(mix(base_color.rgb,  mat.stain_color.rgb, stain_intensity), 0.0, 1.0);
    }

    //refract
    float refract_eta = GetComp(CalculateOutput(o_refract_eta), refract_eta_component).r;
    float refract_rate = GetComp(CalculateOutput(o_refract_rate), refract_rate_component).r;

    vec3 refract_view =  refract_eta * -N_I * view_normal + dir * mat.refract_thickness; 
	//refract bias
    float refract_bias_x = dot(refract_view, view_tangent);
    float refract_bias_y = -dot(refract_view, view_bitangent);

    //Cloth typically used for hair strands
    float cloth_value = 0.0;
    if (enable_cloth_nov)
    {
        //cloth color/output
        vec4 cloth_map = CalculateOutput(o_cloth_map);
        //cloth region to affect
        vec2 cloth_mask = GetComp(CalculateOutput(o_cloth_mask_map), cloth_mask_component).rg;

         float nov = N_I;

       // float nov = clamp(-dot(view_normal, dir), 0.0, 1.0);
        if (is_cloth_nov_reverse)
            nov = clamp(1.0 - nov, 0.0, 1.0);

        //peak offset
        float peak_pos = nov - mat.cloth_nov_peak_pos0;
        //tone and peak
        float nov_tone = pow(nov, mat.cloth_nov_tone_pow0); 
        float nov_peak = exp2(peak_pos * 0.0 - peak_pos * mat.cloth_nov_peak_pow0.x * 100.0) * mat.cloth_nov_peak_intensity0; 
        //cloth output
        cloth_value = clamp(nov_tone * mat.cloth_nov_slope0 + nov_peak, 0.0, 1.0);
        //apply mask
        cloth_value *= clamp(cloth_mask.x + -0.0, 0.0, 1.0);

        //random noise mask
        if (is_cloth_nov_use_rnd_noise_mask)
        {
            float noise = sin(fma(cloth_mask.y * mat.cloth_nov_noise_mask_scale0,
                78.233, cloth_mask.x * mat.cloth_nov_noise_mask_scale0 * 12.9898005)) * 43758.5469;

            cloth_value = clamp(fma(cloth_value * (noise - floor(noise) + -0.5), 40.0, cloth_value), 0.0, 1.0);
        }
        //Apply to diffuse
        base_color.rgb = mix(base_color.rgb, cloth_map.rgb, cloth_value);
    }

    //Ambient occ
    if (enable_ao)
        base_color.rgb *= ao.rgb;

    if (has_transparent_tex && transparent_tex_type == TRANS_TEX_TYPE_BASE_COLOR) //base color refract type
    {
        vec3 refract_color = CalculateOutput(o_refract_color).rgb;
        vec3 transparent_tex = GetTransparentTexOutput(o_transparent_tex, refract_bias_x, refract_bias_y).rgb;

        //refract mix
        base_color.rgb = fma(vec3(refract_rate), fma(transparent_tex, refract_color, 0.0 - base_color.rgb), base_color.rgb);
    }

    //Lighting
    Light light = SetupLight(N, eye_to_pos);

    //Apply refract rate and reduce diffuse
    base_color.rgb *= vec3(1) - refract_rate;

    //Fresnel
    vec3 f0 = mix(vec3(0.04), base_color.rgb, metalness); // dialectric
    vec3 brdf = CalculateBrdf(view_normal, dir, roughness, f0);

    //specular ggx
    if (is_use_forward_ggx_specular)
    {
        vec3 spec_intensity = calcSpecularGGX(roughness, f0, light.N, light.V, light.L, light.H);
        specularTerm += spec_intensity;
    }

    //Lighting
    float spec = metalness * 0.5 + 0.5;
    if (enable_material_light) //use material light cubemap
    {
        const float MAX_LOD = 5.0;
        vec4 spec_cubemap = DecodeCubemap(cTextureMaterialLightCube, light.R, roughness * MAX_LOD);
        specularTerm.rgb += spec * (spec_cubemap.rgb * mdlEnvView.Exposure.y) * brdf;
    }
    else
    {
        const float MAX_LOD = 5.0;
        vec4 spec_cubemap = DecodeCubemap(cTexCubeMapRoughness, light.R, roughness * MAX_LOD);
        specularTerm.rgb += spec * (spec_cubemap.rgb * mdlEnvView.Exposure.y) * brdf;
    }


    if (enable_structural_color)
    {
        //todo cTextureCubeMapStructuralColor * light cube when used
        //never have seen this used yet
    }

    if (enable_material_sphere_light)
    {
	    vec2 sphereCoords = light.N.xy * vec2(0.5) + vec2(0.5,0.5);
        vec4 sphere_light = textureLod(cTextureMaterialLightSphere, sphereCoords, roughness).xyzw;
        specularTerm.rgb += spec * (sphere_light.rgb * mdlEnvView.Exposure.y) * brdf;
    }
    
    //Diffuse
    vec3 diffuseTerm = saturate(base_color.rgb);

    //irradiance lighting
    vec4 irradiance = CalculateDiffuseIrradianceLight(light);

    //Adjust for metalness.
    diffuseTerm *= saturate(1.0 - metalness);
    diffuseTerm *= vec3(1) - brdf;

    diffuseTerm *= irradiance.rgb;

    //base color refract type
    if (enable_transparent)
    {
        vec3 refract_amount = refract_rate * (vec3(1) - brdf);
        vec3 refract_color = CalculateOutput(o_refract_color).rgb;

        if (has_transparent_tex && transparent_tex_type == TRANS_TEX_TYPE_DIFFUSE 
                                || transparent_tex_type == TRANS_TEX_TYPE_DIFFUSE_IRRADIANCE) 
        {
            vec3 transparent_tex = GetTransparentTexOutput(o_transparent_tex, refract_bias_x, refract_bias_y).rgb;
            //refract
            vec3 refract_value = transparent_tex * refract_color * refract_amount;
            if (transparent_tex_type == TRANS_TEX_TYPE_DIFFUSE_IRRADIANCE) //apply irradiance
                refract_value *= irradiance.rgb;

            diffuseTerm.rgb += refract_value;
        }
        if (enable_transparent)
        {
            if  (transparent_type == TRANS_TYPE_IND_FBO || transparent_type == TRANS_TYPE_IND_FBO_DEPTH)
            {
                vec2 ind_coords = refract_eta * -N_I * view_normal.xy;

                //coordinates with refract view as indirect
                vec2 coords = GetScreenCoordinates() + ind_coords;
                //Depth influences refraction
                if (enable_indirect_dist_correct)
                {
                    //depth difference between our current depth and the previously sampled depth
                    float d = -abs(fNormalsDepth.w - texture(cTextureLinearDepth, coords).x) * mat.indirect_depth_scale;
                    d = saturate(1.0 - exp2(d));
                   coords = GetScreenCoordinates() + ind_coords * d;
                }
                //depth type 
                if (transparent_type == TRANS_TYPE_IND_FBO_DEPTH)
                {
                    //Only refract coords behind the current pixel, else keep coordinates normal
                    if (texture(cTextureLinearDepth, coords).x < fNormalsDepth.w) 
                        coords = GetScreenCoordinates();
                }
                vec4 fbo = texture(cFrameBufferTex, coords);
                diffuseTerm.rgb += refract_amount * fbo.rgb * refract_color.rgb;
            }
        }
    }

    if (enable_alphamask)
    {
        if (alpha_test_func == 0)
        {
            discard;
        }
        else if (alpha_test_func == 10)
        {
            if (alpha >= mat.alpha_test_value)
                discard;
        }
        else if (alpha_test_func == 20)
        {
            if (alpha != mat.alpha_test_value)
                discard;
        }
        else if (alpha_test_func == 30)
        {
            if (alpha > mat.alpha_test_value)
                discard;
        }
        else if (alpha_test_func == 40)
        {
            if (alpha <= mat.alpha_test_value)
                discard;
        }
        else if (alpha_test_func == 50)
        {
            if (alpha == mat.alpha_test_value)
                discard;
        }
        else if (alpha_test_func == 60)
        {
            if (alpha < mat.alpha_test_value)
                discard;
        }
    }

    //Light output diffuse + specular
    oLightBuf.rgb = diffuseTerm.rgb + specularTerm;
    oLightBuf.a = alpha; 

    //Cloth Emission
    if (enable_cloth_nov)
        oLightBuf.rgb += CalculateClothEmission(irradiance) * cloth_value;

    //Emission
    if (enable_emission)
        oLightBuf.rgb += CalculateEmission(irradiance).rgb;

    //Metal flake emission
    if (has_transparent_tex && transparent_tex_type == TRANS_TEX_TYPE_METAL_FLAKE)
        oLightBuf.rgb += CalculateMetalFlakeEmission(refract_bias_x, refract_bias_y, irradiance);

    //Sub surface scattering
    if (enable_sss)
    {
        float light_intensity = CalculateDirectionalLightWrap(view_normal);
        oLightBuf.rgb += light_color.xyz * diffuseTerm.rgb * light_intensity * sss.r * (1.0 / PI);
    }

    if (enable_translucent) //adjusts with shadows TODO
    {
        oLightBuf.rgb = oLightBuf.rgb;
    }

    vec2 motion_vector = vec2(0.0);

    if (cRenderType == RENDER_TYPE_DEFERRED_OPAQUE) 
    {
        //motion vector using the previous position differences
        //Unsure what these are used for?
        if (is_output_motion_vec)
        {
            vec3 posInvProj = vec3(fBitangents.zw, fPreviousPos.w);
            vec3 prevPos = fPreviousPos.xyz;

            float invPosZ  = 1.0 / posInvProj.z;
            float invPrevZ = 1.0 / prevPos.z;

            motion_vector.x = (invPosZ * posInvProj.y) - (prevPos.y * invPrevZ) * 0.25;
            motion_vector.y = (invPosZ * posInvProj.x) - (prevPos.x * invPrevZ) * 0.25;
        }
    }
    else if (cRenderType == RENDER_TYPE_DEFERRED_XLU) 
    {
        if (enable_clamp_lbuf) //only seems to be used here in this render type for some reason
            oLightBuf.rgb = clamp(oLightBuf.rgb, 0.0, 1.0);
        //xlu alpha output
        oLightBuf.a = alpha * modelInfo.model_alpha_mask; 
    }
    else if (cRenderType == RENDER_TYPE_FORWARD) 
    {
        oLightBuf.a = alpha * modelInfo.model_alpha_mask; 
    }

    //clamp 0 - 2048 due to HDR/tone mapping
    oLightBuf.rgb = max(oLightBuf.rgb, 0.0);    
    oLightBuf.rgb = min(oLightBuf.rgb, 2048.0);

    //Normals output as half lambert
    oWorldNrm.rg = N.rg * 0.5 + 0.5;

    //Depth output used for computing the deferred shader
    oNormalizedLinearDepth.r = fNormalsDepth.w;
    oNormalizedLinearDepth.y = 0.0;
    oNormalizedLinearDepth.z = 0.0;
    oNormalizedLinearDepth.w = 0.0;

    //Base color with encoded roughness/metalness to compute extra deferred calculations
    oBaseColor = EncodeBaseColor(saturate(base_color.rgb), roughness, metalness, N);

    //Motion vector
    if (is_output_motion_vec)
        oMotionVec.xy = motion_vector.xy;

    //Forward render output
    if (cRenderType == RENDER_TYPE_FORWARD)
    {
        oBaseColor.rgb = saturate(base_color.rgb);
        oBaseColor.a = alpha; 
    }

  //  oLightBuf.rgb = vec3(1.0, 1.0, 0.0) * 50.0;
   // oBaseColor.rgb = vec3(1.0, 1.0, 0.0);

    if (DEBUG_VISUALIZER)
    {
        int type = int(mat.const_single3);

        if (type == -1) oLightBuf.rgb = base_color.rgb; 
        if (type == 1) oLightBuf.rgb = N.rgb * 0.5 + 0.5; 
        if (type == 2) oLightBuf.rgb = CalculateEmission(irradiance).rgb;
        if (type == 3) oLightBuf.rgb = vec3(roughness);
        if (type == 4) oLightBuf.rgb = vec3(metalness);
        if (type == 5) oLightBuf.rgb = brdf;
        if (type == 6) oLightBuf.rgb = irradiance.rgb;
        if (type == 7) oLightBuf.rgb = vec3(alpha);
        if (type == 8) oLightBuf.rgb = vec3(cloth_value);
        if (type == 9) oLightBuf.rgb = mix(vec3(0.0), CalculateOutput(o_cloth_map).rgb, cloth_value);
        if (type == 10) oLightBuf.rgb = CalculateClothEmission(irradiance) * cloth_value;
        if (type == 11) oLightBuf.rgb = vec3(sss);
        if (type == 12)
        {
            float light_intensity = CalculateDirectionalLightWrap(view_normal);
            oLightBuf.rgb = vec3(light_intensity) * sss.r * (1.0 / PI);
        }
        if (type == 13) oLightBuf.rgb = ao.rgb;
        if (type == 14) oLightBuf.rgb = diffuseTerm.rgb;
        if (type == 15) oLightBuf.rgb = specularTerm.rgb;
        if (type == 16) oLightBuf.rgb = CalculateOutput(o_refract_color).rgb;
        if (type == 17) oLightBuf.rgb = CalculateOutput(o_refract_eta).rgb;
        if (type == 18) oLightBuf.rgb = CalculateOutput(o_refract_rate).rgb;

        
        if (type == 21) oLightBuf.rgb = BLEND0_OUTPUT.rgb;
        if (type == 22) oLightBuf.rgb = BLEND1_OUTPUT.rgb;
        if (type == 23) oLightBuf.rgb = BLEND2_OUTPUT.rgb;
        if (type == 24) oLightBuf.rgb = BLEND3_OUTPUT.rgb;
        if (type == 25) oLightBuf.rgb = BLEND4_OUTPUT.rgb;
        if (type == 26) oLightBuf.rgb = BLEND5_OUTPUT.rgb;

        oLightBuf.a = 1.0;
    }

    return;
}