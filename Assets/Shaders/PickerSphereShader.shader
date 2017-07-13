Shader "Unlit/PickerSphereShader"
{
	Properties
	{
		_CubeMap("Cube Map", CUBE) = "black" {}
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent+2" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 boxSpace : TEXCOORD2;
				float3 objSpace : TEXCOORD3;
			};

			v2f vert (appdata v)
			{
				v2f o;
				float longestVal = max(abs(v.vertex.x), max(abs(v.vertex.y), abs(v.vertex.z)));
				float3 rawBoxSpace = v.vertex.xyz * (1 / longestVal);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.boxSpace = rawBoxSpace;
				o.objSpace = v.vertex.xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float diff = pow(length(i.boxSpace - i.objSpace), 5);
				return float4(i.boxSpace / 2 + .5, saturate(diff));
			}
				ENDCG
	}
	Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			samplerCUBE _CubeMap;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 boxSpace : TEXCOORD2;
				float3 objSpace : TEXCOORD3;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD5;
				float3 lightDir : TEXCOORD4;
			};

			v2f vert (appdata v)
			{
				v2f o;
				float longestVal = max(abs(v.vertex.x), max(abs(v.vertex.y), abs(v.vertex.z)));
				float3 rawBoxSpace = v.vertex.xyz * (1 / longestVal);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.boxSpace = rawBoxSpace;
				o.objSpace = v.vertex.xyz;
				o.normal = mul(unity_ObjectToWorld, v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.lightDir = mul(unity_ObjectToWorld, float3(0, 1, 1));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.normal = normalize(i.normal);
				i.viewDir = normalize(i.viewDir);

				float3 reflection = reflect(i.viewDir, i.normal);
				fixed4 cube = texCUBE(_CubeMap, -reflection);
				float fresnel = saturate(1 - dot(i.normal, i.viewDir));
				fresnel = pow(fresnel, 2);
				
				float shine = saturate(dot(i.normal, normalize(i.viewDir + i.lightDir)));
				shine = saturate(pow(shine, 1000));
				float diff = pow(length(i.boxSpace - i.objSpace), 5);
				float3 col = i.boxSpace / 2 + .5;
				col = lerp(col, cube, fresnel);
				diff += fresnel;
				col += shine;
				diff += shine;
				float4 ret = float4(col, saturate(diff));
				return ret;
			}
			ENDCG
		}
	}
}
