using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrCarController : MonoBehaviour
{
    public ScrWheel[] wheels;
    private Rigidbody rb;
    private ScrInput input;

    [Header("Car Specs")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<ScrInput>();
        rb.centerOfMass = new Vector3(0.1f, -0.2f, 0);
    }

    private void FixedUpdate()
    {
        // menetstabilizátor van hátra, sebességtõl függõen ne lehessen túl nagy kanyarodási szöget állítani a keréknek
        if (input.steerInput > 0) // ha jobbra kanyarodunk
        {
            ackermannAngleLeft = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * input.steerInput;
            ackermannAngleRight = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * input.steerInput;
        }
        else if (input.steerInput < 0)
        {
            ackermannAngleLeft = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * input.steerInput;
            ackermannAngleRight = 3 * Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * input.steerInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach (ScrWheel w in wheels)
        {
            if (w.isFrontLeft)
                w.steerAngle = ackermannAngleLeft;
            if (w.isFrontRight)
                w.steerAngle = ackermannAngleRight;
        }
    }
}