using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRotateHandler : MonoBehaviour
{
    // Start is called before the first frame update


    float speed = 50.0f;
    float turnAngle;
    float currentRotation;
    float desiredRotation;
    bool isClosed;

    void Start()
    {
        currentRotation = transform.rotation.y;
        desiredRotation = transform.rotation.y - 90;
        isClosed = true;

    }
    void Update()
    {
        if (isClosed)
        {
            if (currentRotation > desiredRotation)
            {
                turnAngle = speed * Time.deltaTime;
                transform.RotateAround(transform.GetChild(0).position, Vector3.up, turnAngle);
                currentRotation -= turnAngle;

            }
            else
            { 
                transform.RotateAround(transform.GetChild(0).position, Vector3.up, currentRotation - desiredRotation);
                isClosed = false;
            }
        }
        else
        {
            transform.GetComponent<DoorRotateHandler>().enabled = false;
        }

        
    }
}

