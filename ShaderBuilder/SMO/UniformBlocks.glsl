
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

const int MAX_BONE_COUNT = 160;

layout (binding = 8, std140) uniform HDRTranslate //@ id="cHdrTranslate" size="16"
{
    float Power;
    float Range;
}hdr;

layout (binding = 7, std140) uniform ModelAdditionalInfo //@ id="cModelAdditionalInfo" size="496"
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

layout (binding = 5, std140) uniform _Shp //@ id="shape" size="64" type="shape"
{
    mat3x4 cTransform;
} shape;

layout (binding = 4, std140) uniform MdlMtx //@ id="skel" size="9216" type="skeleton"
{
    mat3x4 cBoneMatrices[MAX_BONE_COUNT];
};

layout (binding = 3, std140) uniform Material //@ id="cMat" type="material"
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

layout (binding = 2, std140) uniform MdlEnvView //@ id="cMdlEnvView" size="4800"
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