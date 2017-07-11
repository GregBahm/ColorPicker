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

    public float MaxStrokeWeight;
    private bool _wasColorPicking;
    
	void Update ()
    {
        UpdateMaxStrokeWeight();
        UpdateActualStrokeWeight();
        UpdateColorPicker();
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
