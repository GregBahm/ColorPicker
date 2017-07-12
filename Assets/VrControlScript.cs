using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrControlScript : MonoBehaviour
{
    public PaintbrushScript PaintbrushScript;
    public ColorPickerScript ColorPickerScript;
    public Transform PaintbrushTip;
    public Transform PickerSphere;
    public Transform PickSelection;
    public Transform PrimaryHandAnchor;

    public float MaxStrokeWeight;
    private bool _wasColorPicking;
    
	void Update ()
    {
        UpdateMaxStrokeWeight();
        UpdateActualStrokeWeight();
        UpdateColorPicker();
        HandleUndoRedo();
        MoveDrawing();
    }

    private void MoveDrawing()
    {
        bool leftHand = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        if(leftHand)
        {
            PaintbrushScript.transform.SetParent(PrimaryHandAnchor, true);
        }
        else
        {
            PaintbrushScript.transform.SetParent(null, true);
        }
    }

    private void HandleUndoRedo()
    {
        bool undoRequested = OVRInput.GetDown(OVRInput.Button.Two);
        if(undoRequested)
        {
            Debug.Log("Undo Requested");
            PaintbrushScript.Undo();
        }
        bool redoRequested = OVRInput.GetDown(OVRInput.Button.One);
        if(redoRequested)
        {
            Debug.Log("Redo Requested");
            PaintbrushScript.Redo();
        }
    }

    private void UpdateColorPicker()
    {
        bool colorPicking = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        PickerSphere.gameObject.SetActive(colorPicking);
        PickSelection.gameObject.SetActive(colorPicking);
        ColorPickerScript.enabled = colorPicking;
        if (!_wasColorPicking)
        {
            ColorPickerScript.RestoreSphereOffset();
        }
        _wasColorPicking = colorPicking;
    }

    private void UpdateActualStrokeWeight()
    {
        float index = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        PaintbrushScript.StrokeWeight = index * MaxStrokeWeight;
    }

    private void UpdateMaxStrokeWeight()
    {
        float paintbrushSizeModifier = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        MaxStrokeWeight += paintbrushSizeModifier / 100; //TODO: Adjust that value
        MaxStrokeWeight = Mathf.Max(0, MaxStrokeWeight);
        PaintbrushTip.localScale = new Vector3(1, MaxStrokeWeight * 2, 1);
    }
}
