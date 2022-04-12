using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrWheel : MonoBehaviour
{
    public Rigidbody rb;
    public Transform wheelGraphic;
    public bool isFrontRight;
    public bool isFrontLeft;
    public bool isRearRight;
    public bool isRearLeft;

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springForce;
    private float damperForce;
    private float springVelocity;
    private float driveshaftRadius;

    private Vector3 suspensionForce;
    private Vector3 wheelVelocity;
    private float angularVelocity;
    private float Fx;
    private float Fy;
    private float wheelAngle;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;
    public float steerTime;

    [Header("coefficients")]
    public float staticFriction;
    public float kineticFriction;
    public float rollingFriction;

    void Start()
    {
        //rb = transform.root.GetComponent<Rigidbody>();
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
        driveshaftRadius = 0.02f;
    }

    void SpringCalc(RaycastHit hit)
    {
        lastLength = springLength;
        springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, minLength, maxLength);
        springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        springForce = springStiffness * (restLength - springLength);
        damperForce = damperStiffness * springVelocity;
        suspensionForce = (springForce + damperForce) * transform.up;
    }

    void FrictionCalc(RaycastHit hit)

        //motor torque graph: -(x/500-10)^2+125
    {
        wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
        if (isRearLeft || isRearRight)
        {
            /*float driveshaftForce = Input.GetAxis("Vertical") * springForce;
            float frictionForce;
            if (driveshaftForce > 0)
            {
                frictionForce = rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * staticFriction;
                if (Mathf.Abs(driveshaftForce * driveshaftRadius) > Mathf.Abs(frictionForce * wheelRadius))
                    Fx = rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * kineticFriction;
                else
                    Fx = frictionForce;
            }
            if (driveshaftForce < 0)
            {
                frictionForce = -1.0f * rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * staticFriction;
                if (Mathf.Abs(driveshaftForce * driveshaftRadius) > Mathf.Abs(frictionForce * wheelRadius))
                    Fx = -1.0f * rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * kineticFriction;
                else
                    Fx = frictionForce;
            }
            else
                Fx = 0;
            */
            if (Input.GetAxis("Vertical") > 0)
            {
                Fx = rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * staticFriction;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                Fx = -1.0f * rb.mass * Mathf.Abs(Physics.gravity.y) / 4 * staticFriction;
            }
            else
            {
                Fx = 0;
            }
        }
        Fy = wheelVelocity.x * springForce;

        

        rb.AddForceAtPosition(suspensionForce + Fx * transform.forward + Fy * -transform.right, hit.point);
    }

    private void UpdateWheel(RaycastHit hit)
    {
        wheelGraphic.position = transform.position + (-transform.up * (hit.distance - wheelRadius));
        if (Mathf.Sign(Vector3.Dot(rb.velocity, transform.TransformDirection(Vector3.forward))) < 0)
        {
            wheelGraphic.transform.Rotate(-5 * (wheelVelocity.magnitude / wheelRadius * Mathf.PI * 2) * Time.fixedDeltaTime, 0, 0);
        }
        else
            wheelGraphic.transform.Rotate( 5 * (wheelVelocity.magnitude / wheelRadius * Mathf.PI * 2) * Time.fixedDeltaTime, 0, 0);
    }

    void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        //Debug.DrawRay(transform.position, -transform.up * springLength, Color.green);
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            SpringCalc(hit);
            FrictionCalc(hit);
            UpdateWheel(hit);
            
        }
    }
}
