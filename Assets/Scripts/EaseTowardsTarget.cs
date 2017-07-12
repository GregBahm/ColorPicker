using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseTowardsTarget : MonoBehaviour 
{
    public Transform Target;
    public float EaseRate = 0.1f;

    void Update () 
    {
        if(Target == null)
        {
            return;
        }
        Vector3 newPos = Vector3.Lerp(transform.position, Target.position, EaseRate);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, Target.rotation, EaseRate);
        Vector3 newScale = Vector3.Lerp(transform.localScale, Target.localScale, EaseRate);
        transform.position = newPos;
        transform.rotation = newRotation;
        transform.localScale = newScale;
	}
}
