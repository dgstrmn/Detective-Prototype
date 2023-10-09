using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotateHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        y_axis = Camera.main.transform.up;
        x_axis = Camera.main.transform.right;
    }
    public float horizontalSpeed = 5.0F;
    public float verticalSpeed = 5.0F;
    Vector3 y_axis = Vector3.one;
    Vector3 x_axis = Vector3.one;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");
            transform.RotateAround(transform.position, y_axis, -h);
            transform.RotateAround(transform.position, x_axis, v);
        }

    }
}
