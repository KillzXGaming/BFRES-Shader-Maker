#version 450 core

#include "UniformBlocks.glsl"

#if (BLENDER_RENDER == 0)
layout (location = 0) in vec3 vPosition; //@ id="_p0"
layout (location = 1) in vec3 vNormal; //@ id="_n0"
layout (location = 2) in vec4 vTangent; //@ id="_t0"
layout (location = 4) in vec4 vBoneWeight; //@ id="_w0"
layout (location = 6) in ivec4 vBoneIndices; //@ id="_i0"
layout (location = 8) in vec2 vTexCoords0; //@ id="_u0"
layout (location = 9) in vec2 vTexCoords1;//@ id="_u1"
layout (location = 10) in vec2 vTexCoords2;//@ id="_u2"
layout (location = 11) in vec2 vTexCoords3;//@ id="_u3"
layout (location = 12) in vec4 vColor; //@ id="_c0"

layout (location = 0) out vec4 fTexCoords0;
layout (location = 1) out vec4 fFog;
layout (location = 2) out vec4 fTangents;
layout (location = 3) out vec4 fNormals;
layout (location = 4) out vec4 fTexCoordsBake; // fTexCoords1; xy: Shadow/AO, zw: Lightmap
layout (location = 5) out vec4 fProjCoords;
layout (location = 6) out vec4 fViewDirection;
layout (location = 7) out vec4 fScreenCoords;
layout (location = 9) out vec4 fVtxColor0;
layout (location = 10) out vec4 fTexCoords23;
layout (location = 11) out vec4 fViewPos;
layout (location = 12) out vec4 fCascadeInfo;
#endif

#include "WiiUCommon.glsl"

vec4 calc_fog(vec3 pos, int idx)
{
	Fog fog = environment.fog[idx];
	float z = dot(fog.Direction.xyz, pos.xyz);

	vec4 fog_output = vec4(fog.Color.xyz, 1.0);
    float a = clamp(z * fog.End + fog.Start, 0.0, 1.0);
	fog_output.a = a * a * fog.Color.a;

	return fog_output;
}

vec4 skin(vec3 pos, ivec4 index)
{
    vec4 newPosition = vec4(pos.xyz, 1.0);

    if (gsys_weight == 0 && !gsys_invalidate_world_srt)
        newPosition = vec4(pos, 1.0) * mat4(shape.cTransform);
    if (gsys_weight == 1)
        newPosition = vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.x]);

    if (gsys_weight > 1)
        newPosition = vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.x]) * vBoneWeight.x;
    if (gsys_weight >= 2)
        newPosition += vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.y]) * vBoneWeight.y;
    if (gsys_weight >= 3)
        newPosition += vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.z]) * vBoneWeight.z;
    if (gsys_weight >= 4)
        newPosition += vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.w]) * vBoneWeight.w;
        
    return newPosition;
}

vec3 skinNormal(vec3 nr, ivec4 index)
{
    vec3 newNormal = nr;

    if (gsys_weight == 0 && !gsys_invalidate_world_srt) // gsys_invalidate_world_srt disables shape transform
        newNormal = nr * mat3(shape.cTransform);
    if (gsys_weight == 1)
        newNormal = nr * mat3(boneMatrices.cBoneMatrices[index.x]);

    if (gsys_weight >  1)
        newNormal =  nr * mat3(boneMatrices.cBoneMatrices[index.x]) * vBoneWeight.x;
    if (gsys_weight >= 2)
        newNormal += nr *  mat3(boneMatrices.cBoneMatrices[index.y]) * vBoneWeight.y;
    if (gsys_weight >= 3)
        newNormal += nr * mat3(boneMatrices.cBoneMatrices[index.z]) * vBoneWeight.z;
    if (gsys_weight >= 4)
        newNormal += nr * mat3(boneMatrices.cBoneMatrices[index.w]) * vBoneWeight.w;
    
    return newNormal;
}

vec2 CalcScaleBias(in vec2 uv, in vec4 sb) {

	// This took me forever to figure out, the Blender UV origin is different so we need to flip things
	#if (BLENDER_RENDER == 1)
    vec2 uvFlipped = vec2(uv.x, 1.0 - uv.y);
    return uvFlipped.xy * vec2(sb.x, -sb.y) + vec2(sb.z, 1.0 - sb.w);
	#endif
	
    return uv.xy * sb.xy + sb.zw;
}
 
void main()
{	
	ivec4 bone_index = vBoneIndices;
	
	// POSITION -------------------------------------------------------------------------------------------------------------
	vec4 position = skin(vPosition.xyz, bone_index);
    #if (BLENDER_RENDER == 1)
    position = vec4(vPosition.xyz, 1.0) * mat4(shape.cTransform);
    #endif

    // BILLBOARD (e.g. Item Box question mark) ------------------------------------------------------------------------------
    if (enable_vertex_billboard)
    {
        vec3 billboardPos = vec3(shape.cTransform[0].w, shape.cTransform[1].w, shape.cTransform[2].w);
        vec3 viewPos = vec3(context.cView[0].w, context.cView[1].w, context.cView[2].w);
        vec3 toCamera = normalize(viewPos - billboardPos);
        mat3 billboardMatrix = mat3(
            context.cView[0].xyz, 
            context.cView[1].xyz, 
            context.cView[2].xyz
        );
		
        position.xyz = (mat4(billboardMatrix) * mat4(shape.cTransform) * vec4(vPosition.xyz, 1.0)).xyz;
    }

    mat3x4 view = context.cView;
    mat4 viewMatrix = mat4(view);
    vec3 camPos = viewMatrix[3].xyz;

    if (enable_far_infinity || enable_far_inf_ignore_y)
    {
        if (enable_far_inf_ignore_y) // Not affected by horizontal camera movement
        {
            view[0].w = 0.0;
            view[2].w = 0.0;
        }
        else
        {
            view[0].w = 0.0;
            view[1].w = 0.0;
            view[2].w = 0.0;
        }
    }

	vec3 view_p = (vec4(position.xyz, 1.0) * mat4(view)).xyz;
    gl_Position = vec4(view_p.xyz, 1.0) * context.cProj;

    // Skip other calculations for depth shadow shader
    if (IS_DEPTH)
        return;

	// NORMALS -----------------------------------------------------------------------------------------------------------------
	fNormals = vec4(skinNormal(vNormal.xyz, bone_index).xyz, 1.0);

	// TANGENTS ----------------------------------------------------------------------------------------------------------------
    fTangents = vec4(0.0);
    if (enable_normal_map)
    {
	    fTangents.xyz = skinNormal(vTangent.xyz, bone_index);
	    fTangents.w = vTangent.w;
    }

    #if (BLENDER_RENDER == 1)
	fNormals.xyz = normalize(vNormal.xyz * mat3(shape.cTransform));
    fNormals.xyz = vec3(fNormals.x, fNormals.z, -fNormals.y);
    fTangents.xyz = vec3(fTangents.x, fTangents.z, -fTangents.y);
    #endif

    if (IS_GBUFFER)
    {
        fNormals.xyz  = fNormals.xyz * mat3(context.cView);
        fTangents.xyz = fTangents.xyz * mat3(context.cView);
    }

    #if (BLENDER_RENDER == 1)
	fTangents.xyz = normalize(vTangent.xyz * mat3(shape.cTransform));
	fTangents.w = vTangent.w;
    #endif

	// TEX COORDS -------------------------------------------------------------------------------------------------------------------
	fTexCoords0.xy = vTexCoords0.xy;
	
    if (texcoord_select_normal == 2 || 
       texcoord_select_normal2 == 2 || 
       texcoord_select_specmask == 2 || 
       texcoord_select_emission == 2 || 
       texcoord_select_multiA == 2 || 
       texcoord_select_multiB == 2 || 
       texcoord_select_indirectA == 2 ||
       texcoord_select_transmitt == 2 ||
	   geo_multi_alpha_type == 2)
    {
	    fTexCoords23.xy = vTexCoords2.xy;
    }

    if (texcoord_select_normal == 3 || 
       texcoord_select_normal2 == 3 || 
       texcoord_select_specmask == 3 || 
       texcoord_select_emission == 3 || 
       texcoord_select_multiA == 3 || 
       texcoord_select_multiB == 3 || 
       texcoord_select_indirectA == 3 ||
       texcoord_select_transmitt == 3 ||
	   geo_multi_alpha_type == 3)
    {
	    fTexCoords23.zw = vTexCoords3.xy;
    }

    if (IS_GBUFFER)
        return;

	// BAKES & LIGHTING --------------------------------------------------------------------------------------------------------------
    if (enable_bake_texture)
    {
		if (bake_shadow_type != -1)
			fTexCoordsBake.xy = CalcScaleBias(vTexCoords1.xy, mat.gsys_bake_st0);
		if (bake_light_type != -1)
			fTexCoordsBake.zw = CalcScaleBias(vTexCoords1.xy, mat.gsys_bake_st1);
    }
		
	// Depth shadow cascade calculations
	if (ENABLE_DEPTH_SHADOW_CASCADE) 
	{
		int cascadeIndex;
		if (view_p.z < context.cCascadeSplitDistance.x)
			cascadeIndex = 0;
		else if (view_p.z < context.cCascadeSplitDistance.y)
			cascadeIndex = 1;
		else if (view_p.z < context.cCascadeSplitDistance.z)
			cascadeIndex = 2;
		else
			cascadeIndex = 3;
			
		mat4 cascadeShadowMtx;
		switch (cascadeIndex) {
			case 0:
				cascadeShadowMtx = context.cCascadeMtx0;
				break;
				
			case 1:
				cascadeShadowMtx = context.cCascadeMtx1;
				break;
				
			case 2:
				cascadeShadowMtx = context.cCascadeMtx2;
				break;
				
			case 3:
				cascadeShadowMtx = context.cCascadeMtx3;
				break;
		}
			
		vec4 shadowPos = vec4(position.xyz, 1.0) * cascadeShadowMtx;
		vec2 shadowUV = shadowPos.xy / shadowPos.w;
		float cascadeDepth = shadowPos.z / shadowPos.w;
		
		fCascadeInfo = vec4(shadowUV, float(cascadeIndex), cascadeDepth);
	}

    if (enable_depth_buffer)
	    fViewPos.xyz = view_p;

	// FOG ---------------------------------------------------------------------------------------------------------------------------
	if (enable_fog) 
		fFog = calc_fog(view_p.xyz, 0); //z fog

    if (enable_static_depth_shadow || enable_dynamic_depth_shadow 
		|| enable_light_pre_pass || enable_color_buffer || enable_depth_buffer)
    {
	    vec3 ndc = gl_Position.xyz / gl_Position.w; //perspective divide/normalize
		
		#if (BLENDER_RENDER == 1)
        ndc.y *= -1.0;
		#endif
		
	    // Used by screen effects (shadows, light prepass, color buffer)
        fScreenCoords.xy = ndc.xy * 0.5 + 0.5;
        fScreenCoords.xy *= gl_Position.w;
        fScreenCoords.zw = gl_Position.zw;
    }

	if (enable_projection_shadow)
		fProjCoords.xyz = vec3(1.0) * mat3(context.cProjectionTexMtx0);

	// World pos to camera pos for eye position
	fViewDirection.xyz = position.xyz - vec3(
	   context.cViewInv[0].w,
	   context.cViewInv[1].w, 
	   context.cViewInv[2].w);

    if (enable_multi_texture || enable_vtx_color_diff || enable_vtx_alpha_trans 
		|| enable_vtx_color_spec || enable_vtx_color_emission 
		|| enable_vtx_color_edge_light)
    {
        fVtxColor0 = vColor;
    }
	
	fTexCoords0.w = 1.0;
	if (enable_geo_multi || enable_vtx_alpha_trans || multi_tex_calc_type_alpha == 13)
		fTexCoords0.w = vColor.a;
	
    return;
}
