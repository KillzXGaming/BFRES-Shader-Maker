
//@ render_info="map_category" default="None" type="string" group="Render Info" order="-1"
//@ render_info="proc_texture_2d_type" default="PerlinFbm" type="string" group="Render Info" order="-1"
//@ render_info="proc_texture_3d_type" default="Caustics" type="string" group="Render Info" order="-1"
//@ render_info="display_face" default="front" type="string" group="Render Info" order="-1"
//@ render_info="forward_xlu" default="Blend" type="string" group="Render Info" order="-1"
//@ render_info="enable_xlu_zprepass" default="false" type="string" group="Render Info" order="-1"
//@ render_info="enable_color_blend_custom" default="false" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_const_color" default="1 1 1 1" type="float" group="Render Info" order="-1"
//@ render_info="color_blend_rgb_op" default="add" choices="add, src_minus_dst, min,max, dst_minus_src" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_rgb_src_func" default="src_alpha" choices="zero,one,src_color,one_minus_src_color,src_alpha,one_minus_src_alpha,dst_alpha,one_minus_dst_alpha,const_color,one_minus_const_color,const_alpha,one_minus_const_alpha,src_alpha_saturate" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_rgb_dst_func" default="one_minus_src_alpha" choices="zero,one,src_color,one_minus_src_color,src_alpha,one_minus_src_alpha,dst_alpha,one_minus_dst_alpha,const_color,one_minus_const_color,const_alpha,one_minus_const_alpha,src_alpha_saturate" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_alpha_op" default="add" choices="add, src_minus_dst, min,max, dst_minus_src" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_alpha_src_func" default="one" choices="zero,one,src_color,one_minus_src_color,src_alpha,one_minus_src_alpha,dst_alpha,one_minus_dst_alpha,const_color,one_minus_const_color,const_alpha,one_minus_const_alpha,src_alpha_saturate" type="string" group="Render Info" order="-1"
//@ render_info="color_blend_alpha_dst_func" default="zero" choices="zero,one,src_color,one_minus_src_color,src_alpha,one_minus_src_alpha,dst_alpha,one_minus_dst_alpha,const_color,one_minus_const_color,const_alpha,one_minus_const_alpha,src_alpha_saturate" type="string" group="Render Info" order="-1"
//@ render_info="deferred_xlu" default="BcNrmLbuf" type="string" group="Render Info" order="-1"
//@ render_info="enable_depth_test" default="true" type="string" group="Render Info" order="-1"
//@ render_info="enable_depth_write" default="true" type="string" group="Render Info" order="-1"
//@ render_info="depth_test_func" default="Lequal" type="string" group="Render Info" order="-1"
//@ render_info="draw_priority" default="0" type="int" group="Render Info" order="-1"
//@ render_info="disable_z_pre_pass" default="false" type="string" group="Render Info" order="-1"
//@ render_info="enable_depthshadow" default="true" type="string" group="Render Info" order="-1"
//@ render_info="enable_static_depthshadow" default="true" type="string" group="Render Info" order="-1"
//@ render_info="enable_polygon_offset" default="false" type="string" group="Render Info" order="-1"
//@ render_info="polygon_offset_value" default="-1" type="float" group="Render Info" order="-1"

//-----------------------------------
//Static Options
//-----------------------------------
#define enable_compose_ripple_emission false //@
#define o_base_color 10 //@ choices="10 15 80 81 82 83 84 85 100 101 160 170"
#define o_normal 30 //@ choices="20 30 50 51 52 53 54 80 81 82 83 84 85 100 101"
#define o_roughness 116 //@ choices="10 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 110 111 112 113 115 116 160 170"
#define o_metalness 115 //@ choices="10 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 110 111 112 113 115 116 160 170"
#define o_alpha 116 //@ choices="10 15 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 100 101 110 111 112 113 115 116 160 170"
#define roughness_component 30 //@ choices="30 40 50 60 70 80 90 100"
#define metalness_component 30 //@ choices="30 40 50 60 70 80 90 100"
#define alpha_component 60 //@ choices="30 40 50 60"
#define displacement_component 10 //@ choices="10 30"
#define enable_displacement false //@
#define displacement_fuv_selector 10 //@ choices="10 11 12 13 14"
#define displacement_mul_vtx_color false //@
#define displacement_mul_vtx_alpha false //@
#define displacement1_component 10 //@ choices="10 30"
#define enable_displacement1 false //@
#define displacement1_fuv_selector 10 //@ choices="10 11 12 13 14"
#define displacement1_mul_vtx_color false //@
#define displacement1_mul_vtx_alpha false //@
#define enable_emission false //@
#define o_emission 50 //@ choices="10 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 100 101 160 170"
#define emission_component 10 //@ choices="10 30 40 50 60"
#define emission_type 0 //@ choices="0 2"
#define emission_scale_type 0 //@ choices="0 1 2 3 4 5 6 7"
#define enable_alphamask false //@
#define alphamask_type 10 //@ choices="10 20"
#define alpha_test_func 60 //@ choices="0 10 20 30 40 50 60 70"
#define enable_transparent false //@
#define transparent_type 10 //@ choices="10 20 25 30 40 50"
#define o_refract_color 50 //@ choices="10 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 160 170"
#define o_refract_rate 115 //@ choices="50 51 52 53 54 60 61 62 63 80 81 82 83 84 85 110 111 112 113 115 116 160 170"
#define refract_rate_component 30 //@ choices="30 40 50 60"
#define o_refract_eta 115 //@ choices="50 51 52 53 54 60 61 62 63 110 111 112 113 115 160 170"
#define refract_eta_component 30 //@ choices="30 40 50 60"
#define o_transparent_tex 50 //@ choices="10 50 51 52 53 54 160 170"
#define transparent_tex_type 10 //@ choices="10 15 20 25"
#define enable_indirect_dist_correct false //@
#define o_metal_flake_power 116 //@ choices="50 51 52 53 54 60 61 62 63 110 111 112 113 115 116 160 170"
#define metal_flake_power_component 30 //@ choices="30 40 50 60 70 80 90 100"
#define metal_flake_emission_scale_type 0 //@ choices="0 1 2 3 4 5 6 7"
#define enable_structural_color false //@
#define o_structural_eta 115 //@ choices="50 51 52 53 54 60 61 62 63 110 111 112 113 115 160 170"
#define structural_eta_component 30 //@ choices="30 40 50 60"
#define enable_cloth_nov false //@
#define o_cloth_mask_map 116 //@ choices="10 50 51 52 53 54 116 160 170"
#define cloth_mask_component 30 //@ choices="30 40 50 60 70 80 90 100"
#define o_cloth_map 115 //@ choices="10 50 51 52 53 54 80 81 82 83 84 85 60 61 62 63 110 111 112 113 115 116 160 170"
#define o_cloth_emission_map 115 //@ choices="10 50 51 52 53 54 80 81 82 83 84 85 60 61 62 63 110 111 112 113 115 116 160 170"
#define is_cloth_nov_reverse false //@
#define is_cloth_nov_use_rnd_noise_mask false //@
#define cloth_nov_emission_scale_type 3 //@ choices="0 1 2 3 4 5 6 7"
#define enable_sss false //@
#define o_sss 50 //@ choices="10 50 51 52 53 54 80 81 82 83 84 85 116 160 170"
#define sss_component 30 //@ choices="30 40 50 60"
#define enable_ao false //@
#define o_ao 50 //@ choices="50 51 52 53 54 160 170"
#define ao_component 30 //@ choices="30 40 50 60"
#define enable_mirror false //@
#define enable_translucent false //@
#define enable_flow0 false //@
#define flow0_flow_map 50 //@ choices="50 51 52 53 54"
#define flow0_flow_type 20 //@ choices="10 20 50 51 52 53 54"
#define enable_flow1 false //@
#define flow1_flow_map 50 //@ choices="50 51 52 53 54"
#define flow1_flow_type 20 //@ choices="10 20 50 51 52 53 54"
#define enable_flow2 false //@
#define flow2_flow_map 50 //@ choices="50 51 52 53 54"
#define flow2_flow_type 20 //@ choices="10 20 50 51 52 53 54"
#define enable_base_color true //@
#define enable_base_color_mul_color false //@
#define base_color_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_normal false //@
#define disable_decode_normalmap false //@
#define normal_fuv_selector 10 //@ choices="10 11 12 13 20 21 50 51 52 53"
#define enable_uniform0 false //@
#define enable_uniform0_mul_color false //@
#define enable_uniform0_mul_vtxcolor false //@
#define enable_uniform0_roughness_lod false //@
#define uniform0_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_uniform1 false //@
#define enable_uniform1_mul_color false //@
#define enable_uniform1_mul_vtxcolor false //@
#define enable_uniform1_roughness_lod false //@
#define uniform1_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_uniform2 false //@
#define enable_uniform2_mul_color false //@
#define enable_uniform2_mul_vtxcolor false //@
#define enable_uniform2_roughness_lod false //@
#define uniform2_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_uniform3 false //@
#define enable_uniform3_mul_color false //@
#define enable_uniform3_mul_vtxcolor false //@
#define enable_uniform3_roughness_lod false //@
#define uniform3_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_uniform4 false //@
#define enable_uniform4_mul_color false //@
#define enable_uniform4_mul_vtxcolor false //@
#define enable_uniform4_roughness_lod false //@
#define uniform4_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define enable_proc_texture_2d false //@
#define enable_proc_texture_2d_mul_color false //@
#define enable_proc_texture_2d_mul_vtxcolor false //@
#define enable_proc_texture_2d_roughness_lod false //@
#define proc_texture_2d_fuv_selector 10 //@ choices="10 11 12 13 20 21 30 50 51 52 53"
#define proc_texture_2d_component 0 //@ choices="0 30 40 50 60 70 80 90 100"
#define enable_proc_texture_3d false //@
#define enable_proc_texture_3d_mul_color false //@
#define enable_proc_texture_3d_mul_vtxcolor false //@
#define enable_proc_texture_3d_roughness_lod false //@
#define proc_texture_3d_fuv_selector 60 //@ choices="60 61"
#define proc_texture_3d_fuv_offset 0 //@ choices="0 60 61 62 63"
#define proc_texture_3d_component 0 //@ choices="0 30 70"
#define enable_indirect0 false //@
#define indirect0_src_map 50 //@ choices="50 51 52 53 54 80 81 82 83 84 85 140"
#define indirect0_tgt_uv 10 //@ choices="10 11 12 13"
#define enable_indirect1 false //@
#define indirect1_src_map 50 //@ choices="50 51 52 53 54 80 81 82 83 84 85 140"
#define indirect1_tgt_uv 10 //@ choices="10 11 12 13"
#define enable_blend0 false //@
#define blend0_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 100 101 115 116 160 170"
#define blend0_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend0_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 100 101 115 116 160 170"
#define blend0_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend0_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend0_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend0_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend0_post 0 //@ choices="0 10 20 30 40 50"
#define blend0_cof_map 50 //@ choices="10 50 51 52 53 54 160 170"
#define blend0_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_blend1 false //@
#define blend1_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 100 101 115 116 160 170"
#define blend1_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend1_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 100 101 115 116 160 170"
#define blend1_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend1_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend1_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend1_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend1_post 0 //@ choices="0 10 20 30 40 50"
#define blend1_cof_map 50 //@ choices="10 50 51 52 53 54 80 160 170"
#define blend1_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_blend2 false //@
#define blend2_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 100 101 115 116 160 170"
#define blend2_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend2_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 100 101 115 116 160 170"
#define blend2_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend2_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend2_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend2_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend2_post 0 //@ choices="0 10 20 30 40 50"
#define blend2_cof_map 50 //@ choices="10 50 51 52 53 54 80 81 160 170"
#define blend2_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_blend3 false //@
#define blend3_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 100 101 115 116 160 170"
#define blend3_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend3_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 100 101 115 116 160 170"
#define blend3_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend3_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend3_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend3_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend3_post 0 //@ choices="0 10 20 30 40 50"
#define blend3_cof_map 50 //@ choices="10 50 51 52 53 54 80 81 82 160 170"
#define blend3_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_blend4 false //@
#define blend4_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 83 100 101 115 116 160 170"
#define blend4_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend4_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 83 100 101 115 116 160 170"
#define blend4_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend4_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend4_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend4_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend4_post 0 //@ choices="0 10 20 30 40 50"
#define blend4_cof_map 50 //@ choices="10 50 51 52 53 54 80 81 82 160 170"
#define blend4_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_blend5 false //@
#define blend5_src 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 83 84 100 101 115 116 160 170"
#define blend5_src_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend5_dst 10 //@ choices="10 15 20 30 50 51 52 53 54 60 61 62 63 70 71 72 73 74 78 80 81 82 83 84 100 101 115 116 160 170"
#define blend5_dst_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend5_cof 10 //@ choices="10 20 30 31 32 33 60 61 62 63 115 116"
#define blend5_cof_ch 10 //@ choices="10 11 20 21 30 31 40 41 50 51"
#define blend5_eq 0 //@ choices="0 1 2 3 4 5 6 7 8"
#define blend5_post 0 //@ choices="0 10 20 30 40 50"
#define blend5_cof_map 50 //@ choices="10 50 51 52 53 54 80 81 82 160 170"
#define blend5_indirect_map 50 //@ choices="10 20 50 51 52 53 54"
#define enable_gbuf_fetch_offset false //@
#define sphere_const_color0 0 //@ choices="0 1 2"
#define sphere_const_color1 0 //@ choices="0 1 2"
#define sphere_const_color2 0 //@ choices="0 1 2"
#define sphere_const_color3 0 //@ choices="0 1 2"
#define is_apply_irradiance_pixel false //@
#define is_no_dir_light false //@
#define is_use_texture_bias false //@
#define is_use_forward_ggx_specular false //@
#define enable_material_light true //@
#define enable_material_sphere_light false //@
#define enable_fuv0 true //@
#define enable_fuv1 false //@
#define enable_fuv2 false //@
#define enable_fuv3 false //@
#define fuv0_selector 0 //@ choices="0 1 2 3 10"
#define fuv1_selector 0 //@ choices="0 1 2 3 10"
#define fuv2_selector 0 //@ choices="0 1 2 3 10"
#define fuv3_selector 0 //@ choices="0 1 2 3 10"
#define fuv0_mtx 0 //@ choices="0 1 2 3 4"
#define fuv1_mtx 0 //@ choices="0 1 2 3 4"
#define fuv2_mtx 0 //@ choices="0 1 2 3 4"
#define fuv3_mtx 0 //@ choices="0 1 2 3 4"
#define cRenderType 0 //@ choices="0 1 3"
#define is_render_cubemap false //@
#define is_use_back_face_lighting false //@
#define is_use_forward_shadow_buffer false //@
#define is_use_linear_depth false //@
#define is_use_decal false //@
#define vtxcolor_type -1 //@ choices="-1 0 1 2 3"
#define enable_clamp_lbuf false //@
#define enable_constant_output false //@
#define constant_src 10 //@ choices="10 15 50 51 52 53 54 60 61 62 63 80 81 82 83 84 85"
#define enable_blend_tangent false //@
#define tangent_blend_cof 15 //@ choices="10 15 50 51 52 53 54"
#define tangent_blend_component 60 //@ choices="30 40 50 60"
#define enable_material_lod false //@
#define enable_lod_roughness_fix false //@
#define enable_lod_metalness_fix false //@
#define cSkinWeightNum 0 //@ choices="0 1 2 3 4" is_skin_count="true"
//-----------------------------------
//Dynamic Options (SMO has 4 always used variants, 2 for prog texture, 2 for motion on/off)
//-----------------------------------
#define enable_compose_footprint 0 //@ branch="dynamic" choices="0 1 2"
#define enable_compose_capture false //@ branch="dynamic"
#define enable_add_stain_proc_texture_3d false //@ branch="dynamic"
#define compose_prog_texture0 false //@ branch="dynamic" flags="compile_all_coices"
#define enable_parallax_cubemap false //@ branch="dynamic" flags="compile_all_coices"
#define is_output_motion_vec true //@ branch="dynamic" flags="compile_all_coices"
#define material_lod_level false //@ branch="dynamic"
#define system_id 0 //@ branch="dynamic" choices="0"

// vtxcolor_type
#define VTX_COLOR_TYPE_NONE -1
#define VTX_COLOR_TYPE_DIFFUSE 0
#define VTX_COLOR_TYPE_IRRADIANCE 1
#define VTX_COLOR_TYPE_EMISSION 2
#define VTX_COLOR_TYPE_DIFFUSE_BLEND 3

// transparent_type
#define TRANS_TYPE_CUBEMAP_ROUGHNESS 10 //uses cTexCubeMapRoughness
#define TRANS_TYPE_IND_FBO 20 //uses cFrameBufferTex
#define TRANS_TYPE_IND_FBO_DEPTH 25 //uses cFrameBufferTex
#define TRANS_TYPE_TEX 30 //Multi purpose by transparent_tex_type
#define TRANS_TYPE_CUBEMAP_GEM0 40 
#define TRANS_TYPE_CUBEMAP_GEM1 50 

// transparent_tex_type
#define TRANS_TEX_TYPE_BASE_COLOR 10
#define TRANS_TEX_TYPE_DIFFUSE 15 //after diffuse pbr calc
#define TRANS_TEX_TYPE_DIFFUSE_IRRADIANCE 20 //after diffuse pbr calc. Same as diffuse calc, but * irradiance
#define TRANS_TEX_TYPE_METAL_FLAKE 25

// cRenderType
#define RENDER_TYPE_DEFERRED_OPAQUE 0
#define RENDER_TYPE_DEFERRED_XLU 1
#define RENDER_TYPE_FORWARD 3 //used for translucent types

//The UV layer or method to use
#define FUV_SELECT_UV0 10
#define FUV_SELECT_UV1 11
#define FUV_SELECT_UV2 12
#define FUV_SELECT_UV3 13
#define FUV_SELECT_IND0 20
#define FUV_SELECT_IND1 21
#define FUV_SELECT_SPHERE 30
#define FUV_SELECT_PROJ 50
#define FUV_SELECT_PROJ_MTX0 51 //proj_mtx# from model additional info block
#define FUV_SELECT_PROJ_MTX1 52
#define FUV_SELECT_PROJ_MTX2 53

// Custom
#define DEBUG_VISUALIZER false

const int MAX_BONE_COUNT = 160;

layout(std140, binding = 10)  uniform cShaderOption //@ id="cShaderOption" size="0" type="option"
{
  vec4 data[4096];
}shaderoption;

layout(std140, binding = 2)  uniform cMat //@ id="cMat" size="656" type="material"
{
    vec4 const_color0; //@ default_value="1 1 1 1" group="Constants" type="color"
    vec4 const_color1; //@ default_value="1 1 1 1" group="Constants" type="color"
    vec4 const_color2; //@ default_value="1 1 1 1" group="Constants" type="color"
    vec4 const_color3; //@ default_value="1 1 1 1" group="Constants" type="color"
    float const_single0; //@ default_value="0" group="Constants"
    float const_single1; //@ default_value="0" group="Constants"
    float const_single2; //@ default_value="0" group="Constants"
    float const_single3; //@ default_value="0" group="Constants"
    vec4 base_color_mul_color; //@ default_value="1 1 1 1" group="Base Color" type="color"
    vec4 uniform0_mul_color; //@ default_value="1 1 1 1" group="Uniform 0" type="color"
    vec4 uniform1_mul_color; //@ default_value="1 1 1 1" group="Uniform 1" type="color"
    vec4 uniform2_mul_color; //@ default_value="1 1 1 1" group="Uniform 2" type="color"
    vec4 uniform3_mul_color; //@ default_value="1 1 1 1" group="Uniform 3" type="color"
    vec4 uniform4_mul_color; //@ default_value="1 1 1 1" group="Uniform 4" type="color"
    vec4 proc_texture_2d_mul_color; //@ default_value="1 1 1 1" group="Proc Texture" type="color"
    vec4 proc_texture_3d_mul_color; //@ default_value="1 1 1 1" group="Proc Texture" type="color"
    mat2x4 tex_mtx0; //@ default_value="1 -0 0 1 0 0 0 0" group="UV"
    mat2x4 tex_mtx1; //@ default_value="1 -0 0 1 0 0 0 0" group="UV"
    mat2x4 tex_mtx2; //@ default_value="1 -0 0 1 0 0 0 0" group="UV"
    mat2x4 tex_mtx3; //@ default_value="1 -0 0 1 0 0 0 0" group="UV"
    float displacement_scale; //@ default_value="1" group="Displacement 0"
    vec3 displacement1_scale; //@ default_value="1 1 1" group="Displacement 1"
    vec4 displacement_color; //@ default_value="1 1 1 1" group="Displacement 0" type="color"
    vec4 displacement1_color; //@ default_value="1 1 1 1" group="Displacement 1" type="color"
    float wrap_coef; //@ default_value="0" group="Subsurface Scattering"
    float refract_thickness; //@ default_value="0" group="Refract"
    vec2 indirect0_scale; //@ default_value="1 1" group="Indirect 0"
    vec2 indirect1_scale; //@ default_value="1 1" group="Indirect 1"
    float alpha_test_value; //@ default_value="0.5" group="Transparent"
    float force_roughness; //@ default_value="1" group="PBR"
    float sphere_rate_color0; //@ default_value="1" group="Constants" type="color"
    float sphere_rate_color1; //@ default_value="1" group="Constants" type="color"
    float sphere_rate_color2; //@ default_value="1" group="Constants" type="color"
    float sphere_rate_color3; //@ default_value="1" group="Constants" type="color"
    mat4 mirror_view_proj; //@ default_value="1 0 0 0 0 0 1 0 0 0 0 0 0 1 0 0"
    float decal_range; //@ default_value="1E-05"
    float gbuf_fetch_offset; //@ default_value="0.001"
    float translucence_sharpness; //@ default_value="2"
    float translucence_sharpness_strength; //@ default_value="1.17"
    float translucence_factor; //@ default_value="0.8"
    float translucence_silhouette_stress; //@ default_value="0"
    float indirect_depth_scale; //@ default_value="300"
    float cloth_nov_peak_pos0; //@ default_value="0.75" group="Cloth"
    float cloth_nov_peak_pow0; //@ default_value="2" group="Cloth"
    float cloth_nov_peak_intensity0; //@ default_value="0.5" group="Cloth"
    float cloth_nov_tone_pow0; //@ default_value="1" group="Cloth"
    float cloth_nov_slope0; //@ default_value="1" group="Cloth"
    float cloth_nov_emission_scale0; //@ default_value="0" group="Cloth"
    float cloth_nov_noise_mask_scale0; //@ default_value="0" group="Cloth"
    vec2 padding; //@ default_value="0"
    vec4 proc_texture_3d_scale; //@ default_value="0.01 0.01 0.01 0"
    vec4 flow0_param; //@ default_value="1 1 1 1"
    vec4 ripple_emission_color; //@ default_value="1 1 1 1"
    vec4 hack_color; //@ default_value="1 1 1 1" type="color"
    vec4 stain_color; //@ default_value="1 1 1 1" type="color"
    float stain_uv_scale; //@ default_value="0.018"
    float stain_rate; //@ default_value="1"
    float material_lod_roughness; //@ default_value="1"
    float material_lod_metalness; //@ default_value="0"
}mat;

layout(std140, binding = 6)  uniform cModelAdditionalInfo //@ id="cModelAdditionalInfo" size="496"
{
    float model_alpha_mask; //@ 
    float normal_axis_x_scale; //@ 
    vec2 uv_offset; //@ 
    mat4 proj_mtx0; //@ 
    mat4 proj_mtx1; //@ 
    mat4 proj_mtx2; //@ 
    mat4 proj_mtx3; //@ 
    vec4 prog_constant0; //@ 
    vec4 prog_constant1; //@ 
}modelInfo;

layout(std140, binding = 1)  uniform cMdlEnvView //@ id="cMdlEnvView" size="4800"
{
    mat3x4 cView;
    mat3x4 cViewInv;
    mat4 cViewProj;
    mat3x4 cViewProjInv;
    mat4 cProjInv;
    mat3x4 cProjInvNoPos;
    vec4 Exposure; //[20]
    vec4 Dir;
    vec4 ZNearFar; //[22] //Near, Far, Far - Near, 1 / (Far - Near)
    vec2 TanFov;
    vec2 ProjOffset;
    vec4 ScreenSize;
    vec4 CameraPos;

    mat4 bayer_mtx;
} mdlEnvView;

layout(std140, binding = 4)  uniform cStaticDepthShadow //@ id="cStaticDepthShadow" size="224"
{
  vec4 data[4096];
}staticdepthshadow;

layout(std140, binding = 7)  uniform cHdrTranslate //@ id="cHdrTranslate" size="16"
{
    float Power;
    float Range;
}hdr;

layout(std140, binding = 5)  uniform cDepthShadow //@ id="cDepthShadow" size="288"
{
  vec4 data[4096];
}depthshadow;

layout(std140, binding = 3)  uniform Skel //@ id="skel" size="9216" type="skeleton"
{
    mat3x4 cBoneMatrices[MAX_BONE_COUNT];
}boneMatrices;

layout(std140, binding = 6)  uniform Skel_prev //@ id="skel_prev" size="9216"
{
    mat3x4 cBoneMatrices[MAX_BONE_COUNT];
}skel_prev;

layout(std140, binding = 4)  uniform Shape //@ id="shape" size="64" type="shape"
{
    mat3x4 cTransform;
} shape;

layout(std140, binding = 8)  uniform Shape_prev //@ id="shape_prev" size="64"
{
    mat3x4 cTransform;
}shape_prev;

layout(std140, binding = 9)  uniform cLightEnv //@ id="cLightEnv" size="32"
{
  vec4 data[4096];
}lightenv;

