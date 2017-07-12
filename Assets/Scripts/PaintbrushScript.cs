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

    private const int StrokeSegmentMax = 16777216; // Just picking some random large number (2^24).
    private const int StrokeSegmentStride = sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) + sizeof(float) * 3; // Position Normal Tangent Weight Color

    private struct StrokeSegmentPart
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public float Weight;
        public Color Color;
    }

    private StrokeSegmentPart _lastSegment;

    private int _totalStrokeSegments;
    private int _strokeWriterKernel;
    private ComputeBuffer _strokeSegmentsBuffer;
    public ComputeShader StrokeWriterCompute;
    public Material StrokeMaterial;

    private int _undoIndex = -1;
    private int _currentStrokeSegmentsCount;
    private List<int> _undoStack;

    void Start () 
    {
        _strokeWriterKernel = StrokeWriterCompute.FindKernel("StrokeWriter");
        _strokeSegmentsBuffer = new ComputeBuffer(StrokeSegmentMax, StrokeSegmentStride);
        _lastSegment = GetStrokeSegment(false);
        _undoStack = new List<int>();
    }
    
    private StrokeSegmentPart GetStrokeSegment(bool isStartOfANewStroke)
    {
        Vector3 normal = GetStrokeNormal();
        return new StrokeSegmentPart()
        {
            Position = PaintbrushTip.position,
            Normal = normal,
            Tangent = PaintbrushTip.up,
            Weight = isStartOfANewStroke ? 0 : StrokeWeight, // This is to prevent strokes connecting to one another
            Color = ColorPicker.CurrentColor
        };
    }

    private Vector3 GetStrokeNormal()
    {
        Vector3 toLastStroke = PaintbrushTip.position - _lastSegment.Position;
        Vector3 tangent = PaintbrushTip.up;
        return Vector3.Cross(toLastStroke, tangent).normalized;
    }

    public void Undo()
    {
        if(_undoIndex < 0)
        {
            Debug.Log("No strokes to undo");
            return;
        }
        int segmentsToUndo = _undoStack[_undoIndex];
        _totalStrokeSegments -= segmentsToUndo;
        _undoIndex--;
    }

    public void Redo()
    {
        if(_undoIndex > (_undoStack.Count - 2))
        {
            Debug.Log("No strokes to redo");
            return;
        }
        int segmentsToRedo = _undoStack[_undoIndex + 1];
        _totalStrokeSegments += segmentsToRedo;
        _undoIndex++;
    }

    private void CaptureUndoOperation()
    {
        if(_undoIndex < _undoStack.Count - 1)
        {
            // They hit undo and then made some new stroke, so wipe the redo stack.
            _undoStack.RemoveRange(_undoIndex + 1, _undoStack.Count - (_undoIndex + 1)); //TODO: Make sure these numbers are right
        }

        _undoStack.Add(_currentStrokeSegmentsCount);
        _currentStrokeSegmentsCount = 0;
        _undoIndex++;
        Debug.Log("New Stroke Captured");
    }

    private bool _isStartOfANewStroke;

    void Update () 
    {
        if (StrokeWeight < float.Epsilon && _lastSegment.Weight < float.Epsilon)
        {
            if(_currentStrokeSegmentsCount > 0)
            {
                CaptureUndoOperation();
            }
            _isStartOfANewStroke = true;
            return;
        }

        Vector3 paintBrushMovement = PaintbrushTip.position - _lastSegment.Position;
        if (paintBrushMovement.magnitude < SegmentMinDist)
        {
            return;
        }

        _currentStrokeSegmentsCount++;
        _lastSegment = GetStrokeSegment(_isStartOfANewStroke);
        FireStrokeWriter(_lastSegment, _totalStrokeSegments);
        _totalStrokeSegments++; //TODO: Handle what happens when we run out of stroke segments
        _isStartOfANewStroke = false;
    }

    private void FireStrokeWriter(StrokeSegmentPart part, int strokeSegments)
    {
        StrokeWriterCompute.SetInt("_WriterIndex", strokeSegments);
        StrokeWriterCompute.SetVector("_Position", transform.InverseTransformPoint(part.Position));
        StrokeWriterCompute.SetVector("_Normal", transform.InverseTransformVector(part.Normal));
        StrokeWriterCompute.SetVector("_Tangent", transform.InverseTransformVector(part.Tangent));
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
        StrokeMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
        StrokeMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, _totalStrokeSegments);
    }

    private void OnDestroy()
    {
        _strokeSegmentsBuffer.Release();
    }
}
