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
			#pragma target 5.0 
			
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
				float2 uv : TEXCOORD0;
				float3 tangent : TEXCOORD1;
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

			void DrawQuad(inout TriangleStream<g2f> triStream,
				float3 pointA,
				float3 pointB,
				float3 pointC,
				float3 pointD,
				float3 startColor,
				float3 endColor,
				float3 startNormal,
				float3 endNormal,
				float3 startTangent,
				float3 endTangent)
			{

				pointA = mul(_Transform, float4(pointA, 1)).xyz;
				pointB = mul(_Transform, float4(pointB, 1)).xyz;
				pointC = mul(_Transform, float4(pointC, 1)).xyz;
				pointD = mul(_Transform, float4(pointD, 1)).xyz;

				g2f o;
				o.vertex = UnityObjectToClipPos(pointA);
				o.color = startColor;
				o.normal = startNormal;
				o.tangent = startTangent;
				o.viewDir = WorldSpaceViewDir(float4(pointA, 1));
				o.uv = float2(1, 1);
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointB);
				o.viewDir = WorldSpaceViewDir(float4(pointB, 1));
				o.uv = float2(1, 0);
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointC);
				o.color = endColor;
				o.normal = endNormal;
				o.tangent = endTangent;
				o.viewDir = WorldSpaceViewDir(float4(pointC, 1));
				o.uv = float2(0, 1);
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointD);
				o.viewDir = WorldSpaceViewDir(float4(pointD, 1));
				o.uv = float2(0, 0);
				triStream.Append(o);
			}

			#define _StrokeThickness 0.02

			[maxvertexcount(16)]
			void geo(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float3 baseStartUp = p[0].start.Position + p[0].start.Tangent * p[0].start.Weight;
				float3 baseStartDown = p[0].start.Position + -p[0].start.Tangent * p[0].start.Weight;
				float3 baseEndUp = p[0].end.Position + p[0].end.Tangent * p[0].end.Weight;
				float3 baseEndDown = p[0].end.Position + -p[0].end.Tangent * p[0].end.Weight;

				float3 frontStartUp = baseStartUp - p[0].start.Normal * (p[0].start.Weight * _StrokeThickness);
				float3 frontStartDown = baseStartDown - p[0].start.Normal * (p[0].start.Weight * _StrokeThickness);
				float3 frontEndUp = baseEndUp - p[0].end.Normal * p[0].end.Weight * _StrokeThickness;
				float3 frontEndDown = baseEndDown - p[0].end.Normal * p[0].end.Weight * _StrokeThickness;

				float3 backStartUp = baseStartUp + p[0].start.Normal * (p[0].start.Weight * _StrokeThickness);
				float3 backStartDown = baseStartDown + p[0].start.Normal * (p[0].start.Weight * _StrokeThickness);
				float3 backEndUp = baseEndUp + p[0].end.Normal * p[0].end.Weight * _StrokeThickness;
				float3 backEndDown = baseEndDown + p[0].end.Normal * p[0].end.Weight * _StrokeThickness;


				DrawQuad(triStream, frontStartUp, frontStartDown, frontEndUp, frontEndDown,
					p[0].start.Color, p[0].end.Color, p[0].start.Normal, p[0].end.Normal, p[0].start.Tangent, p[0].end.Tangent);

				triStream.RestartStrip();

				DrawQuad(triStream, backStartDown, backStartUp, backEndDown, backEndUp,
					p[0].start.Color, p[0].end.Color, -p[0].start.Normal, -p[0].end.Normal, -p[0].start.Tangent, -p[0].end.Tangent);

				triStream.RestartStrip();

				DrawQuad(triStream, frontEndUp, backEndUp, frontStartUp, backStartUp,
					p[0].start.Color, p[0].end.Color, p[0].start.Tangent, p[0].end.Tangent, p[0].start.Normal, p[0].end.Normal);

				triStream.RestartStrip();

				DrawQuad(triStream, backEndDown, frontEndDown, backStartDown, frontStartDown,
					p[0].start.Color, p[0].end.Color, -p[0].start.Tangent, -p[0].end.Tangent, -p[0].start.Normal, -p[0].end.Normal);
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				float distToEdge = (i.uv.y - .5) * 2;
				distToEdge = pow(abs(distToEdge), 50) * sign(-distToEdge);
				i.normal = normalize(i.normal + i.tangent * distToEdge);

				i.viewDir = normalize(i.viewDir);
				float frenel = dot(-i.viewDir, i.normal);
				float3 reflectionUvs = reflect(i.viewDir, i.normal);
				float3 finalUvs = lerp(reflectionUvs, i.normal, pow(abs(frenel), 10));

				fixed3 reflectionCol = texCUBE(_CubeMap, -i.normal).xyz;
				reflectionCol = lerp(reflectionCol, 1, .5);
				
				float3 diffusedColor = i.color * reflectionCol;
				return float4(diffusedColor, 1);
			}
			ENDCG
		}
	}
}
