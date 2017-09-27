using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViveInputScript : MonoBehaviour
{
    public SteamVR_TrackedObject LeftHand;
    public SteamVR_TrackedObject RightHand;
    public SixDofControlScript SixDofControl;

    private bool _leftWasScrolling;
    private float _lastLeftScrollPos;

    private bool _rightWasScrolling;
    private float _lastRightScrollPos;

    void Update()
    {
        SteamVR_Controller.Device leftHandDevice = SteamVR_Controller.Input((int)LeftHand.index);
        SteamVR_Controller.Device rightHandDevice = SteamVR_Controller.Input((int)RightHand.index);

        bool leftScroll = leftHandDevice.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        float leftScrollPos = leftHandDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
        float leftScrollDelta = (leftScroll && !_leftWasScrolling) || !leftScroll ? 0 : leftScrollPos - _lastLeftScrollPos;
        _leftWasScrolling = leftScroll;
        _lastLeftScrollPos = leftScrollPos;

        bool rightScroll = rightHandDevice.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        float rightScrollPos = rightHandDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
        float rightScrollDelta = (rightScroll && !_rightWasScrolling) || !rightScroll ? 0 : rightScrollPos - _lastRightScrollPos;
        _rightWasScrolling = rightScroll;
        _lastRightScrollPos = rightScrollPos;

        SixDofControl.StrokeWeightMaxModifier = leftScrollDelta * 20;
        SixDofControl.StrokeWeightAlpha = leftHandDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
        SixDofControl.UndoRequested = leftHandDevice.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        SixDofControl.RedoRequested = rightHandDevice.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        SixDofControl.PickerSphereSizeModifier = rightScrollDelta * 20;
        SixDofControl.ColorPicking = rightHandDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        SixDofControl.LeftHandPressed = leftHandDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        SixDofControl.RightHandPressed = rightHandDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
    }
}
