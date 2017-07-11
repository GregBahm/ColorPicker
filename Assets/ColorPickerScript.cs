using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerScript : MonoBehaviour
{
    public Transform PickerSphere;
    public Transform PickerSelection;
    public Material PickerSelectionMat;

    private Color _currentColor;
    public Color CurrentColor { get { return _currentColor; } }

    private Vector3 _offsetToCurrentColor;

    void Update ()
    {
        _currentColor = GetCurrentColor();
        _offsetToCurrentColor = PickerSphere.transform.position - PickerSelection.transform.position;
        PickerSelectionMat.SetColor("_Color", _currentColor);
	}

    public void RestoreSphereOffset()
    {
        PickerSphere.position = PickerSelection.transform.position + _offsetToCurrentColor;
    }

    private Color GetCurrentColor()
    {
        Vector4 pickerToBox = PickerSphere.worldToLocalMatrix * new Vector4(PickerSelection.position.x, PickerSelection.position.y, PickerSelection.position.z, 1);
        float longestVal = Mathf.Max(Mathf.Abs(pickerToBox.x),
            Mathf.Max(Mathf.Abs(pickerToBox.y), 
            Mathf.Abs(pickerToBox.z)));
        Vector3 projectedToCube = pickerToBox * (1f / longestVal);

        if(longestVal < float.Epsilon)
        {
            return Color.gray;
        }

        projectedToCube *= new Vector3(pickerToBox.x, pickerToBox.y, pickerToBox.z).magnitude;
        return new Color(projectedToCube.x + .5f, projectedToCube.y + .5f, projectedToCube.z + .5f);
    }
}
