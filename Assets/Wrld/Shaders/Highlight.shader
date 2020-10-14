Shader "Wrld/Highlight"
{
	Properties
	{
        _Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
		LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -20

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

            float4 _Color;

			struct appdata
			{
				float4 vertex: POSITION;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(0)
				float4 vertex: SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				float4 col = _Color; 
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
