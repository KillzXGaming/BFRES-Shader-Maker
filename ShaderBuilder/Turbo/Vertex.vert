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
layout (location = 12) in vec4 vColor; //@ id="_c0"

layout (location = 0) out vec4 fTexCoords0;
layout (location = 1) out vec4 fFog;
layout (location = 2) out vec4 fTangents;
layout (location = 3) out vec4 fNormals;
layout (location = 4) out vec4 fTexCoordsBake; //xy shadow, zw lightmap
layout (location = 5) out vec4 fProjCoords;
layout (location = 6) out vec4 fViewDirection;
layout (location = 7) out vec4 fScreenCoords;
layout (location = 9) out vec4 fVtxColor0;
layout (location = 10) out vec4 fTexCoords23;
layout (location = 11) out vec4 fViewPos;

#endif

#include "WiiUCommon.glsl"

vec4 calc_fog(vec3 pos, int idx)
{
	Fog fog = environment.fog[idx];
	float z = dot(fog.Direction.xyz, pos.xyz);

	vec4 fog_output = vec4(fog.Color.xyz, 1.0);
	fog_output.a = fog.Color.a * clamp(z * fog.End + fog.Start, 0.0, 1.0);

	return fog_output;
}

vec4 skin(vec3 pos, ivec4 index)
{
    vec4 newPosition = vec4(pos.xyz, 1.0);

    if (gsys_weight == 0 && !gsys_invalidate_world_srt)
        newPosition = vec4(pos, 1.0) * mat4(shape.cTransform);
    if (gsys_weight == 1)
        newPosition = vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.x]);

    if (gsys_weight >  1)
        newPosition =  vec4(pos, 1.0) * mat4(boneMatrices.cBoneMatrices[index.x]) * vBoneWeight.x;
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

    if (gsys_weight == 0 && !gsys_invalidate_world_srt) //gsys_invalidate_world_srt disables shape transform
        newNormal =  nr * mat3(shape.cTransform);
    if (gsys_weight == 1)
        newNormal =  nr * mat3(boneMatrices.cBoneMatrices[index.x]);

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
// This took me forever to figure out, the blender uv origin is different so we need to flip things
#if (BLENDER_RENDER == 1)
    vec2 uvFlipped = vec2(uv.x, 1.0 - uv.y);
    return uvFlipped.xy * vec2(sb.x, -sb.y) + vec2(sb.z, 1.0 - sb.w);
#endif
    return uv.xy * sb.xy + sb.zw;
}

vec2 calc_texcoord_matrix(mat2x4 matrix, vec2 tex_coord)
{
	//actually a 2x3 matrix stored in 2x4
    vec2 tex_coord_out;
    tex_coord_out.x = fma(tex_coord.x, matrix[0].x, tex_coord.y * matrix[0].z) + matrix[1].x;
    tex_coord_out.y = fma(tex_coord.x, matrix[0].y, tex_coord.y * matrix[0].w) + matrix[1].y;
	return tex_coord_out;
}

vec2 get_tex_coord(vec2 tex_coord, mat2x4 matrix, int type)
{
	if (type == 0 && !gsys_invalidate_texture_srt)
		return calc_texcoord_matrix(matrix, tex_coord);
	if (type == 4) //sphere mapping used on metal characters
	{
		//view normal
		vec3 view_n = (normalize(vNormal.xyz) * mat3(context.cView)).xyz;

    #if (BLENDER_RENDER == 1)
        return vec2(view_n.x, view_n.y) * 0.5 + 0.5;
    #endif

		//center the uvs
        return vec2(view_n.x, -view_n.y) * 0.5 + 0.5;
	}
	return tex_coord;
}

 
void main()
{	
	ivec4 bone_index = vBoneIndices;
	//position
	vec4 position = skin(vPosition.xyz, bone_index);
    #if (BLENDER_RENDER == 1)
        position = vec4(vPosition.xyz, 1.0) * mat4(shape.cTransform);
    #endif

    //Billboard (ie item box font)
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

    mat4 viewMatrix = mat4(context.cView);
    vec3 camPos = viewMatrix[3].xyz;

    mat3x4 view = context.cView;

    if (enable_far_infinity || enable_far_inf_ignore_y)
    {
        if (enable_far_inf_ignore_y) //Not affected by horizontal camera movement
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

    //Skip other calculations for depth shadow shader
    if (IS_DEPTH)
        return;

	//normals
	fNormals = vec4(skinNormal(vNormal.xyz, bone_index).xyz, 1.0);

	//tangents
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

	// Material tex coords
	fTexCoords0.xy = get_tex_coord(vTexCoords0.xy, mat.tex_mtx0, texcoord_calc_texcoord0);	

    // Removed usage warnings
#if (IS_WII_U == 1)
    fTexCoords23 = vec4(0.0);
#endif

    if (texcoord_select_normal == 2 || 
       texcoord_select_normal2 == 2 || 
       texcoord_select_specmask == 2 || 
       texcoord_select_emission == 2 || 
       texcoord_select_multiA == 2 || 
       texcoord_select_multiB == 2 || 
       texcoord_select_indirectA == 2 ||
       texcoord_select_transmitt == 2)
    {
	    fTexCoords23.xy = get_tex_coord(vTexCoords2.xy, mat.tex_mtx1, texcoord_calc_texcoord2);	
    }

    if (texcoord_select_normal == 3 || 
       texcoord_select_normal2 == 3 || 
       texcoord_select_specmask == 3 || 
       texcoord_select_emission == 3 || 
       texcoord_select_multiA == 3 || 
       texcoord_select_multiB == 3 || 
       texcoord_select_indirectA == 3||
       texcoord_select_transmitt == 3)
    {
	    fTexCoords23.zw = get_tex_coord(vTexCoords2.xy, mat.tex_mtx2, texcoord_calc_texcoord3);	
    }

    if (IS_GBUFFER)
        return;

	//bake texCoords
    if (enable_bake_texture)
    {
        fTexCoordsBake.xy = CalcScaleBias(vTexCoords1.xy, mat.gsys_bake_st0);
        fTexCoordsBake.zw = CalcScaleBias(vTexCoords1.xy, mat.gsys_bake_st1);
    }

    if (enable_depth_buffer)
	    fViewPos.xyz = view_p;

	//view pos z
    if (enable_depth_buffer)
	    fTexCoords0.z = view_p.z;

    //transparency
    fTexCoords0.w = 1.0;
    if (ENABLE_TRANSPARENCY || enable_geo_multi)
        fTexCoords0.w = mat.transparency;

    if (enable_vtx_alpha_trans)
        fTexCoords0.w *= vColor.a;

	//Fog
	if (enable_fog) 
		fFog = calc_fog(view_p.xyz, 0); //z fog

    if (enable_static_depth_shadow || enable_dynamic_depth_shadow 
    || enable_light_pre_pass || enable_color_buffer || enable_depth_buffer)
    {
	    vec3 ndc = gl_Position.xyz / gl_Position.w; //perspective divide/normalize
	    //Flip for blender
#if (BLENDER_RENDER == 1)
        ndc.y *= -1.0;
#endif
	    //used by screen effects (shadows, light prepass, color buffer)
        fScreenCoords.xy = ndc.xy * 0.5 + 0.5;
        fScreenCoords.xy *= gl_Position.w;
        fScreenCoords.zw = gl_Position.zw;
    }

	if (enable_projection_shadow)
		fProjCoords.xyz = vec3(1.0) * mat3(context.cProjectionTexMtx0);

	//world pos - camera pos for eye position
	fViewDirection.xyz = position.xyz - vec3(
	   context.cViewInv[0].w,
	   context.cViewInv[1].w, 
	   context.cViewInv[2].w);

    if (enable_vtx_color_diff || enable_vtx_alpha_trans || 
        enable_vtx_color_spec || enable_vtx_color_emission)
    {
        fVtxColor0 = vColor;
    }
    return;
}
