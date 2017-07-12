Shader "Unlit/CubemapLighting"
{
	Properties
	{
		_MainTex ("Texture", CUBE) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
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
				float3 normal : NORMAL;
				float3 viewDir : VIEWDIR;
			};

			samplerCUBE _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				o.viewDir = WorldSpaceViewDir(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.viewDir = normalize(i.viewDir);
				float frenel = dot(i.viewDir, i.normal);
				float3 reflectionUvs = reflect(-i.viewDir, i.normal);
				float3 finalUvs = lerp(reflectionUvs, i.normal, frenel);
				fixed4 reflectionCol = texCUBElod(_MainTex, float4(finalUvs, pow(frenel, .5) * 4));
				return reflectionCol;
			}
			ENDCG
		}
	}
}
