Shader "Unlit/StrokeShader"
{
	Properties
	{
		_CubeMap("Texture", CUBE) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			

			samplerCUBE _CubeMap;

			struct StrokeSegment
			{
				float3 Position;
				float3 Normal;
				float3 Tangent;
				float Weight;
				float3 Color;
			};

			float4x4 _Transform;

			struct v2g
			{
				StrokeSegment start : Normal; // I cant use TEXCOORD0 for some reason.
				StrokeSegment end : TEXCOORD1;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float3 color : COLOR0;
				float3 normal : NORMAL;
				float3 viewDir : VIEWDIR;
			};

			StructuredBuffer<StrokeSegment> _StrokeSegmentsBuffer;

			v2g vert(uint meshId : SV_VertexID, uint instanceId : SV_InstanceID)
			{
				StrokeSegment segmentStart = _StrokeSegmentsBuffer[instanceId];
				StrokeSegment segmentEnd = _StrokeSegmentsBuffer[instanceId - 1];

				v2g o;
				o.start = segmentStart;
				o.end = segmentEnd;
				return o;
			}

			void DrawRibbonSide(inout TriangleStream<g2f> triStream,
				float3 pointA,
				float3 pointB,
				float3 pointC,
				float3 pointD,
				float3 startColor,
				float3 endColor,
				float3 startNormal,
				float3 endNormal)
			{
				g2f o;
				o.vertex = UnityObjectToClipPos(pointA);
				o.color = startColor;
				o.normal = startNormal;
				o.viewDir = WorldSpaceViewDir(float4(pointA, 1));
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointB);
				o.viewDir = WorldSpaceViewDir(float4(pointB, 1));
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointC);
				o.color = endColor;
				o.normal = endNormal;
				o.viewDir = WorldSpaceViewDir(float4(pointC, 1));
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointD);
				o.viewDir = WorldSpaceViewDir(float4(pointD, 1));
				triStream.Append(o);
			}

			[maxvertexcount(8)]
			void geo(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float3 pointA = p[0].start.Position + p[0].start.Tangent * p[0].start.Weight;
				pointA = mul(_Transform, float4(pointA, 1)).xyz;
				float3 pointB = p[0].start.Position + -p[0].start.Tangent * p[0].start.Weight;
				pointB = mul(_Transform, float4(pointB, 1)).xyz;

				float3 pointC = p[0].end.Position + p[0].end.Tangent * p[0].end.Weight;
				pointC = mul(_Transform, float4(pointC, 1)).xyz;
				float3 pointD = p[0].end.Position + -p[0].end.Tangent * p[0].end.Weight;
				pointD = mul(_Transform, float4(pointD, 1)).xyz;

				DrawRibbonSide(triStream, pointA, pointB, pointC, pointD, p[0].start.Color, p[0].end.Color, p[0].start.Normal, p[0].end.Normal);
				triStream.RestartStrip();
				DrawRibbonSide(triStream, pointB, pointA, pointD, pointC, p[0].start.Color, p[0].end.Color, -p[0].start.Normal, -p[0].end.Normal);
				triStream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				i.viewDir = normalize(i.viewDir);
				float frenel = dot(i.viewDir, i.normal);
				float3 reflectionUvs = reflect(-i.viewDir, i.normal);
				float3 finalUvs = lerp(reflectionUvs, i.normal, frenel);
				fixed3 reflectionCol = texCUBE(_CubeMap, finalUvs).xyz;
				float diffuse = dot(i.normal, float3(0, 1, 0)) / 2 + .5;
				float3 diffusedColor = i.color * reflectionCol;
				return float4(diffusedColor, 1);
			}
			ENDCG
		}
	}
}
