using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ExporterScript : MonoBehaviour
{
  ComputeShader _computeConverter;
  ComputeBuffer _exportBuffer;
  int _meshBufferStride = 0; //TODO: Figure out this;

  int _converterKernel;

  private void Start()
  {
    _converterKernel = _computeConverter.FindKernel("Converter");
  }

  public void Export(ComputeBuffer strokeBuffer, int totalStrokeSegments, string filePath)
  {
    _exportBuffer = new ComputeBuffer(totalStrokeSegments, _meshBufferStride);
    _computeConverter.SetBuffer(_converterKernel, "_StrokeSegmentsBuffer", strokeBuffer);
    _computeConverter.SetBuffer(_converterKernel, "_ExportBuffer", _exportBuffer);
    _computeConverter.Dispatch(_converterKernel, totalStrokeSegments, 1, 1);
    
  }
}
