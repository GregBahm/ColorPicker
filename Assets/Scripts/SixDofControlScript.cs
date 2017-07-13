using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SixDofControlScript : MonoBehaviour
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
    public Transform PaintMotionDampener;
    public Transform HandAverage;

    private EaseTowardsTarget _paintbrushEaser;
    private PaintbrushScript _paintbrushScript;


    public bool ColorPicking;
    public bool LeftHandPressed;
    public bool RightHandPressed;
    public float PickerSphereSizeModifier;
    public bool UndoRequested;
    public bool RedoRequested;
    public float StrokeWeightAlpha;
    public float StrokeWeightMaxModifier;

    private void Start()
    {
        _paintbrushEaser = PaintMotionDampener.GetComponent<EaseTowardsTarget>();
        _paintbrushScript = PaintMotionDampener.GetChild(0).GetComponent<PaintbrushScript>();
    }

    void Update()
    {
        UpdateMaxStrokeWeight();
        UpdateActualStrokeWeight();
        UpdateColorPicker();
        HandleUndoRedo();
        MoveDrawing();
        UpdateColorPickerSize();
        UpdateScaleMode();
    }

    private void UpdateColorPickerSize()
    {
        ColorPickerScript.SphereSize *= 1 + PickerSphereSizeModifier / 30;
    }

    private void MoveDrawing()
    {
        HandAverage.position = Vector3.Lerp(MainHand.position, OffHand.position, .5f);
        HandAverage.rotation = Quaternion.Lerp(MainHand.rotation, OffHand.rotation, .5f);
        HandAverage.LookAt(MainHand.position, HandAverage.up);

        _scaleMode = LeftHandPressed && RightHandPressed;

        _paintbrushEaser.Target = GetDampenerTarget(LeftHandPressed, RightHandPressed);
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
                _initialScale = HandAverage.transform.localScale.x;
                _initialHandDistance = dist;
            }
            else
            {
                float newScale = _initialScale * (dist / _initialHandDistance);
                HandAverage.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            _scaling = false;
        }
    }

    private void HandleUndoRedo()
    {
        if (UndoRequested)
        {
            Debug.Log("Undo Requested");
            _paintbrushScript.Undo();
        }
        if (RedoRequested)
        {
            Debug.Log("Redo Requested");
            _paintbrushScript.Redo();
        }
    }

    private void UpdateColorPicker()
    {
        PickerSphere.gameObject.SetActive(ColorPicking);
        PickSelection.gameObject.SetActive(ColorPicking);
        ColorPickerScript.enabled = ColorPicking;
        if (!_wasColorPicking)
        {
            ColorPickerScript.RestoreSphereOffset();
        }
        _wasColorPicking = ColorPicking;
    }

    private void UpdateActualStrokeWeight()
    {
        float newTargetWeight = Mathf.Pow(StrokeWeightAlpha, .5f) * MaxStrokeWeight;
        float easedWeight = Mathf.Lerp(_paintbrushScript.StrokeWeight, newTargetWeight, .7f);
        if (easedWeight < (MaxStrokeWeight / 1000))
        {
            easedWeight = 0; // Clip this off so strokes don't trail off too much.
        }
        _paintbrushScript.StrokeWeight = easedWeight;
    }

    private void UpdateMaxStrokeWeight()
    {
        MaxStrokeWeight *= 1 + StrokeWeightMaxModifier / 30;
        PaintbrushTip.localScale = new Vector3(1, MaxStrokeWeight * 2, 1);
    }
}
