using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintbrushScript : MonoBehaviour 
{
    public ColorPickerScript ColorPicker;

    public Transform PaintbrushTip;
    
    public float SegmentMinDist;

    public float StrokeWeight;

    private const int StrokeSegmentMax = 16777216; // Just picking some random large number (2^24). No idea if it is too big or too small.
    private const int StrokeSegmentStride = sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) + sizeof(float) * 3;

    private struct StrokeSegmentPart
    {
        public Vector3 Position;
        public Vector3 Normal;
        public float Weight;
        public Color Color;
    }

    private StrokeSegmentPart _lastSegment;

    private int _strokeSegments;
    private int _strokeWriterKernel;
    private ComputeBuffer _strokeSegmentsBuffer;
    public ComputeShader StrokeWriterCompute;
    public Material StrokeMaterial;

    void Start () 
    {
        _strokeWriterKernel = StrokeWriterCompute.FindKernel("StrokeWriter");
        _strokeSegmentsBuffer = new ComputeBuffer(StrokeSegmentMax, StrokeSegmentStride);
        _lastSegment = GetStrokeSegment();
    }
    
    private StrokeSegmentPart GetStrokeSegment()
    {
        return new StrokeSegmentPart()
        {
            Position = PaintbrushTip.position,
            Normal = PaintbrushTip.up,
            Weight = StrokeWeight,
            Color = ColorPicker.CurrentColor
        };
    }
	
	void Update () 
    {
        if (StrokeWeight < float.Epsilon && _lastSegment.Weight < float.Epsilon)
        {
            return;
        }

        Vector3 paintBrushMovement = PaintbrushTip.position - _lastSegment.Position;
        if (paintBrushMovement.magnitude < SegmentMinDist)
        {
            return;
        }

        _lastSegment = GetStrokeSegment();
        FireStrokeWriter(_lastSegment, _strokeSegments);
        _strokeSegments++; //TODO: Handle what happens when we run out of stroke segments
    }

    private void FireStrokeWriter(StrokeSegmentPart part, int strokeSegments)
    {
        StrokeWriterCompute.SetInt("_WriterIndex", strokeSegments);
        StrokeWriterCompute.SetVector("_Position", part.Position);
        StrokeWriterCompute.SetVector("_Normal", part.Normal);
        StrokeWriterCompute.SetFloat("_Weight", part.Weight);
        StrokeWriterCompute.SetVector("_Color", ColorToVector(part.Color));
        StrokeWriterCompute.SetBuffer(_strokeWriterKernel, "_StrokeSegmentsBuffer", _strokeSegmentsBuffer);
        StrokeWriterCompute.Dispatch(_strokeWriterKernel, 1, 1, 1);
    }

    private static Vector4 ColorToVector(Color color)
    {
        return new Vector4(color.r, color.g, color.b, color.a);
    }

    private void OnRenderObject()
    {
        StrokeMaterial.SetBuffer("_StrokeSegmentsBuffer", _strokeSegmentsBuffer);
        StrokeMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, _strokeSegments);
    }
}
