using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPickerScript : MonoBehaviour
{
    public Transform PickerSphere;
    public Transform PickerSelection;
    public Material PickerSelectionMat;

    private Color _currentColor;
    public Color CurrentColor { get { return _currentColor; } }

    void Update ()
    {
        _currentColor = GetCurrentColor();
        PickerSelectionMat.SetColor("_Color", _currentColor);
	}

    private Color GetCurrentColor()
    {
        Vector3 pickerToBox = PickerSphere.worldToLocalMatrix * PickerSelection.position;
        float longestVal = Mathf.Max(Mathf.Abs(pickerToBox.x),
            Mathf.Max(Mathf.Abs(pickerToBox.y), 
            Mathf.Abs(pickerToBox.z)));
        Vector3 projectedToCube = pickerToBox * (1f / longestVal);
        projectedToCube *= pickerToBox.magnitude;

        return new Color(projectedToCube.x + .5f, projectedToCube.y + .5f, projectedToCube.z + .5f);
    }
}
