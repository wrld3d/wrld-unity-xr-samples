Shader "WRLD AR Sample/MainShader" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
     	_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  "Queue"="Geometry+2" }
		LOD 200

        Stencil 
        {
            Ref 1
            Comp equal
            Pass keep
        }

		CGPROGRAM
		#pragma surface surf Standard

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input 
		{
     		float2 uv_MainTex;
     		float2 uv_BumpMap;
   		};

   		half _Glossiness;
   		half _Metallic;
   		fixed4 _Color;

   		void surf (Input IN, inout SurfaceOutputStandard o) 
   		{
     		fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
     		o.Albedo = c.rgb;
     		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
     		o.Metallic = _Metallic;
     		o.Smoothness = _Glossiness;
   		}
		ENDCG
	}

	FallBack "Diffuse"
}
