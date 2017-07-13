using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseTowardsTarget : MonoBehaviour 
{
    public Transform Target;
    [Range(0, 1)]
    public float PositionLerp = .5f;
    [Range(0, 1)]
    public float RotationLerp = .5f;
    [Range(0, 1)]
    public float ScaleLerp = .5f;

    private Vector3 _lastPositionTarget;
    private Quaternion _lastRotationTarget;
    private Vector3 _lastScaleTarget;

    private Transform _lastTarget;
    private Transform _payload;

    void Start()
    {
        _payload = transform.GetChild(0);
        _lastPositionTarget = transform.position;
        _lastRotationTarget = transform.rotation;
        _lastScaleTarget = transform.localScale ;
    }

    void Update()
    {
        if (Target == null)
        {
            transform.position = Vector3.Lerp(transform.position, _lastPositionTarget, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, _lastRotationTarget, RotationLerp);
            transform.localScale = Vector3.Lerp(transform.localScale, _lastScaleTarget, ScaleLerp);
        }
        if (Target != null)
        {
            if (Target != _lastTarget)
            {
                _payload.parent = null;
                transform.position = Target.position;
                transform.rotation = Target.rotation;
                transform.localScale = Target.localScale;
                _payload.parent = transform;
            }
            transform.position = Vector3.Lerp(transform.position, Target.position, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, Target.rotation, RotationLerp);
            transform.localScale = Vector3.Lerp(transform.localScale, Target.localScale, ScaleLerp);
            _lastPositionTarget = Target.position;
            _lastRotationTarget = Target.rotation;
            _lastScaleTarget = Target.localScale;
        }
        _lastTarget = Target;
    }
}
