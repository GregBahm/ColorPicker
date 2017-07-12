using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrControlScript : MonoBehaviour
{
    public ColorPickerScript ColorPickerScript;
    public Transform PaintbrushTip;
    public Transform PickerSphere;
    public Transform PickSelection;

    public float MaxStrokeWeight;
    private bool _wasColorPicking;

    private bool _scaleMode;
    private bool _scaling;
    private float _initialScale;
    private float _initialHandDistance;

    public Transform MainHand;
    public Transform OffHand;
    public Transform PaintbrushControl;
    public Transform HandAverage;

    private EaseTowardsTarget _paintbrushEaser;
    private PaintbrushScript _paintbrushScript;

    private void Start()
    {
        _paintbrushEaser = PaintbrushControl.GetComponent<EaseTowardsTarget>();
        _paintbrushScript = PaintbrushControl.GetComponent<PaintbrushScript>();
    }

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
        HandAverage.position = Vector3.Lerp(MainHand.position, OffHand.position, .5f);
        HandAverage.rotation = Quaternion.Lerp(MainHand.rotation, OffHand.rotation, .5f);
        HandAverage.LookAt(MainHand.position, HandAverage.up);

        bool leftHand = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        bool rightHand = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

        _scaleMode = leftHand && rightHand;

        _paintbrushEaser.Target = GetDampenerTarget(leftHand, rightHand);
    }
    private Transform GetDampenerTarget(bool leftHand, bool rightHand)
    {
        if (leftHand && rightHand)
        {
            return HandAverage;
        }
        if (leftHand)
        {
            return MainHand;
        }
        if (rightHand)
        {
            return OffHand;
        }
        return null;
    }
    private void UpdateScaleMode()
    {
        if (_scaleMode)
        {
            float dist = (MainHand.position - OffHand.position).magnitude;
            if (!_scaling)
            {
                _scaling = true;
                _initialScale = PaintbrushControl.transform.localScale.x;
                _initialHandDistance = dist;
            }
            else
            {
                float newScale = _initialScale * (dist / _initialHandDistance);
                PaintbrushControl.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            _scaling = false;
        }
    }

    private void HandleUndoRedo()
    {
        bool undoRequested = OVRInput.GetDown(OVRInput.Button.Two);
        if(undoRequested)
        {
            Debug.Log("Undo Requested");
            _paintbrushScript.Undo();
        }
        bool redoRequested = OVRInput.GetDown(OVRInput.Button.One);
        if(redoRequested)
        {
            Debug.Log("Redo Requested");
            _paintbrushScript.Redo();
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
        float easedWeight = Mathf.Lerp(_paintbrushScript.StrokeWeight, index * MaxStrokeWeight, .5f);
        if(easedWeight < (MaxStrokeWeight / 100))
        {
            easedWeight = 0; // Clip this off so strokes don't trail off too much.
        }
        _paintbrushScript.StrokeWeight = easedWeight;
    }

    private void UpdateMaxStrokeWeight()
    {
        float paintbrushSizeModifier = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        MaxStrokeWeight += paintbrushSizeModifier / 100; //TODO: Adjust that value
        MaxStrokeWeight = Mathf.Max(0, MaxStrokeWeight);
        PaintbrushTip.localScale = new Vector3(1, MaxStrokeWeight * 2, 1);
    }
}
