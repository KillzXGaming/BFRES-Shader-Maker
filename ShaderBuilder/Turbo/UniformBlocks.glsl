#define BLENDER_RENDER 0

const int MAX_BONE_COUNT = 100;

#define enable_debug_shading false  //

#define IS_WII_U 0 //@

//-----------------------------------
//Static Options
//-----------------------------------

#define gsys_alpha_test_enable false //@ render_state_property="ALPHA_TEST" render_info="gsys_alpha_test_enable" choices="false:0 true:1"
#define gsys_alpha_test_func 6 //@ choices="0 1 2 3 4 5 6 7" render_state_property="ALPHA_FUNC"
#define enable_far_infinity false //@
#define enable_far_inf_ignore_y false //@
#define gsys_invalidate_world_srt false //@
#define gsys_invalidate_texture_srt false //@
#define gsys_invalidate_xlu_zprepass false //@
#define enable_normal_map false //@
#define gsys_normalmap_BC1 false //@
#define gsys_enable_normal_map2 false //@
#define gsys_enable_area_env true //@
#define enable_area_env_direct false //@
#define gsys_enable_light_probe false //@
#define enable_indirect false //@
#define indirect_world_scale false //@
#define indirect_texture_is_BC5s false //@
#define indirect_effect_albedo false //@
#define indirect_effect_normal false //@
#define indirect_effect_normal2 false //@
#define indirect_effect_specmask false //@
#define indirect_effect_emission false //@
#define indirect_effect_multiA false //@
#define indirect_effect_multiB false //@
#define select_indirect_mag_albedo 0 //@ choices="0 1"
#define select_indirect_mag_normal 0 //@ choices="0 1"
#define select_indirect_mag_normal2 0 //@ choices="0 1"
#define select_indirect_mag_specmask 0 //@ choices="0 1"
#define select_indirect_mag_emission 0 //@ choices="0 1"
#define select_indirect_mag_multiA 0 //@ choices="0 1"
#define select_indirect_mag_multiB 0 //@ choices="0 1"
#define enable_color_buffer false //@
#define color_buffer_as_albedo false //@
#define color_buffer_as_multi_A false //@
#define color_buffer_as_multi_B false //@
#define enable_refraction false //@
#define mii_albedo_skin_color false //@
#define mii_albedo_tex_skin_color false //@
#define mii_albedo_favorite_color false //@
#define mii_albedo_tex_favorite_color false //@
#define mii_reg0_skin_color false //@
#define mii_reg0_favorite_color false //@
#define enable_multi_texture false //@
#define multi_tex_output_type 0 //@ choices="0 1 3 2"
#define multi_tex_calc_type_color 0 //@ choices="0 1 2 19 3 7 4 11 14 15 16 17 21 26 42 24 25 28 5 6 18 27 33 41 29 34 39 35 8 9 10 12 22 36 37 38 13 30 31 32 40 43 20 44 23 100"
#define multi_tex_calc_type_alpha 0 //@ choices="0 1 13 14 2 11 12 3 4 5 6 15 7 8 9 10"
#define enable_geo_multi false //@
#define geo_multi_alpha_type 0 //@ choices="0 1 2 3"
#define geo_multi_specmask_calc_type 0 //@ choices="0 1"
#define enable_diffuse2 true //@
#define enable_albedo true //@
#define enable_vtx_color_diff false //@
#define enable_diffuse true //@
#define enable_opa_trans false //@
#define enable_opa_trans_tex false //@
#define enable_opa_trans_albedo false //@
#define enable_edge_light false //@
#define enable_vtx_color_edge_light false //@
#define enable_specular false //@
#define enable_specular_mask false //@
#define enable_specular_mask_rougness false //@
#define specular_mask_is_color false //@
#define enable_specular_shadow_link false //@
#define enable_vtx_color_spec false //@
#define enable_fresnel false //@
#define enable_emission false //@
#define enable_emission_map false //@
#define enable_vtx_color_emission false //@
#define enable_bake_texture false //@
#define enable_bake_shadow_mask false //@
#define enable_bake_l_cancel_s false //@
#define enable_d_shadow_bake_l_cancel false //@
#define enable_projection_shadow false //@
#define enable_projection_light false //@
#define enable_static_depth_shadow false //@
#define enable_dynamic_depth_shadow false //@
#define enable_decal_ao false //@
#define enable_albedo_ao false //@
#define enable_fog true //@
#define enable_fog_y false //@
#define fog_game_effect_order false //@
#define enable_fog_y_prim false //@
#define enable_fog_emission false //@
#define enable_fog_edge false //@
#define enable_vtx_alpha_trans false //@
#define enable_color_buffer_blend false //@
#define enable_edge_xlu false //@
#define enable_edge_xlu_rev false //@
#define enable_water_surface false //@
#define enable_shiny_specular false //@
#define enable_fresnel_look_depend true //@
#define enable_fresnel_look_factor false //@
#define enable_game_effect false //@
#define gamefx_lerp_type false //@
#define enable_gamefx_edge false //@
#define enable_gamefx_edge_fragment false //@
#define enable_specular_dirty_mask false //@
#define enable_light_pre_pass_intensity false //@
#define enable_white_exposure false //@
#define enable_cascade_shadow_force1 false //@
#define enable_vertex_billboard false //@
#define enable_screen_fake_scale false //@
#define texcoord_calc_texcoord0 0 //@ choices="0 1 4 2 3"
#define texcoord_calc_texcoord2 0 //@ choices="0 1 4 2 3"
#define texcoord_calc_texcoord3 0 //@ choices="0 1 4 2 3"
#define texcoord_aspect_mod_texcoord0 0 //@ choices="0 1 2"
#define texcoord_aspect_mod_texcoord2 0 //@ choices="0 1 2"
#define texcoord_aspect_mod_texcoord3 0 //@ choices="0 1 2"
#define texcoord_select_albedo 0 // choices="0 2 3"
#define texcoord_select_specmask 0 //@ choices="0 2 3"
#define texcoord_select_normal 0 //@ choices="0 2 3"
#define texcoord_select_normal2 0 //@ choices="0 2 3"
#define texcoord_select_indirectA 0 //@ choices="0 2 3"
#define texcoord_select_multiA 0 //@ choices="0 2 3"
#define texcoord_select_multiB 0 //@ choices="0 2 3"
#define texcoord_select_emission 0 //@ choices="0 2 3"
#define texcoord_select_transmitt 0 //@ choices="0 2 3"
#define enable_specular_aniso false //@
#define bake_shadow_type -1 //@ choices="-1 0 1 2 3 4 5"
#define bake_light_type -1 //@ choices="-1 0 1 2 3 4 5"
#define bake_debug_disable_shadow false //@
#define enable_light_pre_pass true //@
#define enable_light_pre_pass_no_effect false //@
#define decal_trail_type 0 //@ choices="0 1 2 3 4 5 6 10 11 12 13 14 15 16 17 7 8 9 18 19 20 21"
#define gsys_renderstate 0 //@ render_state_property="RENDER_STATE" render_info="gsys_render_state_mode" choices="opaque:0 mask:1 translucent:2 custom:3"
#define enable_specular_physical false //@

// XLU options (todo should we split shader models?)
#define enable_depth_buffer false //@
#define enable_xlu_depth_silhoutte false //@
#define enable_soft_edge false //@
#define enable_xlu_depth_inv false //@
#define enable_xlu_depth false //@
#define enable_xlu_spec false //@
#define enable_xlu_alpha_out false //@
#define enable_xlu_discard false //@

// Wii U specific which is unused. Add anyways so no warnings appear for missing option macros
#define enable_edge_light_with_probe false //@
#define enable_effect_normal_offset false //@
#define gsys_reflection_type 0 //@
#define bake_calc_type 0 //@

//-----------------------------------
//Dynamic Options
//-----------------------------------
#define gsys_weight -1 //@ branch="dynamic" is_skin_count="true"  choices="-1 0 1 2 3 4 5 6 7 8"
#define gsys_assign_type 0 //@ branch="dynamic" type="string" flags="compile_all_coices" choices="gsys_assign_material gsys_assign_zonly gsys_assign_gbuffer gsys_assign_xlu_zprepass gsys_assign_visualize"
#define system_id 0 //@ branch="dynamic" choices="0"

#define IS_GBUFFER gsys_assign_type == 2  //gsys_assign_gbuffer
#define IS_DEPTH gsys_assign_type == 1  //gsys_assign_zonly
#define ENABLE_TRANSPARENCY gsys_renderstate == 2 || gsys_renderstate == 3 || gsys_assign_type == 3
    
// Wii U merges the scene material block, switch seperated them
#if (IS_WII_U == 1)

// Choose 6 binding since wii u doesn't work with 0 binding
layout (std140, binding = 6) uniform Context //@ id="gsys_context" size="2320"
{
    mat3x4 cView;
    mat4 cViewProj;
    mat4 cProj;
    mat3x4 cViewInv;
    vec4 cNearFar; //znear, zfar, ratio, inverse ratio
    vec4 cFovParams; //fov
    mat3x4 cProjectionTexMtx0;
    mat3x4 cProjectionTexMtx1;
    vec4 cProjParams;
    vec4 cProjParams1;
    vec4 cProjParams3;
    vec4 cProjParams4;
    vec4 cProjParams5;

    mat4 cCascadeMtx0;
    mat4 cCascadeMtx1;
    mat4 cCascadeMtx2;
    mat4 cCascadeMtx3;

    vec4 cShadowParams0;
    vec4 cShadowParams1;
    vec4 cShadowParams2;
    vec4 cShadowParams3;
    vec4 cShadowParams4;

    mat4 cStaticShadowProj;
    vec4 cStaticShadowParams;

    vec4 shadow_color;
    vec4 ao_color;

    mat3x4 cPrevView;
    mat4 cPrevViewProj;
    mat4 cPrevProj;
    mat3x4 cPrevViewInv;

    vec4 blurParams;

    vec4 light_prepass_param; //z = shadow intensity

    vec4 nearFarParams2;

    vec4 exposure;
    vec4 lighting;
    vec4 cf;
    vec4 cCubemapHDR;
    vec4 canvas;
    vec4 state;
    vec4 lighting_specular;
}context;

#else

layout (std140, binding = 0) uniform Context //@ id="gsys_context" size="2320"
{
    mat3x4 cView;
    mat4 cViewProj;
    mat4 cProj;
    mat3x4 cViewInv;
    vec4 cNearFar; //znear, zfar, ratio, inverse ratio
    vec4 cScreen; //screen width/height
    vec4 cDist; //zfar - znear
    vec4 cFovParams; //fov
    vec4 cf;
    vec4 cc;
    vec4 sf;
    mat3x4 cPrevView;
    mat4 cPrevViewProj;
    mat4 cPrevProj;
    mat3x4 cPrevViewInv;
    mat3x4 cProjectionTexMtx0;
    mat3x4 cProjectionTexMtx1;
    vec4 cProjParams;
    mat4 cCascadeMtx0;
    mat4 cCascadeMtx1;
    mat4 cCascadeMtx2;
    mat4 cCascadeMtx3;

    vec4 Unk0;
    vec4 Unk1;
    vec4 Unk2;
    vec4 Unk3;
    vec4 Unk4;
    vec4 Unk5;

    vec4 cCubemapHDR;
}context;

#endif

struct Fog {
    vec4 Color;
    vec3 Direction;
    float Start;
    float End;
    float Damp;
    float Padding1;
    float Padding2;
};

layout (std140, binding = 1)  uniform Shape //@ id="gsys_shape" size="112" type="shape"
{
    mat3x4 cTransform;
	vec4 cParams; //x = skin count
	vec4 Unknown; 
	vec4 cAreaParams; //indices for what area to use for the cubemap (used by moving objects) 
}shape;

layout (std140, binding = 2) uniform GsysEnvironment //@ id="gsys_environment" size="464"
{
    vec4 cAmbientColor;
    vec4 cHemiSkyColor;
    vec4 cHemiGroundColor;
    vec4 cHemiDirection;

    vec4 cLightDirection0;
    vec4 cLightColor;
    vec4 cLightSpecColor;

    vec4 cLightDirection1;
    vec4 cLightColor1;
    vec4 cLightSpecColor1;

    Fog fog[4]; //env data[10] +
}environment;

layout (std140, binding = 3) uniform GsysMaterial //@ id="gsys_material" type="material"
{
    mat2x4 tex_mtx0; //@ default_value="1 -0 0 1 0 0 0 0"
    mat2x4 tex_mtx1; //@ default_value="1 -0 0 1 0 0 0 0"
    mat2x4 tex_mtx2; //@ default_value="1 -0 0 1 0 0 0 0"
    vec4 gsys_depth_silhoutte_color; //@ default_value="1 1 1 1"
    vec4 gsys_outline_color; //@ default_value="1 1 1 1"
    float padding_112; //@ default_value="0"
    float bloom_intensity; //@ default_value="1"
    float gsys_outline_width; //@ default_value="1"
    float gsys_alpha_threshold; //@ default_value="0.5"
    vec4 gsys_area_env_data0; //@ default_value="0 0 0 0"
    vec4 gsys_area_env_data1; //@ default_value="0 0 0 0"
    vec4 gsys_sssss_color; //@ default_value="1 1 1 1"
    vec4 gsys_bake_st0; //@ default_value="1 1 0 0"
    vec4 gsys_bake_st1; //@ default_value="1 1 0 0"
    vec4 gsys_bake_light_scale; //@ default_value="1 1 1 0"
    vec4 gsys_bake_light_scale1; //@ default_value="1 1 1 0"
    vec4 gsys_bake_light_scale2; //@ default_value="1 1 1 0"
    vec3 albedo_tex_color; //@ default_value="1 1 1"
    float shadow_density; //@ default_value="1"
    vec4 multi_tex_reg0; //@ default_value="1 1 1 1"
    vec4 multi_tex_reg1; //@ default_value="1 1 1 1"
    vec4 multi_tex_reg2; //@ default_value="1 1 1 1"
    vec4 multi_tex_param0; //@ default_value="0 0 0 0"
    vec3 edge_light_color; //@ default_value="1 1 1"
    float edge_light_intensity; //@ default_value="0.5"
    vec2 indirect_mag; //@ default_value="1 1"
    float indirect_magBX; //@ default_value="1"
    float indirect_magBY; //@ default_value="1"
    vec3 specular_color; //@ default_value="1 1 1"
    float specular_intensity; //@ default_value="1"
    float specular_roughness; //@ default_value="0"
    float specular_fresnel_i; //@ default_value="0.5"
    float specular_fresnel_s; //@ default_value="0"
    float specular_fresnel_m; //@ default_value="0"
    vec3 emission_color; //@ default_value="1 1 1"
    float emission_intensity; //@ default_value="0.1"
    float soft_edge_dist_inv; //@ default_value="0.1"
    float silhoutte_depth; //@ default_value="0.01"
    float refraction_intensity; //@ default_value="0"
    float normal_map_weight; //@ default_value="0.5"
    vec3 silhoutte_depth_color; //@ default_value="1 1 1"
    float silhoutte_depth_contrast; //@ default_value="1"
    vec4 mii_modulate_type; //@ default_value="0 0 0 0"
    vec4 mii_constant_color0; //@ default_value="0 0 0 0"
    vec4 mii_constant_color1; //@ default_value="0 0 0 0"
    vec4 mii_constant_color2; //@ default_value="0 0 0 0"
    vec3 gsys_mii_skin_color; //@ default_value="1 1 1"
    float mii_hair_specular_intensity; //@ default_value="1"
    vec3 gsys_mii_favorite_color; //@ default_value="1 1 1"
    float post_multi_texture; //@ default_value="0"
    vec3 fog_emission_color; //@ default_value="1 1 1"
    float fog_emission_intensity; //@ default_value="1"
    float fog_emission_effect; //@ default_value="1"
    float fog_edge_power; //@ default_value="1"
    float fog_edge_width; //@ default_value="1"
    float gsys_alpha_test_ref_value; //@ default_value="0.5"
    vec4 fog_edge_color; //@ default_value="1 1 1 1"
    float edge_light_sharpness; //@ default_value="0.3"
    float edge_light_rim_i; //@ default_value="1"
    float d_shadow_bake_l_cancel_rateX; //@ default_value="0.5"
    float d_shadow_bake_l_cancel_rateY; //@ default_value="0"
    vec3 gsys_i_color0; //@ default_value="1 1 1"
    float gsys_i_color_ratio0; //@ default_value="0"
    vec3 gsys_i_color0_b; //@ default_value="0 0 0"
    float gsys_edge_ratio0; //@ default_value="0"
    vec3 gsys_edge_color0; //@ default_value="7.500 5.0 2.500"
    float gsys_edge_width0; //@ default_value="0.5"
    float game_edge_pow; //@ default_value="20"
    float edge_alpha_scale; //@ default_value="1"
    float edge_alpha_width; //@ default_value="0.5"
    float edge_alpha_pow; //@ default_value="5"
    float transparency; //@ default_value="0.5"
    float alphat_out_start; //@ default_value="10"
    float alphat_out_end; //@ default_value="100"
    float gsys_area_env_index_diffuse; //@ default_value="0"
    vec3 gsys_point_light_color; //@ default_value="1 1 1"
    float ao_density; //@ default_value="1"
    vec3 transmit_color; //@ default_value="1 1 1"
    float transmit_intensity; //@ default_value="1"
    float edge_light_vc_intensity; //@ default_value="1"
    float specular_aniso_power; //@ default_value="10"
    float transmit_shadow_intensity; //@ default_value="1"
    float light_pre_pass_intensity; //@ default_value="1"
    vec3 shiny_specular_color; //@ default_value="1 1 1"
    float gsys_bake_opacity; //@ default_value="1"
    float shiny_specular_intensity; //@ default_value="256"
    float shiny_specular_sharpness; //@ default_value="160000"
    float shiny_specular_fresnel; //@ default_value="16"
    float fresnel_look_depend_factor; //@ default_value="1"
    vec3 decal_trail_color; //@ default_value="0 0 0"
    float decal_trail_intensity; //@ default_value="1"
    vec4 gsys_xlu_zprepass_alpha; //@ default_value="1 1 1 1"
    vec3 screen_fake_scale_factor; //@ default_value="0 0 0"
    float screen_fake_scale_begin_ratio; //@ default_value="0.25"
    vec4 cDebugFlags; //@ default_value="0 0 0 0"
}mat;

layout (std140, binding = 4) uniform SceneMaterial //@ id="gsys_scene_material" type="scene" size="96"
{
    vec4 shadow_color; //@ id="shadow_color" default_value="0 0 0 0"
    vec4 ao_color; //@ id="ao_color" default_value="0 0 0 0"
    vec4 lighting; //@ id="lighting" default_value="1 1 1 1"
    vec4 lighting_specular; //@ id="lighting_specular" default_value="1 1 1 1"
    vec4 light_prepass_param; //@ id="light_prepass_param" default_value="1 1 1 1"
    vec4 exposure; //@ id="exposure" default_value="1 1 1 1"
}scene_material;

layout (std140, binding = 5) uniform GsysSkeleton //@ id="gsys_skeleton" size="48" type="skeleton"
{
    mat3x4 cBoneMatrices[MAX_BONE_COUNT];
}boneMatrices;