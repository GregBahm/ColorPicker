Shader "Unlit/StrokeShader"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct StrokeSegment
			{
				float3 Position;
				float3 Normal;
				float Weight;
				float3 Color;
			};

			struct v2g
			{
				StrokeSegment start : Normal; // I cant use TEXCOORD0 for some reason.
				StrokeSegment end : TEXCOORD1;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float3 color : COLOR0;
			};

			StructuredBuffer<StrokeSegment> _StrokeSegmentsBuffer;

			v2g vert(uint meshId : SV_VertexID, uint instanceId : SV_InstanceID)
			{
				StrokeSegment segmentStart = _StrokeSegmentsBuffer[instanceId - 1];
				StrokeSegment segmentEnd = _StrokeSegmentsBuffer[instanceId];

				v2g o;
				o.start = segmentStart;
				o.end = segmentEnd;
				return o;
			}

			[maxvertexcount(4)]
			void geo(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float3 pointA = p[0].start.Position + p[0].start.Normal * p[0].start.Weight;
				float3 pointB = p[0].start.Position + -p[0].start.Normal * p[0].start.Weight;

				float3 pointC = p[0].end.Position + p[0].end.Normal * p[0].end.Weight;
				float3 pointD = p[0].end.Position + -p[0].end.Normal * p[0].end.Weight;

				g2f o;
				o.vertex = UnityObjectToClipPos(pointA);
				o.color = p[0].start.Color;
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointB);
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointC);
				o.color = p[0].end.Color;
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(pointD);
				triStream.Append(o);
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				return float4(i.color, 1);
			}
			ENDCG
		}
	}
}
