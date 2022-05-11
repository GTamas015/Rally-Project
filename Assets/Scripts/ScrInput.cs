using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrInput : MonoBehaviour
{
    public float pedalInput;
    public float steerInput;
    public float handbrake;

    void Start()
    {
        
    }

    void Update()
    {
        pedalInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        handbrake = Input.GetAxis("Jump");
    }
}
