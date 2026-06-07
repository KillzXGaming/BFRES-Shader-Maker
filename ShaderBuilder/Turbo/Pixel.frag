#version 450 core

#include "UniformBlocks.glsl"

// Most code based on
// https://github.com/magcius/noclip.website/blob/master/src/MarioKart8Deluxe/Render.ts

//@ render_info="gsys_pass" default="no_setting" type="string" group="Render Info" order="-1"
//@ render_info="gsys_dynamic_depth_shadow" default="1" type="string" group="Render Info" order="-1"
//@ render_info="gsys_dynamic_depth_shadow_only" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_static_depth_shadow" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_static_depth_shadow_only" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_cube_map" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_cube_map_only" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_dynamic_reflection" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_multi_filter" default="0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_priority_hint" default="player" type="string" group="Render Info" order="-1"
//@ render_info="gsys_priority" default="0" type="int" group="Render Info" order="-1"
//@ render_info="gsys_light_diffuse" default="diffuse" type="string" group="Render Info" order="-1"
//@ render_info="gsys_env_obj_set" default="Turbo_area0" type="string" group="Render Info" order="-1"
//@ render_info="gsys_bake_group" default="group1" type="string" group="Render Info" order="-1"
//@ render_info="gsys_bake_texel_param" default="100" type="float" group="Render Info" order="-1"
//@ render_info="gsys_bake_uv_unite" default="none" type="string" group="Render Info" order="-1"
//@ render_info="gsys_bake_normal_map" default="default" type="string" group="Render Info" order="-1"
//@ render_info="gsys_bake_emission_map" default="default" type="string" group="Render Info" order="-1"

//@ render_info="gsys_render_state_display_face" default="front" type="string" group="Render Info" order="-1"
//@ render_info="gsys_render_state_mode" default="opaque" type="string" group="Render Info" order="-1"
//@ render_info="gsys_depth_test_enable" default="true" type="string" group="Render Info" order="-1"
//@ render_info="gsys_depth_test_func" default="lequal" type="string" group="Render Info" order="-1"
//@ render_info="gsys_depth_test_write" default="true" type="string" group="Render Info" order="-1"
//@ render_info="gsys_alpha_test_enable" default="false" type="string" group="Render Info" order="-1"
//@ render_info="gsys_alpha_test_func" default="gequal" type="string" group="Render Info" order="-1"
//@ render_info="gsys_alpha_test_value" default="0.5" type="float" group="Render Info" order="-1"
//@ render_info="gsys_render_state_blend_mode" default="none" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_rgb_op" default="add" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_rgb_src_func" default="src_alpha" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_rgb_dst_func" default="one_minus_src_alpha" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_alpha_op" default="add" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_alpha_src_func" default="one" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_alpha_dst_func" default="zero" type="string" group="Render Info" order="-1"
//@ render_info="gsys_color_blend_const_color" default="0 0 0 0" type="float" group="Render Info" order="-1"

//@

#if (BLENDER_RENDER == 0)

	#if (IS_WII_U == 1)
		uniform sampler2D albedo_texture; //@ id="_a0"
		uniform sampler2D specular_map;  //@ id="_s0"
		uniform sampler2D normal_map; //@ id="_n0"
		uniform sampler2D indirect_texture; //@ id="_a3"
		uniform sampler2D texture_multiA; //@ id="_a1"
		uniform sampler2D texture_multiB; //@ id="_a2"
		uniform sampler2D emissive_map; //@ id="_e0"

		#if (gsys_enable_normal_map2)
		uniform sampler2D normal_map_layer2; //@ id="_n1"
		#endif

		uniform sampler2D bake_shadow_map; //@ id="_b0"
		uniform sampler2DArray bake_light_map; //@ id="_b1"

		#if (enable_opa_trans_tex)
		uniform sampler2D transmission_map; //@ id="_t0"
		#endif
		uniform samplerCube gsys_lightmap_diffuse; //@ id="gsys_lightmap_diffuse"
		uniform samplerCubeArray gsys_cube_map; //@ id="gsys_cube_map"
		uniform sampler2DArray gsys_light_prepass; //@ id="gsys_light_prepass"
		uniform sampler2D gsys_static_depth_shadow; //@ id="gsys_static_depth_shadow"

		#if (enable_color_buffer || enable_depth_buffer)
		uniform sampler2D gsys_color_buffer; //@ id="gsys_color_buffer"
		uniform sampler2D gsys_normalized_linear_depth;  //@ id="gsys_normalized_linear_depth"
		#endif

		#if (enable_projection_shadow)
		uniform sampler2D gsys_projection0; //@ id="gsys_projection0"
		#endif

	#else // Switch
		layout (binding = 0) uniform sampler2D albedo_texture; //@ id="_a0"
		layout (binding = 1) uniform sampler2D specular_map; //@ id="_s0"
		layout (binding = 2) uniform sampler2D normal_map; //@ id="_n0"
		layout (binding = 3) uniform sampler2D indirect_texture; //@ id="_a3"
		layout (binding = 4) uniform sampler2D emissive_map; //@ id="_e0"
		layout (binding = 5) uniform sampler2D transmission_map; //@ id="_t0"
		layout (binding = 6) uniform sampler2D texture_multiA; //@ id="_a1"
		layout (binding = 7) uniform sampler2D texture_multiB; //@ id="_a2"
		layout (binding = 8) uniform sampler2D normal_map_layer2; //@ id="_n1"
		layout (binding = 9) uniform sampler2D bake_shadow_map; //@ id="_b0"
		layout (binding = 10) uniform sampler2DArray bake_light_map; //@ id="_b1"
		layout (binding = 11) uniform samplerCube gsys_lightmap_diffuse; //@ id="gsys_lightmap_diffuse"
		layout (binding = 12) uniform samplerCubeArray gsys_cube_map; //@ id="gsys_cube_map"
		layout (binding = 13) uniform sampler2D gsys_static_depth_shadow; //@ id="gsys_static_depth_shadow"
		layout (binding = 14) uniform sampler2DArrayShadow gsys_depth_shadow_cascade; //@ id="gsys_depth_shadow_cascade"
		layout (binding = 15) uniform sampler2D gsys_projection0; //@ id="gsys_projection0"
		layout (binding = 16) uniform sampler2DArray gsys_light_prepass; //@ id="gsys_light_prepass"
		layout (binding = 17) uniform sampler2D gsys_color_buffer; //@ id="gsys_color_buffer"
		layout (binding = 18) uniform sampler2D gsys_normalized_linear_depth;  //@ id="gsys_normalized_linear_depth"

		layout (location = 0) in vec4 fTexCoords0;
		layout (location = 1) in vec4 fFog;
		layout (location = 2) in vec4 fTangents;
		layout (location = 3) in vec4 fNormals;
		layout (location = 4) in vec4 fTexCoordsBake;
		layout (location = 5) in vec4 fProjCoords;
		layout (location = 6) in vec4 fViewDirection;
		layout (location = 7) in vec4 fScreenCoords;
		layout (location = 8) in vec4 fBitangents;
		layout (location = 9) in vec4 fVtxColor0;
		layout (location = 10) in vec4 fTexCoords23;
		layout (location = 11) in vec4 fViewPos;
		layout (location = 12) in vec4 fCascadeInfo; // xy: cascade shadow uv, z: cascade index, w: depth

		layout (location = 0) out vec4 out_attr0;
		layout (location = 3) out vec4 oNormals;
	#endif
#endif

#define fViewPosZ fViewPos.z
#define fVertexAlpha fTexCoords0.w

struct BakeResult {
    vec3 IndirectLight;
    float Shadow;
    float AO;
};

#include "WiiUCommon.glsl"

float saturate(float v)
{
    return clamp(v, 0.0, 1.0);
}
vec3 saturate(vec3 v)
{
    return clamp(v, 0.0, 1.0);
}

vec3 saturation(vec3 rgb, float adjustment)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    vec3 intensity = vec3(dot(rgb, W));
    return mix(intensity, rgb, adjustment);
}

vec3 transformToMinusZForward(in vec3 v) {
	v.z *= -1.0; // samplerCubes expect -Z forward, but world normals are at +Z
	
	float cubeNormalized = max(abs(v.x), max(abs(v.y), abs(v.z)));
	v /= cubeNormalized;
	
	return v;
}

vec2 calc_texcoord_matrix(mat2x4 matrix, vec2 tex_coord)
{
	// Actually a 2x3 matrix stored in 2x4
    vec2 tex_coord_out;
    tex_coord_out.x = fma(tex_coord.x, matrix[0].x, tex_coord.y * matrix[0].z) + matrix[1].x;
    tex_coord_out.y = fma(tex_coord.x, matrix[0].y, tex_coord.y * matrix[0].w) + matrix[1].y;
	return tex_coord_out;
}


struct SelectTexCoordInfo {
	vec2 tex_coord;
	mat2x4 matrix;
	int type;
	
	vec2 result;
};

vec2 get_tex_coord(inout SelectTexCoordInfo info)
{
	if (info.type == 0 && !gsys_invalidate_texture_srt)
		return calc_texcoord_matrix(info.matrix, info.tex_coord);
	
	return info.tex_coord;
}

SelectTexCoordInfo SelectTexCoordOnlyType0(in int t_Selection) {
	SelectTexCoordInfo info;

    if (t_Selection == 0) {
        info.tex_coord = fTexCoords0.xy;
		info.matrix = mat.tex_mtx0;
		info.type = texcoord_calc_texcoord0;
	}
    else if (t_Selection == 2) {
        info.tex_coord = fTexCoords23.xy;
		info.matrix = mat.tex_mtx1;
		info.type = texcoord_calc_texcoord2;
	}
    else if (t_Selection == 3) {
        info.tex_coord = fTexCoords23.zw;
		info.matrix = mat.tex_mtx2;
		info.type = texcoord_calc_texcoord3;
	}
    else
        return info; // error
		
	info.result = get_tex_coord(info);
	return info;
}

float GetGeoAlpha(sampler2D sampler)
{
    if (geo_multi_alpha_type == 1)  // texture with uv 0
       return texture(sampler, SelectTexCoordOnlyType0(0).result).a;
    else if (geo_multi_alpha_type == 2) // texture with uv 2
       return texture(sampler, SelectTexCoordOnlyType0(2).result).a;
    else if (geo_multi_alpha_type == 3) // texture with uv 3
       return texture(sampler, SelectTexCoordOnlyType0(3).result).a;

    // Type 0 default 
    return fVertexAlpha; 
}

void Indirect(inout vec2 t_TexCoord, bool is_indirect, int select)
{
    if (!is_indirect || !enable_indirect)
        return;

    vec2 t_IndOffset = texture(indirect_texture, SelectTexCoordOnlyType0(texcoord_select_indirectA).result).rg;

    if (!indirect_texture_is_BC5s)
        t_IndOffset = t_IndOffset * 2.0 - 1.0;

    t_IndOffset *= mat.indirect_mag.xy * 2.0;
    t_TexCoord += t_IndOffset;
}

vec3 ReconstructNormal(in vec2 t_NormalXY) 
{
    float t_NormalZ = sqrt(saturate(1.0 - dot(t_NormalXY.xy, t_NormalXY.xy)));
    return vec3(t_NormalXY.xy, t_NormalZ);
}

vec3 UnpackNormalMap(vec4 t_NormalMapSample)
{
    vec2 t_NormalXY = t_NormalMapSample.xy;
	
	#if (BLENDER_RENDER == 1)
	t_NormalXY = t_NormalXY * 2.0 - 1.0;
	#else
    if (gsys_normalmap_BC1)	// BC1
	    t_NormalXY = t_NormalXY * 2.0 - 1.0;
	#endif

    return ReconstructNormal(t_NormalXY);
}

vec3 SampleNormalMap0()
{
    vec2 tex_coords = SelectTexCoordOnlyType0(texcoord_select_normal).result;
    Indirect(tex_coords, indirect_effect_normal, select_indirect_mag_normal);

    vec4 t_normalMap = texture(normal_map, tex_coords);        
	return UnpackNormalMap(t_normalMap);
}

vec3 SampleNormalMap1(sampler2D sampler)
{
    vec2 tex_coords = SelectTexCoordOnlyType0(texcoord_select_normal2).result;
    Indirect(tex_coords, indirect_effect_normal2, select_indirect_mag_normal2);

    vec4 t_normalMap = texture(sampler, tex_coords);        
	return UnpackNormalMap(t_normalMap);
}

vec3 CalcTangentToWorld(in vec3 t_TangentNormal, in vec3 t_Basis0, in vec3 t_Basis1, in vec3 t_Basis2) {
    return (t_TangentNormal.xxx * t_Basis0 + 
		   t_TangentNormal.yyy * t_Basis1 + 
		   t_TangentNormal.zzz * t_Basis2);
}

vec3 CalcNormals()
{
    if (!enable_normal_map)
        return normalize(fNormals.xyz);

    // TBN
    vec3 t_Normals = fNormals.xyz;
    vec3 t_Tangent = fTangents.xyz;
    vec3 t_Binormal = vec3(0);

    t_Binormal = cross(fNormals.xyz, fTangents.xyz) * fTangents.w;

    // Normals calculatiom
    vec3 t_TangentNormal0 = fNormals.xyz;
    if (enable_normal_map)
        t_TangentNormal0 = SampleNormalMap0();

    vec3 t_NormalWorld0 = CalcTangentToWorld(t_TangentNormal0, t_Tangent, t_Binormal, t_Normals);
	
    if (gsys_enable_normal_map2)
    {
        vec3 t_TangentNormal1 = SampleNormalMap1(normal_map_layer2);
        vec3 t_NormalWorld1 = CalcTangentToWorld(t_TangentNormal1, t_Tangent, t_Binormal, t_Normals);
		
		float mixer = 0.5;
		if (enable_geo_multi)
			mixer = GetGeoAlpha(albedo_texture);
		else
			mixer = mat.normal_map_weight;
		
		return normalize(mix(t_NormalWorld0, t_NormalWorld1, mixer));
    }

    return normalize(t_NormalWorld0);
}

SelectTexCoordInfo SelectTexCoord(in int t_Selection) 
{
	SelectTexCoordInfo info = SelectTexCoordOnlyType0(t_Selection);
	
	if (info.type != 0) 
	{
		vec3 n_map = CalcNormals();
		vec3 view_n = (normalize(n_map.xyz) * mat3(context.cView)).xyz;
		
		#if (BLENDER_RENDER == 1)
		vec2 uv = vec2(view_n.x, view_n.y);
		#else
		vec2 uv = vec2(view_n.x, -view_n.y);
		#endif
		
		if (info.type == 1) // Sphere mapping with Y negative offset (e.g. used on 8's Rainbow Road glass hologram)
			uv = vec2(uv.x * 0.5 + 0.5, uv.y * 0.5 - 0.5);
			
		else if (info.type == 4) // Sphere mapping with Y positive offset (e.g. used on metal characters)
			uv = uv * 0.5 + 0.5;
		
		info.result = calc_texcoord_matrix(info.matrix, uv);
	}
	
	return info;
}	

vec3 convert_cubemap_hdr(vec4 cubemap)
{
    float hdr_scale = context.cCubemapHDR.x;
    float hdr_range = context.cCubemapHDR.y;

    vec3 rgb = cubemap.rgb * pow(cubemap.a, hdr_range) * hdr_scale;
    return rgb; 
}

vec3 CalculateColorEffect(vec3 t_Color, float t_EdgeFactor)
{
    // Simple color mix between ratio and color
    t_Color = mix(t_Color, mat.gsys_i_color0, mat.gsys_i_color_ratio0);

    // Edge effect (TODO: Not working)
    if (enable_gamefx_edge)
    {
        float w = mat.gsys_edge_width0 + 2.0;
        float edge_intensity = clamp(pow(t_EdgeFactor * w, mat.game_edge_pow), 0.0, 1.0);

        // Calculate outline effect
        float edge_amount = edge_intensity * mat.gsys_edge_ratio0;
        t_Color = mix(t_Color, mat.gsys_edge_color0, edge_amount);
    }
	
    return t_Color;
}

vec4 SampleMultiTextureA() 
{
    vec2 t_TexCoord = SelectTexCoord(texcoord_select_multiA).result;
    Indirect(t_TexCoord, indirect_effect_multiA, select_indirect_mag_multiA);
    return texture(texture_multiA, t_TexCoord).rgba;
}

vec4 SampleMultiTextureB() 
{
    vec2 t_TexCoord = SelectTexCoord(texcoord_select_multiB).result;
    Indirect(t_TexCoord, indirect_effect_multiB, select_indirect_mag_multiB);
    return texture(texture_multiB, t_TexCoord).rgba;
}

void CalcMultiTexture(in int t_OutputType, inout vec4 t_Sample)
{
    if (!enable_multi_texture)
        return;

    if (multi_tex_output_type != t_OutputType)
        return;

    if (enable_geo_multi)
    {
        t_Sample.rgb = mix(t_Sample.rgb, SampleMultiTextureA().rgb, GetGeoAlpha(albedo_texture));
        return;
    }

    if (multi_tex_calc_type_color == 1)
        t_Sample.rgb *= SampleMultiTextureA().rgb;
    else if (multi_tex_calc_type_color == 2)
        t_Sample.rgb *= SampleMultiTextureA().rgb * SampleMultiTextureB().rgb;
    else if (multi_tex_calc_type_color == 3)
        t_Sample.rgb = mix(0.0 - SampleMultiTextureA().rgb + t_Sample.rgb, SampleMultiTextureA().rgb, t_Sample.a);
    else if (multi_tex_calc_type_color == 5)
        t_Sample.rgb = saturate((t_Sample.rgb + SampleMultiTextureA().rgb - vec3(mat.multi_tex_reg0.x)) * mat.multi_tex_reg0.y);
    else if (multi_tex_calc_type_color == 6)
        t_Sample.rgb = saturate((t_Sample.rgb + SampleMultiTextureB().rgb - vec3(mat.multi_tex_reg0.x)) * mat.multi_tex_reg0.y);
    else if (multi_tex_calc_type_color == 7)
        t_Sample.rgb = mix(t_Sample.rgb, SampleMultiTextureA().rgb, SampleMultiTextureA().a);
    else if (multi_tex_calc_type_color == 8)
    {
        vec3 t_Sum = saturate(t_Sample.rgb + SampleMultiTextureA().rgb + SampleMultiTextureB().rgb - mat.multi_tex_reg0.x) * mat.multi_tex_reg0.y;
        t_Sample.rgb = mix(mat.multi_tex_reg2.rgb, mat.multi_tex_reg1.rgb, t_Sum);
    }
    else if (multi_tex_calc_type_color == 11)
        t_Sample.rgb = mix(mat.multi_tex_reg0.rgb, mat.multi_tex_reg1.rgb, t_Sample.a);
    else if (multi_tex_calc_type_color == 12)
        t_Sample.rgb = saturate(t_Sample.rgb * SampleMultiTextureA().rgb + SampleMultiTextureB().rgb);
    else if (multi_tex_calc_type_color == 13)
        t_Sample.rgb = saturate(SampleMultiTextureA().rgb + SampleMultiTextureB().rgb -mat.multi_tex_reg0.x) * mat.multi_tex_reg0.y;
    else if (multi_tex_calc_type_color == 14)
        t_Sample.rgb = mix(mat.multi_tex_reg0.rgb, t_Sample.rgb, t_Sample.a);
    else if (multi_tex_calc_type_color == 17)
        t_Sample.rgb = mix(SampleMultiTextureA().rgb * mat.multi_tex_reg0.rgb, t_Sample.rgb, mat.multi_tex_reg0.a);
    else if (multi_tex_calc_type_color == 19)
        t_Sample.rgb = t_Sample.rgb * saturate(SampleMultiTextureA().rgb + mat.multi_tex_reg0.rgb);
    else if (multi_tex_calc_type_color == 20)
        t_Sample.rgb = mix(mat.multi_tex_reg0.rgb, t_Sample.rgb, fVtxColor0.rgb);
    else if (multi_tex_calc_type_color == 21)
        t_Sample.rgb = mix(mat.multi_tex_reg0.rgb, mat.multi_tex_reg1.rgb, (t_Sample.r + t_Sample.g + t_Sample.b) / 3.0);
    else if (multi_tex_calc_type_color == 22)
        t_Sample.rgb = saturate(SampleMultiTextureA().rgb * SampleMultiTextureB().rgb + t_Sample.rgb);
    else if (multi_tex_calc_type_color == 24)
    {
        float scale = SampleMultiTextureA().x * mat.multi_tex_reg0.w * mat.multi_tex_reg1.w;
        vec3 color = mix(mat.multi_tex_reg0.rgb,mat. multi_tex_reg1.rgb, SampleMultiTextureA().x);
        t_Sample.rgb = color * scale + t_Sample.rgb;
    }
    else if (multi_tex_calc_type_color == 25)
    {
        float scale = SampleMultiTextureA().x * mat.multi_tex_reg0.w * mat.multi_tex_reg1.w;
        vec3 color = mix(mat.multi_tex_reg0.rgb + mat.multi_tex_reg1.rgb, mat.multi_tex_reg2.rgb, SampleMultiTextureA().x);
        t_Sample.rgb = color * scale + t_Sample.rgb;
    }
    else if (multi_tex_calc_type_color == 27)
        t_Sample.rgb = saturate(SampleMultiTextureA().rgb * mat.multi_tex_reg0.rgb + t_Sample.rgb);
    else if (multi_tex_calc_type_color == 30)
        t_Sample.rgb =  t_Sample.rgb * (SampleMultiTextureA().rgb + SampleMultiTextureB().rgb);
    else if (multi_tex_calc_type_color == 33)
        t_Sample.rgb =  SampleMultiTextureA().rgb * mat.multi_tex_reg0.rgb + t_Sample.rgb;
    else if (multi_tex_calc_type_color == 34)
        t_Sample.rgb =  SampleMultiTextureA().rgb + t_Sample.rgb;
    else if (multi_tex_calc_type_color == 35)
        t_Sample.rgb =  SampleMultiTextureA().rgb + t_Sample.rgb; // same as 34
    else if (multi_tex_calc_type_color == 38)
    {
        vec3 color = SampleMultiTextureA().rgb * -SampleMultiTextureB().rgb + SampleMultiTextureA().rgb;
        t_Sample.rgb = color * mat.multi_tex_reg0.r + t_Sample.rgb;
    }
    else if (multi_tex_calc_type_color == 39)
        t_Sample.rgb *= mat.multi_tex_reg0.rgb + SampleMultiTextureA().rgb * mat.multi_tex_reg1.rgb;
    else if (multi_tex_calc_type_color == 40)
        t_Sample.rgb = SampleMultiTextureA().rgb * SampleMultiTextureA().a * fVtxColor0.r + t_Sample.rgb;
    else if (multi_tex_calc_type_color == 44)
        t_Sample.rgb = saturate((t_Sample.rgb.x - SampleMultiTextureA().rgb - mat.multi_tex_param0.x) * mat.multi_tex_param0.y);

    if (multi_tex_calc_type_alpha == 1)
        t_Sample.a *= SampleMultiTextureA().a;
    else if (multi_tex_calc_type_alpha == 4)
        t_Sample.a *= (SampleMultiTextureA().a + SampleMultiTextureB().a - mat.multi_tex_reg0.z) * mat.multi_tex_reg0.a;
    else if (multi_tex_calc_type_alpha == 6)
        t_Sample.a = SampleMultiTextureA().a;
    else if (multi_tex_calc_type_alpha == 7)
        t_Sample.a = SampleMultiTextureA().r;
    else if (multi_tex_calc_type_alpha == 8)
        t_Sample.a = SampleMultiTextureB().r;
		
    // Used for eyes
    else if (multi_tex_calc_type_alpha == 13)
        t_Sample.a = clamp(fVertexAlpha * t_Sample.a * (1.0 - SampleMultiTextureA().a) + t_Sample.a, 0.0, 1.0);
}

void CalcGeoMultiTextureSpec(inout vec4 t_SpecMask)
{
    if (!enable_geo_multi || !enable_multi_texture)
        return;

    if (geo_multi_specmask_calc_type == 0)
    {
        if (multi_tex_calc_type_color == 0)
            t_SpecMask.rgb = mix(t_SpecMask.rgb, SampleMultiTextureB().rgb, GetGeoAlpha(albedo_texture));
        else if (multi_tex_calc_type_color == 7)
            t_SpecMask.rgb = mix(t_SpecMask.rgb, SampleMultiTextureB().rgb, GetGeoAlpha(texture_multiA));
    }
    else if (geo_multi_specmask_calc_type == 1)
        t_SpecMask.rgb *= SampleMultiTextureB().rgb;
}

void CalcBakeResult(out BakeResult t_Result, in vec4 t_TexCoordBake) 
{
    if (enable_diffuse && bake_light_type == 0)
    {
		#if (BLENDER_RENDER == 1)
        vec4 t_Lightmap = texture(bake_light_map, t_TexCoordBake.zw);
		#else
        vec4 t_Lightmap = texture(bake_light_map, vec3(t_TexCoordBake.zw, 0));
		#endif
		
        t_Result.IndirectLight = t_Lightmap.rgb * t_Lightmap.a * mat.gsys_bake_light_scale.rgb * scene_material.lighting.y;
    }
    else
        t_Result.IndirectLight = vec3(0.0);

	t_Result.AO = 1.0;
	t_Result.Shadow = 1.0;
    if (bake_shadow_type == 0)
    {
        float t_BakeSample = texture(bake_shadow_map, t_TexCoordBake.xy).r;
        t_Result.AO = t_BakeSample;
    }
    else if (bake_shadow_type == 1)
    {
        float t_BakeSample = texture(bake_shadow_map, t_TexCoordBake.xy).r;
        t_Result.Shadow = t_BakeSample;
    }
    else if (bake_shadow_type == 2)
    {
        vec2 t_BakeSample = texture(bake_shadow_map, t_TexCoordBake.xy).rg;
        t_Result.AO = t_BakeSample.r;
        t_Result.Shadow = t_BakeSample.g;
    }
}

float CalcLuminance(vec3 color)
{
    return saturate(dot(color, vec3(0.298911989, 0.586610973, 0.114478)));
}

vec3 CalculateHemiLighting(vec3 N) 
{
	float hemiIntensity = max(dot(N, -normalize(environment.cHemiDirection.xyz)), 0);
    vec3 hemiLight = mix(environment.cHemiGroundColor.rgb * environment.cHemiGroundColor.w, environment.cHemiSkyColor.rgb * environment.cHemiSkyColor.w, hemiIntensity);
	
	return hemiLight;
}

vec3 CalculateLighting(vec3 N, float intensity)
{
    float directionaIntensity = max(dot(N, -normalize(environment.cLightDirection0.xyz)), 0);
    vec3 directionalLight = mix(vec3(0.0), environment.cLightColor.rgb * environment.cLightColor.w, directionaIntensity);

    vec3 hemiLight = CalculateHemiLighting(N);
    vec3 lit = hemiLight + directionalLight;
    vec3 unlit = hemiLight;
    return mix(lit, unlit, intensity);
}

vec2 CalcSpecularFresnel(float edge, float f_i, float f_s, float f_m, float roughness)
{
	float fresnelExp = 2.0 + (f_s * 6.0);
    float fresnel = saturate(4.0 * pow(edge, fresnelExp));
	float fIntensity = f_i * (1.0 - fresnel);
	float fresnel_m = 1.0 - fIntensity * (1.0 - f_m);
	
	float specIntensity = fresnel_m * f_i * 0.5 + fresnel_m;
	float specRoughness = saturate(roughness + fIntensity * 0.4);

    return vec2(specIntensity, specRoughness);
}

vec3 CalculateSpecular(vec3 t_Normal, vec3 t_ViewDirection, vec3 t_Lightprepass, float shadow, vec3 indirect_bake_light)
{
    // Roughness specular
    float t_Roughness = mat.specular_roughness;
    float spec_intensity = max(0.0, mat.specular_intensity);
    vec3 spec_color = mat.specular_color;

    if (enable_specular_aniso)
    {
        // TODO
    }

    // Specular texture config
    if (enable_specular_mask)
    {
        vec2 spec_coords = SelectTexCoord(texcoord_select_specmask).result;
        Indirect(spec_coords, indirect_effect_specmask, select_indirect_mag_specmask);

        vec4 spec_mask = texture(specular_map, spec_coords).rgba;
        CalcMultiTexture(3, spec_mask);

        if (enable_geo_multi)
            CalcGeoMultiTextureSpec(spec_mask);

        spec_color *= spec_mask.rgb;
        if (enable_specular_mask_rougness)
        {
            if (specular_mask_is_color)
                t_Roughness *= 1.0 - spec_mask.a;
            else
                t_Roughness *= 1.0 - spec_mask.g;
        }
    }
    if (enable_vtx_color_spec)
        spec_color *= fVtxColor0.rgb;

    if (enable_specular_physical)
        t_Roughness = 1.0 - spec_intensity;

    if (enable_fresnel)
    {
        float mag = dot(t_Normal.xyz, t_ViewDirection.xyz);
        float edge = saturate(1.0 + mag);
        vec2 fresnel_output = CalcSpecularFresnel(edge,
                                        mat.specular_fresnel_i, 
                                        mat.specular_fresnel_s,
                                        mat.specular_fresnel_m, t_Roughness);
        spec_intensity *= fresnel_output.x;
        t_Roughness = fresnel_output.y;
    }

    // Increase spec by intensity
    spec_color *= max(0.0, spec_intensity);

    // Multiply roughness to max mip for getting cubemap mipmap
    float max_mip = 5; // Total mipmaps used for the reflection cubemap
    t_Roughness *= max_mip;
	
	vec3 t_Reflect = reflect(t_ViewDirection.xyz, normalize(t_Normal)); // reflection
    t_Reflect = fma(t_Normal * dot(t_ViewDirection, t_Normal), vec3(-2.0), t_ViewDirection);

    // Flip directions to better match original specular lighting
    t_Reflect.z *= -1.0;

    vec3 spec_output = vec3(0);
    if (enable_light_pre_pass)
    {
        spec_output += t_Lightprepass.rgb;
    }
	
	if (enable_area_env_direct) // Area cubemap assgined by mat param
	{
		#if (BLENDER_RENDER == 1)
		vec4 specular_cubemap = textureLod(gsys_cube_map, vec4(t_Reflect, t_Roughness), 0);	
		#else
		vec4 specular_cubemap = textureLod(gsys_cube_map, vec4(t_Reflect, mat.gsys_area_env_index_diffuse), t_Roughness);	
		#endif

		spec_output += convert_cubemap_hdr(specular_cubemap);
	}
	else if (gsys_enable_area_env) // Moving objects
	{
		//Cubemaps blend when changing areas.
		float blend_ratio = shape.cAreaParams.z;

		#if (BLENDER_RENDER == 1)
		vec4 specular_cubemap0 = texture(gsys_cube_map, vec4(t_Reflect, t_Roughness));	
		vec4 specular_cubemap1 = texture(gsys_cube_map, vec4(t_Reflect, t_Roughness));	
		#else
		vec4 specular_cubemap0 = textureLod(gsys_cube_map, vec4(t_Reflect, shape.cAreaParams.x), t_Roughness);	
		vec4 specular_cubemap1 = textureLod(gsys_cube_map, vec4(t_Reflect, shape.cAreaParams.y), t_Roughness);	
		#endif

		vec3 cubemap_0 = convert_cubemap_hdr(specular_cubemap0);
		vec3 cubemap_1 = convert_cubemap_hdr(specular_cubemap1);

		spec_output += mix(cubemap_0, cubemap_1, blend_ratio);  
	}
	else
	{
		#if (BLENDER_RENDER == 1)
		vec4 specular_cubemap = texture(gsys_cube_map, vec4(t_Reflect, t_Roughness));	
		#else
		vec4 specular_cubemap = textureLod(gsys_cube_map, vec4(t_Reflect, 0), t_Roughness);	
		#endif
			
		spec_output += convert_cubemap_hdr(specular_cubemap);
	}

    spec_output *= max(vec3(0.0), spec_color);

    // Luminance adjust with shadow scaling
    float scale  = scene_material.lighting_specular.x;
    float shadow_l_scale = scene_material.lighting_specular.y;

    if (enable_bake_l_cancel_s) // Cancel specular by bake light and shadow
    {
        float light_cancel = CalcLuminance(indirect_bake_light) * scene_material.lighting_specular.z;
        float s = saturate(shadow - light_cancel);
        scale *= 1.0 - s * shadow_l_scale;
    }
    else if (enable_specular_shadow_link) // Cancel specular by luminance spec and shadow
    {
        float luminance = CalcLuminance(spec_color);
        float s = saturate(shadow - luminance);

        scale *= 1.0 - s * shadow_l_scale;
    }
    else if (HAS_SHADOW) // Cancel spec by shadow (if shadows are used)
    {
        scale *= 1.0 - shadow * shadow_l_scale;
    }

    return spec_output * scale;
}

void DoAlphaTest(float t_Alpha)
{
    if (gsys_alpha_test_enable)
    {
        float alpha_ref_value = mat.gsys_alpha_test_ref_value;

        if (gsys_alpha_test_func == 0) // never
            return;
			
        else if (gsys_alpha_test_func == 1) // less
        {
            if (t_Alpha < alpha_ref_value)
                return;
        }
        else if (gsys_alpha_test_func == 3) // lequal
        {
            if (t_Alpha <= alpha_ref_value)
                return;
        }
        else if (gsys_alpha_test_func == 4) // greater
        {
            if (t_Alpha > alpha_ref_value)
                return;
        }
        else if (gsys_alpha_test_func == 5) // nequal
        {
            if (t_Alpha != alpha_ref_value)
                return;
        }
        else if (gsys_alpha_test_func == 6) // gequal
        {
            if (t_Alpha >= alpha_ref_value)
                return;
        }
        else if (gsys_alpha_test_func == 7) // equal
        {
            if (t_Alpha == alpha_ref_value)
                return;
        }
		
		discard;
    }
}

void main()
{
	vec2 screenCoords = fScreenCoords.xy / fScreenCoords.w;
    screenCoords.y = 1.0 - screenCoords.y;

    vec3 t_ViewDirection = normalize(fViewDirection.xyz);

	#if (BLENDER_RENDER == 1)
    t_ViewDirection = vec3(t_ViewDirection.x, t_ViewDirection.z, -t_ViewDirection.y);
	#endif

    // Calculate incoming light
    vec3 t_IncomingLightDiffuse = vec3(0.0);
    vec3 t_IncomingLightSpecular = vec3(0.0);
    vec3 t_IncomingLightTransmission = vec3(0.0);
    vec3 t_AmbientOcc = vec3(1.0);

	// NORMALS ---------------------------------------------------------------------------------------------------------------------
	vec3 t_FragNormal = CalcNormals();

    float front_face = 1.0;
    if (enable_opa_trans)
        front_face = gl_FrontFacing ? 1.0 : 0.0;
		
    // Flip normals if needed based on front facing usage
    if (enable_opa_trans)
        t_FragNormal = fma(vec3(front_face) * t_FragNormal, vec3(2.0), 0.0 - t_FragNormal);

    float NdotV = dot(normalize(t_FragNormal), t_ViewDirection.xyz);
    float t_EdgeFactor = saturate(NdotV + 1.0);
    vec3 view_normal = normalize(t_FragNormal * mat3(context.cView));
	
	if (enable_refraction)
		screenCoords.xy += vec2(view_normal.x, view_normal.y * context.cScreen.z) * mat.refraction_intensity;
		
	// ALBEDO -----------------------------------------------------------------------------------------------------------------------
    vec3 t_Albedo = scene_material.lighting.z * mat.albedo_tex_color.rgb;
	float t_Alpha = 1.0;
	bool color_buffer = (enable_color_buffer && color_buffer_as_albedo);

    if ((enable_diffuse2 && enable_albedo) || color_buffer)
    {
        // Indirect calculations
        vec2 ind_offset = vec2(0.0);
        Indirect(ind_offset, indirect_effect_albedo, select_indirect_mag_albedo);
		
        vec2 texCoords = SelectTexCoord(texcoord_select_albedo).result;
        vec4 albedo_sample = vec4(1.0);
		
        if (color_buffer)
        {
            // Indirect coords behind only
            // First indirect the depth tex, adjusting the size with the screen width/height ratio
            vec2 off = normalize(ind_offset) * vec2(context.cf.z, context.cf.w);
				
            // Get the depth
            float depth_n = texture(gsys_normalized_linear_depth, screenCoords + off * 2.0).r;
            float z = (fScreenCoords.z / fScreenCoords.w + 1.0) * 0.5; // normalized z depth
            float dist = depth_n - z; // distance between current depth and previously sampled depth

            // Check if depth is within range of the sampled depth and the surface/material z
            // Refract any behind the depth
            float depth_diff = ((dist > 0.0 ? 1.0 : 0.0) + 0.0 - dist < 0.0 ? 1.0 : 0.0 + 1.0) * 0.5; // TODO: this doesn't work

            vec4 color_tex = texture(gsys_color_buffer, screenCoords + ind_offset * depth_diff);
            albedo_sample.rgb = color_tex.rgb;
			albedo_sample.a = 1.0;
        }
		else
		    albedo_sample = texture(albedo_texture, texCoords + ind_offset);
			
		CalcMultiTexture(0, albedo_sample);
		t_Alpha = saturate(albedo_sample.a * fVertexAlpha);

        t_Albedo.rgb *= albedo_sample.rgb;
    }
	
	if (ENABLE_TRANSPARENCY)
		t_Alpha *= mat.transparency;
	else
		DoAlphaTest(t_Alpha);
		
    t_Alpha = saturate(t_Alpha);
	
    if (IS_DEPTH)
    {
        out_attr0 = vec4(0, 0, 0, t_Alpha);
        return;
    }
    if (IS_GBUFFER)
    {
        out_attr0 = vec4(0, 0, 0, t_Alpha);
        oNormals.rgb = t_FragNormal.rgb * 0.5 + 0.5;
        oNormals.a = mat.specular_roughness; // Might be used for light pre pass specular
        return;
    }

    if (enable_vtx_color_diff)
        t_Albedo *= fVtxColor0.rgb;

    // Bakes (Shadow/AO and Lightmaps) ---------------------------------------------------------------------------------------------
    BakeResult t_BakeResult;
    t_BakeResult.IndirectLight = vec3(0.0);

    if (enable_bake_texture)
        CalcBakeResult(t_BakeResult, fTexCoordsBake);

    t_BakeResult.IndirectLight *= front_face;
	
    if (enable_bake_texture && bake_shadow_type >= 0)
    { 
        t_AmbientOcc *= mix(scene_material.ao_color.rgb, vec3(1.0), t_BakeResult.AO);
        t_AmbientOcc = saturate(mix(vec3(1.0), t_AmbientOcc, mat.ao_density));
    }

    // EMISSION ---------------------------------------------------------------------------------------------------------------------
    vec3 t_Emission = mat.emission_color.rgb * (mat.emission_intensity * scene_material.lighting.w);
	
    if (enable_emission_map)
    {
        vec2 emission_coords = SelectTexCoord(texcoord_select_emission).result;
        Indirect(emission_coords, indirect_effect_emission, select_indirect_mag_emission);

        vec4 emission_map = texture(emissive_map, emission_coords).rgba;
        CalcMultiTexture(1, emission_map);

        t_Emission *= emission_map.rgb;
    }
    if (enable_vtx_color_emission)
        t_Emission *= fVtxColor0.rgb;

	// SHADOWS -----------------------------------------------------------------------------------------------------------------------
	float t_ShadowMult = 1.0;

	if (ENABLE_DEPTH_SHADOW_CASCADE) 
		t_ShadowMult *= texture(gsys_depth_shadow_cascade, fCascadeInfo);
	else 
	{
		if (enable_dynamic_depth_shadow) 
			t_ShadowMult *= texture(gsys_static_depth_shadow, screenCoords).r; // dynamic
			
		if (enable_static_depth_shadow)
			t_ShadowMult *= texture(gsys_static_depth_shadow, screenCoords).g; // static
	}

    if (enable_projection_shadow)
    {
        float shadow_proj = textureProj(gsys_projection0, fProjCoords.xyz).r;
        t_ShadowMult *= mix(1.0, shadow_proj, context.cProjParams.x); // intensity param
    }

    if (enable_bake_texture) {
	    t_ShadowMult *= t_BakeResult.Shadow;
		t_ShadowMult = saturate(t_ShadowMult);
	}
	
	float t_ShadowIntensity = 1.0 - t_ShadowMult;
	if (HAS_SHADOW)
		t_ShadowIntensity *= mat.shadow_density * scene_material.shadow_color.a;

    // LIGHTING ------------------------------------------------------------------------------------------------------------------------
	float t_LightIntensity = t_ShadowIntensity;
	
	// LIGHTS (e.g. probe lighting, point, spot lights) --------------------------------------------------------------------------------
    vec3 light_prepass_diff = vec3(0.0);
    vec3 light_prepass_spec = vec3(0.0);

    if (enable_diffuse && enable_light_pre_pass)
    {
		#if (BLENDER_RENDER == 1)
        light_prepass_diff = texture(gsys_light_prepass, screenCoords).xyz;
        light_prepass_spec = vec3(0.0);
		#else
        light_prepass_diff = texture(gsys_light_prepass, vec3(screenCoords, 0)).xyz;
        light_prepass_spec = texture(gsys_light_prepass, vec3(screenCoords, 1.0)).xyz;
		#endif

        // Shadows affect light prepass
        light_prepass_diff *= (t_ShadowIntensity * scene_material.light_prepass_param.z);
        light_prepass_spec *= (t_ShadowIntensity * scene_material.light_prepass_param.z);
		
		if (enable_light_pre_pass_intensity) {
			light_prepass_diff *= mat.light_pre_pass_intensity;
			light_prepass_spec *= mat.light_pre_pass_intensity;
		}
    }
    
	// Inverse vectors
	vec3 t_FragNormalInv = transformToMinusZForward(t_FragNormal);

    // TRANSMISSION ----------------------------------------------------------------------------------------------------------------------
    if (enable_opa_trans)
    {
        vec3 t_Transmission = mat.transmit_color * mat.transmit_intensity * scene_material.lighting.x; 

		float t_TransmitLightIntensity = t_LightIntensity;
		if (HAS_SHADOW)
			t_TransmitLightIntensity *= mat.transmit_shadow_intensity;
			
		if (enable_opa_trans_albedo) 
			t_Transmission *= t_Albedo.rgb;
        else if (enable_opa_trans_tex)
        {
            vec2 texCoords = SelectTexCoord(texcoord_select_transmitt).result;
			t_Transmission *= texture(transmission_map, texCoords).rgb;
        }
		else
			t_Transmission /= 10.0;
		
        // Lighting but with negative view direction
		vec3 t_FragTransNormal = transformToMinusZForward(-t_FragNormal);
		vec3 t_Lightmap_DiffuseTrans = textureLod(gsys_lightmap_diffuse, t_FragTransNormal, t_TransmitLightIntensity).rgb;

		#if (BLENDER_RENDER == 1)
		t_Lightmap_DiffuseTrans = CalculateLighting(t_FragNormalInv, t_TransmitLightIntensity);
		#endif

        t_IncomingLightTransmission.rgb = t_Transmission * t_Lightmap_DiffuseTrans;
    }

	// LIGHT MAP ---------------------------------------------------------------------------------------------------------------------------
	vec3 t_Lightmap_Diffuse = vec3(0.0);
	
	if (enable_diffuse)
		t_Lightmap_Diffuse = textureLod(gsys_lightmap_diffuse, t_FragNormalInv, t_ShadowIntensity).rgb;
	else
		t_Lightmap_Diffuse = t_ShadowIntensity * (textureLod(gsys_lightmap_diffuse, vec3(0.0, 1.0, 0.0), 1.0).rgb - 1.0) + 1.0;
	
	#if (BLENDER_RENDER == 1)
	t_Lightmap_Diffuse = CalculateLighting(t_FragNormalInv, t_LightIntensity);
	#endif

    t_IncomingLightDiffuse += t_Lightmap_Diffuse;
	t_IncomingLightDiffuse += t_BakeResult.IndirectLight;
	if (enable_light_pre_pass)
		t_IncomingLightDiffuse += light_prepass_diff;

    // SPECULAR -----------------------------------------------------------------------------------------------------------------------------
    if (enable_specular) {
		t_IncomingLightSpecular += CalculateSpecular(t_FragNormal, t_ViewDirection.xyz, light_prepass_spec, 
                                    t_ShadowIntensity, t_BakeResult.IndirectLight);
	}

    if (enable_depth_buffer)
    {
        // Get the depth
        float depth_n = texture(gsys_normalized_linear_depth, screenCoords).r;
		
        // Convert into depth space from the vertex pos
        float depth = depth_n * context.cNearFar.y + fViewPosZ;

        if (enable_xlu_depth)
        {
            if (enable_xlu_depth_silhoutte)
            {
                float a = saturate(mat.silhoutte_depth_contrast * (1.0 - depth * mat.silhoutte_depth));
                if (enable_xlu_depth_inv)
                    a = 1.0 - a;
					
                t_Albedo.rgb = mix(t_Albedo.rgb, mat.silhoutte_depth_color.rgb, a);
            }
            else
            {
                float a = saturate(depth * mat.silhoutte_depth);
                if (enable_xlu_depth_inv)
                    a = 1.0 - a;

                t_Alpha = saturate(t_Alpha + a);
            }
        }

        if (enable_soft_edge)
            t_Alpha *= clamp(depth * mat.soft_edge_dist_inv, 0, 1);
    }

    if (enable_edge_light)
    {
        // Light from the view direction
        vec3 t_Lightmap_View = textureLod(gsys_lightmap_diffuse, t_ViewDirection * vec3(1, 1, -1), 0.0).rgb;

		// Clamp to 127 for HDR/tone mapping (no black bleeding)
        float amount_edge_light = min(127.0, pow(t_EdgeFactor, mat.edge_light_sharpness * 10.0) * mat.edge_light_intensity);
        vec3 edge_light_color = mat.edge_light_color.rgb;
		
        if (enable_vtx_color_edge_light)
            edge_light_color *= fVtxColor0.rgb;

        vec3 light = amount_edge_light * edge_light_color * mix(vec3(1.0), t_Lightmap_View, mat.edge_light_rim_i);
        t_Albedo.rgb += light;
    }

    // OUTPUT -----------------------------------------------------------------------------------------------------------------------------------
    vec3 t_OutputColor = t_Albedo.rgb;
	
	if (enable_diffuse || HAS_SHADOW)
		t_OutputColor *= t_IncomingLightDiffuse;
        
    if (enable_opa_trans)
        t_OutputColor.rgb += t_IncomingLightTransmission.rgb;

    if (enable_specular)
        t_OutputColor.rgb += t_IncomingLightSpecular.rgb;

    if (!enable_albedo_ao)
        t_OutputColor.rgb *= t_AmbientOcc.rgb;
    else
    {
        t_OutputColor.rgb *= mix(t_Albedo.rgb,
                            roundEven(t_AmbientOcc.rgb),
                            abs(2.0 * t_AmbientOcc.rgb - 1.0));
    }

    if (enable_emission)
        t_OutputColor.rgb += t_Emission;

    if (enable_game_effect) // Game color effect (e.g. star power-up, blooper's ink)
        t_OutputColor.rgb = CalculateColorEffect(t_OutputColor.rgb, t_EdgeFactor);

    if (enable_decal_ao)
    {
        float t_TrailValue = texture(gsys_static_depth_shadow, screenCoords).w;
		switch (decal_trail_type) {
			case 1: // Light
				t_TrailValue = saturate(max(t_TrailValue * 2.0, 0.1));
				t_OutputColor.rgb = mix(vec3(0.0), t_OutputColor.rgb, t_TrailValue);
				
				break;
				
			case 2: // Ice
				break;
				
			case 3: // Unknown
				break;
				
			case 4: // Snow or sand
				t_TrailValue = max(min(fma((0.0 - t_LightIntensity + 2.0) * (t_TrailValue + -0.5), 4.0, 1.0), 1.8), 0.2);
				t_OutputColor.rgb = mix(vec3(0.0), t_OutputColor.rgb, t_TrailValue);
				
				break;
				
			case 5: // Emission from anti-gravity
				t_TrailValue = saturate(t_TrailValue * -2.0 + 1.0) * mat.decal_trail_intensity;
				vec3 t_TrailColor = mat.decal_trail_color * t_TrailValue;
				t_OutputColor.rgb += t_TrailColor;
				
				break;
		}
    }

    if (enable_fog) // No enable_fog_y done yet
        t_OutputColor.rgb = mix(t_OutputColor, fFog.rgb, fFog.a);
    
	// Clamp 0 - 127 due to HDR/tone mapping
    t_OutputColor.rgb = max(t_OutputColor.rgb, 0.0);    
    t_OutputColor.rgb = min(t_OutputColor.rgb, 127.0);

    out_attr0.x = t_OutputColor.r;
    out_attr0.y = t_OutputColor.g;
    out_attr0.z = t_OutputColor.b;
    out_attr0.w = t_Alpha;

    if (enable_debug_shading) // Check the type by changing the bloom_intensity parameter
    {           
        out_attr0.rgb = vec3(0.0);    

        int type = int(mat.bloom_intensity);
		switch (type) {
			case 0: // Diffuse light
				out_attr0.rgb = max(t_IncomingLightDiffuse.rgb, 0.0);
				break;
				
			case 1: // Albedo
				out_attr0.rgb = max(t_Albedo.rgb, 0.0);
				break;
				
			case 2: // Specular
				out_attr0.rgb = max(t_IncomingLightSpecular.rgb, 0.0);
				break;
				
			case 3: // Normals
				out_attr0.rgb = t_FragNormal.rgb * 0.5 + 0.5;
				break;
				
			case 4: // Emission
				out_attr0.rgb = t_Emission.rgb;
				break;
				
			case 5: // Transmission
				out_attr0.rgb = t_IncomingLightTransmission.rgb;
				break;
				
			case 6: // Shadow
				out_attr0.rgb = vec3(t_ShadowIntensity);
				break;
				
			case 7: // Edge lighting
				vec3 t_Lightmap_View = textureLod(gsys_lightmap_diffuse, t_ViewDirection * vec3(1, 1, -1), 0.0).rgb;
				float amount_edge_light = pow(t_EdgeFactor, mat.edge_light_sharpness * 10.0) * mat.edge_light_intensity;
				vec3 light = amount_edge_light * mat.edge_light_color.rgb * mix(vec3(1.0), t_Lightmap_View, mat.edge_light_rim_i);
				
				out_attr0.rgb = light;
				break;
				
			case 8: // Normal map
				out_attr0.rgb = SampleNormalMap0().rgb;
				break;
				
			case 9: // Tangents
				out_attr0.rgb = fTangents.rgb * 0.5 + 0.5;
				break;
				
			case 10: // Alpha
				out_attr0.rgb = vec3(t_Alpha);
				break;
				
			case 11: // AO
				if (enable_albedo_ao)
				{
					out_attr0.rgb = mix(t_Albedo.rgb,
										roundEven(t_AmbientOcc.rgb),
										abs(2.0 * t_AmbientOcc.rgb - 1.0) );
				}
				else
					out_attr0.rgb = vec3(t_AmbientOcc);
					
				break;
				
			case 12: // Indirect light color
				out_attr0.rgb = t_BakeResult.IndirectLight.rgb;
				break;
				
			case 13: // Indirect light alpha	
			
				#if (BLENDER_RENDER == 1)
				vec4 t_Lightmap = texture(bake_light_map, fTexCoordsBake.zw);
				#else
				vec4 t_Lightmap = texture(bake_light_map, vec3(fTexCoordsBake.zw, 0));
				#endif

				out_attr0.rgb = vec3(t_Lightmap.a);
				break;
			
			case 14: // UV
				out_attr0.rgb = vec3(fTexCoordsBake.zw, 1);
				break;
		}

        if (type != 0) // Adjust for no srgb correction when not diffuse
            out_attr0.rgb = pow(out_attr0.rgb, vec3(2.2));   
			
        out_attr0.a = 1.0;
    }

    return;
}

