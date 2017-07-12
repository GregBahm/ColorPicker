using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterScript : MonoBehaviour 
{
    public PaintbrushScript Brush;
    public Transform PaintbrushTip;
    public Transform ColorPickerTip;

    private Vector3 _autoRotateVector;

    private void Start()
    {
        _autoRotateVector = new Vector3(0.1f, .1f, .1f);
    }

    void Update () 
    {
        float weight = Mathf.Sin(UnityEngine.Time.fixedTime) / 2 + .5f;
        Brush.StrokeWeight = weight;
        PaintbrushTip.Rotate(_autoRotateVector);

        ColorPickerTip.transform.position = new Vector3(weight, 0, 0);
	}
}
