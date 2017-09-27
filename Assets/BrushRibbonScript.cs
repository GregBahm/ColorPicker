using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushRibbonScript : MonoBehaviour 
{
    public SixDofControlScript SixDofController;

    public Transform BrushTop;
    public Transform BrushBottom;
    public Transform BrushItself;

    private Vector3 lastPos;

	void Update () 
    {
        Vector3 brushPosition = (BrushTop.position + BrushBottom.position) / 2;
        float length = (BrushTop.position - BrushBottom.position).magnitude;
        BrushItself.position = brushPosition;
        BrushItself.localScale = new Vector3(1, length, 1);
        SixDofController.MaxStrokeWeight = length / 2;

        Vector3 unprojectedNormal = lastPos - brushPosition;
        if(unprojectedNormal.magnitude > float.Epsilon)
        {
            Vector3 tangent = BrushTop.position - BrushBottom.position;
            Vector3 projected = Vector3.ProjectOnPlane(unprojectedNormal, tangent);

            BrushItself.LookAt(brushPosition + projected, tangent);
        }
        lastPos = brushPosition;
	}
}
