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
    private float epsilon;

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

    [Header("Wheel")]
    public float wheelRadius;
    public float wheelMass;
    public float steerAngle;
    private Vector3 wheelVelocity;
    private float angularVelocity;
    private float Fx;
    private float Fy;
    public float wheelAngle;
    public float wheelTorque;
    public float wheelP;
    public float slipRatio;
    public float wheelRps; // rotation per second

    [Header("Coefficients")]
    public float forwardFriction;
    public float sidewaysFriction;
    public float rollingFriction;
    public float steerTime;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
        driveshaftRadius = 0.02f;
        wheelTorque = 0;
        angularVelocity = 0;
        wheelMass = 20;
        epsilon = 0.0314f;
        springLength = restLength;
    }

    float MagicFormula(float k, float Fz)
    {
        float B, C, D, E;
        B = 10; C = 1.9f; D = 1; E = 0.97f;
        return Fz * D * Mathf.Sin(C * Mathf.Atan(B * k - E * (B * k * Mathf.Atan(B * k))));
    }

    void SpringCalc(RaycastHit hit)
    {
        lastLength = springLength;
        Vector3 springToHit = hit.point - transform.position;
        if (Vector3.Dot(-rb.transform.up, Vector3.Normalize(springToHit)) > 0.99f)
        {
            springLength = springToHit.magnitude - wheelRadius;
        }
        else
        {
            float angle = Mathf.Acos(Vector3.Dot(-rb.transform.up, Vector3.Normalize(springToHit)));
            /*Debug.Log("szog");
            Debug.Log(angle);
            Debug.Log("rugoirany nagysag");
            Debug.Log(springToHit.magnitude);*/
            float sinAngle = springToHit.magnitude * Mathf.Sin(angle) / wheelRadius;
            sinAngle = Mathf.Clamp(sinAngle, 0, 1);
            float innerAngle = Mathf.PI - Mathf.Asin(sinAngle);
            /*Debug.Log("belso szog");
            Debug.Log(innerAngle);*/
            float outerAngle = Mathf.PI - angle - innerAngle;
            /*Debug.Log("kulso szog");
            Debug.Log(outerAngle);*/
            springLength = wheelRadius * Mathf.Sin(outerAngle) / Mathf.Sin(angle);
            /*springLength = springToHit.magnitude * Mathf.Cos(angle) -
                Mathf.Sqrt(Mathf.Pow(wheelRadius, 2) - Mathf.Pow(springToHit.magnitude, 2) * Mathf.Pow(Mathf.Sin(angle), 2));*/
        }
        /*Debug.Log("rugohossz");
        Debug.Log(springLength);*/
        springLength = Mathf.Clamp(springLength, minLength, maxLength);
        springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        springForce = springStiffness * (restLength - springLength);
        damperForce = damperStiffness * springVelocity;
        suspensionForce = (springForce + damperForce) * transform.up;
        rb.AddForceAtPosition(suspensionForce, hit.point);
    }

    void FrictionCalc(RaycastHit hit)
    {
        wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

        float slipAngle = Mathf.Acos(Vector3.Dot(transform.forward, wheelVelocity.normalized)); // kerék elõre iránya és a sebesség közti szög
        float sprungMass = suspensionForce.magnitude / Physics.gravity.y;

        float wheelSlipVelocity = wheelRadius * angularVelocity - wheelVelocity.magnitude;
        float slipRatio;
        if (wheelVelocity.magnitude == 0)
            slipRatio = 0;
        else
            slipRatio = wheelSlipVelocity / wheelVelocity.magnitude;

        if (isFrontLeft || isFrontRight)
        {
            
            if (wheelTorque != 0)
            {
                Fx = wheelTorque - MagicFormula(slipRatio, springForce);
                angularVelocity += (wheelTorque * wheelRadius / driveshaftRadius - wheelRadius * Fx) / springForce * Time.fixedDeltaTime;
                //angularVelocity = 2 * wheelRps;
            }
            else
            {
                Fx = 0;
                angularVelocity = rb.velocity.magnitude / wheelRadius;
                if (!isGoingForward())
                {
                    angularVelocity *= -1;
                }
            }
        }
        else
        {
            Fx = 0;
            angularVelocity = wheelVelocity.magnitude / wheelRadius;
            if (!isGoingForward())
            {
                angularVelocity *= -1;
            }
            
        }
        Fy = wheelVelocity.x * springForce - wheelVelocity.x * MagicFormula(slipAngle, springForce);

        /*if(isRearRight)
             Debug.Log("rear right");
         if (isRearLeft)
             Debug.Log("rear left");
         if (isFrontLeft)
             Debug.Log("front left");
         if (isFrontRight)
             Debug.Log("front right");
         Debug.Log(angularVelocity);*/

        Vector3 forwardForceDirection = Vector3.Normalize(Vector3.Cross(hit.point - wheelGraphic.position, transform.right));
        rb.AddForceAtPosition(Fx * forwardForceDirection + Fy * -transform.right, hit.point);
    }

    private void UpdateWheel(RaycastHit hit)
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        wheelGraphic.position = transform.position + (-transform.up * springLength);
        wheelGraphic.Rotate(180 / Mathf.PI / 2 * angularVelocity * Time.fixedDeltaTime, 0, 0);
    }

    private bool isGoingForward()
    {
        return Mathf.Sign(Vector3.Dot(rb.velocity, transform.TransformDirection(Vector3.forward))) > 0;
    }

    void FixedUpdate()
    {
        RaycastHit finalHit;
        Physics.Raycast(transform.position, -transform.up, out RaycastHit downwardHit, 100);
        finalHit = downwardHit;
        Debug.DrawRay(transform.position, finalHit.point - transform.position);
        for (int i = 0; i < 180; i++)
        {
            if (Physics.Raycast(transform.position - transform.up * springLength, Quaternion.AngleAxis(i, transform.right) * transform.forward,
                out RaycastHit circleHit, wheelRadius + 0.1f))
            {
                if (circleHit.distance < finalHit.distance) finalHit = circleHit;
                // lehet kell 2 vagy 3 tapadási pont, amire elosztjuk az erõket
            }
        }
        Debug.DrawRay(transform.position, finalHit.point - transform.position);

        SpringCalc(finalHit);
        FrictionCalc(finalHit);
        UpdateWheel(finalHit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * springLength);
    }
}
