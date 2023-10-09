using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class ObjectRotationHandler : MonoBehaviour
{

    public float horizontalSpeed = 5.0F;
    public float verticalSpeed = 5.0F;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void Update()
    {
        HandleInspectionRotation();
    }

    void HandleInspectionRotation()
    {
        if (Input.GetMouseButton(0))
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");

            float xRot = v * Camera.main.transform.forward.z;
            float zRot = -v * Camera.main.transform.forward.x;
            if(Mathf.Abs(xRot) < 1f && Mathf.Abs(zRot) < 1f)
            {
                xRot *= 5;
                zRot *= 5;
            }
            transform.Rotate(xRot, -h, zRot, Space.World); //By default it is Space.Self and you do not need to include that value
            Debug.Log(v * Camera.main.transform.forward.z + " " + -v * Camera.main.transform.forward.x + " " + -h);
        }
    }

    
}
