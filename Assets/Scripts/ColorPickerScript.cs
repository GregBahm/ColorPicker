using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerScript : MonoBehaviour
{
    public Transform PickerSphere;
    public Transform PickerSelection;
    public Material PickerSelectionMat;

    public float SphereSize;

    private Color _currentColor;
    public Color CurrentColor { get { return _currentColor; } }

    private Vector3 _offsetToCurrentColor;

    private void Start()
    {
        SphereSize = PickerSphere.localScale.x;
    }

    private void Update ()
    {
        _currentColor = GetCurrentColor();
        _offsetToCurrentColor = PickerSphere.transform.position - PickerSelection.transform.position;
        PickerSelectionMat.SetColor("_Color", _currentColor);
        UpdatePickerSphereScale();
	}

    private void UpdatePickerSphereScale()
    {
        float oldScale = PickerSphere.localScale.x;
        if (Mathf.Abs(PickerSphere.localScale.x - SphereSize) > float.Epsilon)
        {
            PickerSphere.Translate(-_offsetToCurrentColor, Space.World);
            PickerSphere.localScale = new Vector3(SphereSize, SphereSize, SphereSize);
            _offsetToCurrentColor = _offsetToCurrentColor * SphereSize / oldScale;
            PickerSphere.Translate(_offsetToCurrentColor, Space.World);
        }
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
