Shader "Wrld/Stencil/StencilWrite"
{
	Properties
	{
		_MirrorClearColor("MirrorClearColor", Color) = (0,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Geometry" }
		LOD 200
		Lighting Off
		Pass
		{
			ZWrite On ZTest LEqual
			ColorMask 0

			Stencil
			{ 
				Ref 1
				Comp Always
				Pass Replace
				Fail Replace
				ZFail Keep
				ReadMask 255
				WriteMask 255
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = float4(0.0f, 0.0f, 0.0f, 1.0f);
				
				return col;
			}
			ENDCG
		}

		Pass
		{
			ZWrite On ZTest Always
			ColorMask RGBA

			Stencil
			{
				Ref 1
				Comp Equal
				Pass Keep
				Fail Keep
				ZFail Keep
				ReadMask 255
				WriteMask 255
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform float4 _MirrorClearColor;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// push the z-buffer back to the far plane for the area cut from the stencil in the first pass
				#if defined(UNITY_REVERSED_Z)
				o.vertex.xy /= o.vertex.w;
				o.vertex.z = 0.0;
				o.vertex.w = 1.0;
				#else
				o.vertex.z = o.vertex.w;
				#endif

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return _MirrorClearColor;
			}
			ENDCG
		}
	}
}
