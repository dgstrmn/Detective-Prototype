using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotateHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    Vector3 y_axis = new Vector3(0, 1, 0);
    Vector3 x_axis = new Vector3(1, 0, 0);
    void Update()
    {
        if(Cursor.lockState == CursorLockMode.Confined)
        {
            if (Input.GetMouseButton(0))
            {
                float h = horizontalSpeed * Input.GetAxis("Mouse X");
                float v = verticalSpeed * Input.GetAxis("Mouse Y");
                transform.RotateAround(transform.position, y_axis, -h);
                transform.RotateAround(transform.position, x_axis, -v);
            }
        }
        
        
    }
}
