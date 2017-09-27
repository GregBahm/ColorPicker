using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusInputScript : MonoBehaviour
{
    public SixDofControlScript SixDofControl;

    void Update()
    {
        SixDofControl.StrokeWeightMaxModifier = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        SixDofControl.StrokeWeightAlpha = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        SixDofControl.RedoRequested = OVRInput.GetDown(OVRInput.Button.One);
        SixDofControl.UndoRequested = OVRInput.GetDown(OVRInput.Button.Two);
        SixDofControl.PickerSphereSizeModifier = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        SixDofControl.ColorPicking = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        SixDofControl.LeftHandPressed = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        SixDofControl.RightHandPressed = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
    }
}