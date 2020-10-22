/*
Unity Standard Vertex Color Shader for Unity 2017.1.1f1
https://github.com/H-man/UnityVertexColors
*/

#ifndef UNITY_VC_INCLUDED
#define UNITY_VC_INCLUDED

struct VertexInput_VC
{
	float4 vertex	: POSITION;
	float4 color : COLOR;
	half3 normal	: NORMAL;
	float2 uv0		: TEXCOORD0;
	float2 uv1		: TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	float2 uv2		: TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
	half4 tangent	: TANGENT;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords_VC(VertexInput_VC v)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
	texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	return texcoord;
}

inline half4 VertexGIForward_VC(VertexInput_VC v, float3 posWorld, half3 normalWorld)
{
	half4 ambientOrLightmapUV = 0;
	// Static lightmaps
#ifndef LIGHTMAP_OFF
	ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	ambientOrLightmapUV.zw = 0;
	// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
#elif UNITY_SHOULD_SAMPLE_SH
#ifdef VERTEXLIGHT_ON
	// Approximated illumination from non-important point lights
	ambientOrLightmapUV.rgb = Shade4PointLights(
	unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
	unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
	unity_4LightAtten0, posWorld, normalWorld);
#endif

	ambientOrLightmapUV.rgb = ShadeSHPerVertex(normalWorld, ambientOrLightmapUV.rgb);
#endif

#ifdef DYNAMICLIGHTMAP_ON
	ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

	return ambientOrLightmapUV;
}

//Forward Pass
struct VertexOutputForwardBase_VC
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
#if UNITY_VERSION > 201731
    float4 tangentToWorldAndPackedData[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
#elif UNITY_VERSION > 560
	half4 tangentToWorldAndPackedData[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
#else
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;
#endif
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
	//UNITY_FOG_COORDS(7)
	float4 color : COLOR;
	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
	float3 posWorld					: TEXCOORD7;

#if UNITY_OPTIMIZE_TEXCUBELOD
	#if UNITY_SPECCUBE_BOX_PROJECTION
		half3 reflUVW				: TEXCOORD8;
	#else
		half3 reflUVW				: TEXCOORD7;
	#endif
#endif

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardBase_VC vertForwardBase_VC(VertexInput_VC v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputForwardBase_VC o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase_VC, o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.posWorld = posWorld.xyz;
	o.pos = UnityObjectToClipPos(v.vertex);

	o.tex = TexCoords_VC(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
#ifdef _TANGENT_TO_WORLD
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
#if UNITY_VERSION >= 560
	o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
	o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
	o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
#else
	o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
	o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
	o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
#endif
#else
#if UNITY_VERSION >= 560
	o.tangentToWorldAndPackedData[0].xyz = 0;
	o.tangentToWorldAndPackedData[1].xyz = 0;
	o.tangentToWorldAndPackedData[2].xyz = normalWorld;
#else
	o.tangentToWorldAndParallax[0].xyz = 0;
	o.tangentToWorldAndParallax[1].xyz = 0;
	o.tangentToWorldAndParallax[2].xyz = normalWorld;
#endif
#endif
	//We need this for shadow receving
	TRANSFER_SHADOW(o);

	o.ambientOrLightmapUV = VertexGIForward_VC(v, posWorld, normalWorld);

#ifdef _PARALLAXMAP
	TANGENT_SPACE_ROTATION;
	half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
	o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
	o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
	o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
#endif

#if UNITY_OPTIMIZE_TEXCUBELOD
	o.reflUVW = reflect(o.eyeVec, normalWorld);
#endif

	o.color = v.color;

    //UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

half4 fragForwardBase_VC(VertexOutputForwardBase_VC i) : SV_Target
{
	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW = i.reflUVW;
#endif

	UnityLight mainLight = MainLight();// (s.normalWorld);
	half atten = SHADOW_ATTENUATION(i);

	half occlusion = Occlusion(i.tex.xy);
	UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

	half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
	c *= i.color;

    c.rgb += UNITY_BRDF_GI(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, occlusion, gi);
	c.rgb += Emission(i.tex.xy); //i.color.a * s.diffColor;

	//UNITY_APPLY_FOG(i.fogCoord, c.rgb);
	s.alpha *= i.color.a;

	return OutputForward(c, s.alpha);
}

//  Additive forward pass (one light per pass)
struct VertexOutputForwardAdd_VC
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;

#if UNITY_VERSION > 201731
    float4 tangentToWorldAndLightDir[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:lightDir]
#else
	half4 tangentToWorldAndLightDir[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:lightDir]
#endif
	LIGHTING_COORDS(5, 6)
	//UNITY_FOG_COORDS(7)
	float4 posWorld : TEXCOORD7;
	float4 color : COLOR;
		// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
	/*
#if defined(_PARALLAXMAP)
	half3 viewDirForParallax			: TEXCOORD8;
#endif
	
	*/

	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardAdd_VC vertForwardAdd_VC(VertexInput_VC v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputForwardAdd_VC o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd_VC, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.pos = UnityObjectToClipPos(v.vertex);

	o.tex = TexCoords_VC(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
#ifdef _TANGENT_TO_WORLD
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
	o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
	o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
	o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
#else
	o.tangentToWorldAndLightDir[0].xyz = 0;
	o.tangentToWorldAndLightDir[1].xyz = 0;
	o.tangentToWorldAndLightDir[2].xyz = normalWorld;
#endif
	//We need this for shadow receving
	TRANSFER_VERTEX_TO_FRAGMENT(o);

	float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = NormalizePerVertexNormal(lightDir);
#endif
	o.tangentToWorldAndLightDir[0].w = lightDir.x;
	o.tangentToWorldAndLightDir[1].w = lightDir.y;
	o.tangentToWorldAndLightDir[2].w = lightDir.z;

#ifdef _PARALLAXMAP
	TANGENT_SPACE_ROTATION;
	o.viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
#endif

    o.color = v.color;

	//UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

half4 fragForwardAdd_VC(VertexOutputForwardAdd_VC i) : SV_Target
{
	FRAGMENT_SETUP_FWDADD(s)

    #if UNITY_VERSION > 201731
    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld)
    UnityLight light = AdditiveLight(IN_LIGHTDIR_FWDADD(i), atten);
    #else
    UnityLight light = AdditiveLight(IN_LIGHTDIR_FWDADD(i), LIGHT_ATTENUATION(i));
    #endif

	UnityIndirect noIndirect = ZeroIndirect();

	half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, light, noIndirect);
	c *= i.color;

	//UNITY_APPLY_FOG_COLOR(i.fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
	return OutputForward(c, s.alpha);
}

//Deferred Pass
struct VertexOutputDeferred_VC
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
#if UNITY_VERSION > 201731
    float4 tangentToWorldAndPackedData[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
#elif UNITY_VERSION >= 560
	half4 tangentToWorldAndPackedData[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
#else
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;
#endif
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UVs			
	float4 color : COLOR;

	float3 posWorld						: TEXCOORD6;


#if UNITY_OPTIMIZE_TEXCUBELOD
#if UNITY_SPECCUBE_BOX_PROJECTION
	half3 reflUVW				: TEXCOORD7;
#else
	half3 reflUVW				: TEXCOORD6;
#endif
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputDeferred_VC vertDeferred_VC(VertexInput_VC v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputDeferred_VC o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred_VC, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.posWorld = posWorld;
	o.pos = UnityObjectToClipPos(v.vertex);

	o.tex = TexCoords_VC(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
#ifdef _TANGENT_TO_WORLD
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
	#if UNITY_VERSION >= 560
		o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#endif
#else
	#if UNITY_VERSION >= 560
		o.tangentToWorldAndPackedData[0].xyz = 0;
		o.tangentToWorldAndPackedData[1].xyz = 0;
		o.tangentToWorldAndPackedData[2].xyz = normalWorld;
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif
#endif

	o.ambientOrLightmapUV = 0;
#ifndef LIGHTMAP_OFF
	o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#elif UNITY_SHOULD_SAMPLE_SH
	o.ambientOrLightmapUV.rgb = ShadeSHPerVertex(normalWorld, o.ambientOrLightmapUV.rgb);
#endif
#ifdef DYNAMICLIGHTMAP_ON
	o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

#ifdef _PARALLAXMAP
	TANGENT_SPACE_ROTATION;
	half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
	o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
	o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
	o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
#endif

	o.color = v.color;

#if UNITY_OPTIMIZE_TEXCUBELOD
	o.reflUVW = reflect(o.eyeVec, normalWorld);
#endif

	return o;
}

void fragDeferred_VC(
	VertexOutputDeferred_VC i,
	out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
	out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
	out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
	out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
)
{
#if (SHADER_TARGET < 30)
	outDiffuse = 1;
	outSpecSmoothness = 1;
	outNormal = 0;
	outEmission = 0;
	return;
#endif

	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
		s.reflUVW = i.reflUVW;
#endif

	// no analytic lights in this pass
	UnityLight dummyLight = DummyLight();
	half atten = 1;

	// only GI
	half occlusion = Occlusion(i.tex.xy);
#if UNITY_ENABLE_REFLECTION_BUFFERS
	bool sampleReflectionsInDeferred = false;
#else
	bool sampleReflectionsInDeferred = true;
#endif

	UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

	half3 color = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

    color *= i.color;
	color += UNITY_BRDF_GI(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, occlusion, gi);

#ifdef _EMISSION
	color += Emission(i.tex.xy);
#endif

#ifndef UNITY_HDR_ON
	color.rgb = exp2(-color.rgb);
#endif

	s.diffColor *= i.color.rgb;

	outDiffuse = half4(s.diffColor, occlusion);
	outSpecSmoothness = half4(s.specColor, s.smoothness);
	outNormal = half4(s.normalWorld*0.5 + 0.5, 1);
	outEmission = half4(color, 1);
}

#endif
