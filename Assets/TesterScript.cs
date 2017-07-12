using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterScript : MonoBehaviour 
{
    public PaintbrushScript Brush;
    public Transform PaintbrushTip;
    public Transform ColorPickerTip;
    
	// Update is called once per frame
	void Update () 
    {
        float weight = Mathf.Sin(UnityEngine.Time.fixedTime) / 2 + .5f;
        Brush.StrokeWeight = weight;
	}
}
