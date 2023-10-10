using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class ObjectRotationHandler : MonoBehaviour
{

    public float horizontalSpeed = 5.0F;
    public float verticalSpeed = 5.0F;
    Transform rotationFixer;
    // Start is called before the first frame update
    void Start()
    {
        rotationFixer = GameObject.Find("rotationfixer").transform;
    }
    
    void Update()
    {
        HandleInspectionRotation();
    }

    void HandleInspectionRotation()
    {
        if (Input.GetMouseButton(0))
        {
            rotationFixer.rotation = Quaternion.identity;
            float xRot = verticalSpeed * Input.GetAxis("Mouse Y") * Camera.main.transform.forward.z;
            float zRot = verticalSpeed * Input.GetAxis("Mouse Y") * Camera.main.transform.forward.x;
            float yRot = horizontalSpeed * Input.GetAxis("Mouse X") ;

            rotationFixer.Rotate(Camera.main.transform.up, -yRot);
            rotationFixer.Rotate(Vector3.right, xRot);
            rotationFixer.Rotate(Vector3.forward, -zRot);



            transform.rotation = rotationFixer.rotation * transform.rotation;


        }
    }

    
}
