using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrEngine : MonoBehaviour
{
    private ScrCarController carContoller;
    private ScrInput input;
    private int shiftTime;
    public AnimationCurve engineTorqueCurve;

    public int gear;
    private float[] gearRatio = { 3.06f, 3.3f, 1.94f, 1.31f, 1.03f, 0.84f };
    private float transmissionToWheelRatio;
    private float transmissionOutputTorque;

    private float[] angles = new float[4];
    private float engineTorque;
    private float closerToApexWheelTorque;
    private float furtherFromApexWheelTorque;

    [SerializeField] private float rpm;
    private float idleRpm;
    private float maxRpm;
    private float engineBrake;
    private float breakTorque;
    private float performance = 100000;

    void Start()
    {
        carContoller = GetComponent<ScrCarController>();
        input = GetComponent<ScrInput>();
        idleRpm = 800;
        maxRpm = 6500;
        rpm = 800;
        engineBrake = 0;
        engineTorque = 0;
        transmissionToWheelRatio = 3.94f;
        gear = 1;
        breakTorque = 0;
        shiftTime = 0;
    }

    private void Update()
    {

    }

    void FixedUpdate()
    {
        // jobb lenne, ha a gázpedál a leadott nyomaték mértékét befolyásolná, nem a fordulatszámot

        if (shiftTime == 0) // ha nem váltunk
        {
            if (input.pedalInput > 0) // ha elõre akarunk menni
            {
                if (isGoingForward()) // ha elõre megyünk
                {
                    if (gear == 0)
                    {
                        gear = 1;
                        rpm = 800;
                    }
                    rpm += input.pedalInput / gear * 1000 * Time.fixedDeltaTime;
                    breakTorque = 0;
                    engineTorque = engineTorqueCurve.Evaluate(rpm);
                }
                else // ha hátra megyünk
                {
                    rpm -= input.pedalInput / gear * rpm * Time.fixedDeltaTime;
                    breakTorque = input.pedalInput * 200;
                    engineTorque = 0;
                }
                
                //engineTorque = -Mathf.Pow((rpm / 500 - 10), 2) + 125;
            }
            else if (input.pedalInput < 0) // ha hátra akarunk menni
            {
                if (isGoingForward()) // ha elõre megyünk
                {
                    rpm += input.pedalInput  * rpm * Time.fixedDeltaTime;
                    breakTorque = input.pedalInput * 200;
                    engineTorque = 0;
                }
                else // ha hátra megyünk
                {
                    gear = 0;
                    rpm += -input.pedalInput * 1000 * Time.fixedDeltaTime;
                    breakTorque = 0;
                    engineTorque = -engineTorqueCurve.Evaluate(rpm);
                }
               
                //engineTorque = -1 * (-Mathf.Pow((rpm / 500 - 10), 2) + 125);
            }
            else // ha nem nyomunk semmit
            {
                rpm -= rpm / 100 * Time.fixedDeltaTime;
                engineTorque = 0;
                breakTorque = 0;
                if (isGoingForward())
                    engineBrake = -rpm / 100;
                else
                    engineBrake = rpm / 100;
            }
            if (rpm > 6500 && gear >= 1 && gear < 5) // ha elérjük a fordulatszámláló tetejét
            {
                shiftTime++;
                gear++;
                rpm -= 1500;
            }

            if (rpm < 3500 && gear > 1) // ha elérjük a fordulatszámláló alját
            {
                shiftTime++;
                gear--;
                rpm += 1500;
            }

            if (rpm < idleRpm)
                rpm = idleRpm;
            if (rpm > maxRpm)
                rpm = maxRpm;
        }
        else // ha váltunk
        {
            engineTorque = 0;
            breakTorque = 0;
            shiftTime++;
            if(isGoingForward() && input.pedalInput < 0 || !isGoingForward() && input.pedalInput > 0) // ha váltás közben fékezünk
            {
                breakTorque = input.pedalInput * 200;
            }
        }
        if (shiftTime == 50) // fél másodperc a váltás
            shiftTime = 0;

        foreach (ScrWheel i in carContoller.wheels)
        {
            transmissionOutputTorque = (engineTorque + breakTorque) * gearRatio[gear];
            closerToApexWheelTorque = transmissionOutputTorque * transmissionToWheelRatio * (-Mathf.Pow(i.wheelAngle / 3, 2) + 50) / 100;
            furtherFromApexWheelTorque = transmissionOutputTorque * transmissionToWheelRatio * (1 - (-Mathf.Pow(i.wheelAngle / 3, 2) + 50) / 100);

            if ((i.isFrontLeft || i.isFrontRight) && input.steerInput == 0) // FWD, RWD, AWD megoldása utolsónak
            {
                Debug.Log("kerek:");
                Debug.Log(engineTorque);
                Debug.Log(transmissionOutputTorque);
                //Debug.Log(breakTorque);
                //Debug.Log(transmissionOutputTorque);
                i.wheelTorque = transmissionOutputTorque * transmissionToWheelRatio / 2;
                i.wheelRps = rpm / i.wheelTorque / 60;
                //i.wheelP = performance / 2;
            }
            if (i.isFrontLeft && input.steerInput < 0)
            {
                i.wheelTorque = closerToApexWheelTorque;
                i.wheelRps = rpm / i.wheelTorque / 60;
            }
            if (i.isFrontRight && input.steerInput > 0)
            {
                i.wheelTorque = closerToApexWheelTorque;
                i.wheelRps = rpm / i.wheelTorque / 60;
            }
            if (i.isFrontLeft && input.steerInput > 0)
            {
                i.wheelTorque = furtherFromApexWheelTorque;
                i.wheelRps = rpm / i.wheelTorque / 60;
            }
            if (i.isFrontRight && input.steerInput < 0)
            {
                i.wheelTorque = furtherFromApexWheelTorque;
                i.wheelRps = rpm / i.wheelTorque / 60;
            }
        }
    }

    private bool isGoingForward()
    {
        return Mathf.Sign(Vector3.Dot(GetComponentInParent<Rigidbody>().velocity, transform.TransformDirection(Vector3.forward))) > 0;
    }
}
