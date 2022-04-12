using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrCamera : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 offset;
    public float followSpeed;
    public float lookSpeed;

    public void LookAtTarget()
    {
        Vector3 lookDirection = objectToFollow.position - transform.position;
        lookDirection.y += 1.0f;
        Quaternion rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, lookSpeed * Time.deltaTime);
    }

    public void MoveToTarget()
    {
        Vector3 targetPosition = objectToFollow.position + objectToFollow.forward * offset.z + objectToFollow.right * offset.x + objectToFollow.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        LookAtTarget();
        MoveToTarget();
    }
}
