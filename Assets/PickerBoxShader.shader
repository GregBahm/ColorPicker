Shader "Unlit/PickerBoxShader"
{
	Properties
	{
		_ClipThreshold("Clip Threshold", Range(0, 1)) = .5
		_Size("Size", Range(0, 1)) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _ClipThreshold;
			float _Size;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 objSpace : TEXCOORD1;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				v.vertex *= _Size;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.objSpace = v.vertex + .5;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uvDists = abs((i.uv - .5) * 2);
				float uvDist = max(uvDists.x, uvDists.y);
				clip(uvDist - _ClipThreshold);
				return float4(i.objSpace, 1);
			}
			ENDCG
		}
	}
}
