using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrCarController : MonoBehaviour
{
    public ScrWheel[] wheels;

    [Header("Car Specs")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;

    private float steerInput;
    private float ackermannAngleLeft;
    private float ackermannAngleRight;

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        if(steerInput > 0)
        {
            ackermannAngleLeft = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
            ackermannAngleRight = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
        }
        else if(steerInput < 0)
        {
            ackermannAngleLeft = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
            ackermannAngleRight = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach(ScrWheel w in wheels)
        {
            if(w.isFrontLeft)
                w.steerAngle = ackermannAngleLeft;
            if(w.isFrontRight)
                w.steerAngle = ackermannAngleRight;
        }
    }
}
