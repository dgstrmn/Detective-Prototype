using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotateHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float horizontalSpeed = 5.0F;
    public float verticalSpeed = 5.0F;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");

            transform.Rotate(v * Camera.main.transform.forward.z, -h, -v * Camera.main.transform.forward.x, Space.World); //By default it is Space.Self and you do not need to include that value
        }

    }

    void HandleInspectableSecrets()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && hit.collider != null)
        {

        }
    }
}
