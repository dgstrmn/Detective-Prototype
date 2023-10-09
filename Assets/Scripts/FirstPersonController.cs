using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float angleLimit = 80.0f;
    [SerializeField] private float headSwayMultiplier = 1.0f;


    [Header("Inputs Customization")]
    [SerializeField] private string horizontalMoveInput = "Horizontal";
    [SerializeField] private string verticalMoveInput = "Vertical";
    [SerializeField] private string MouseXInput = "Mouse X";
    [SerializeField] private string MouseYInput = "Mouse Y";
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Camera mainCamera;
    private float headSwayAngle = 0;
    private const float TWO_PI = Mathf.PI * 2;
    private float verticalRotation;
    private Vector3 currentMovement = Vector3.zero;
    private CharacterController characterController;
    private Transform interactedObject;
    private Transform lastInteractedObject;
    GameObject textObject;
    private bool isOccupied = false;
    bool playerActive = true;
    Vector3 objectLastPos = Vector3.zero;
    Quaternion objectLastRot = Quaternion.identity;
    enum Mode
    {
        Use, Inspect, Off
    }
    private void Start()
    {
        textObject = GameObject.Find("Text");
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        if (playerActive)
        {
            HandleMovement();
            HandleRotation();
        }
        HandleInteraction();

    }

    void HandleMovement()
    {
        float speedMultiplier = Input.GetKey(sprintKey) ? sprintMultiplier : 1f;

        // Use Input.GetAxisRaw or the character will keep moving a few frames after button release.
        Vector3 horizontalMovement = new Vector3(Input.GetAxisRaw(horizontalMoveInput), 0, Input.GetAxisRaw(verticalMoveInput)).normalized;
        horizontalMovement = transform.rotation * horizontalMovement;

        HandleGravityandJumping();

        currentMovement.x = horizontalMovement.x * walkSpeed * speedMultiplier;
        currentMovement.z = horizontalMovement.z * walkSpeed * speedMultiplier;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    void HandleGravityandJumping()
    {
        if (characterController.isGrounded)
        {

            if (Input.GetKeyDown(jumpKey))
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }

    void HandleRotation()
    {
        float mouseXRotation = Input.GetAxisRaw(MouseXInput) * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);


        verticalRotation -= Input.GetAxisRaw(MouseYInput) * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -angleLimit, angleLimit);
        float headSway = HandleHeadShake();
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, headSway * headSwayMultiplier);
    }

    float HandleHeadShake()
    {
        headSwayMultiplier = Input.GetKey(sprintKey) ? 1.5f : 1f;

        if (Input.GetButton("Vertical") && characterController.isGrounded)
        {
            headSwayAngle += 8f * Time.deltaTime * headSwayMultiplier;
            headSwayAngle %= TWO_PI;
        }
        else
        {
            if (headSwayAngle < Mathf.PI && headSwayAngle > 0)
            {
                headSwayAngle += 8f * Time.deltaTime * headSwayMultiplier;
                if (headSwayAngle >= Mathf.PI)
                {
                    headSwayAngle = 0;
                }
            }
            else if (headSwayAngle > Mathf.PI)
            {
                headSwayAngle += 8f * Time.deltaTime * headSwayMultiplier;
                if (headSwayAngle >= TWO_PI)
                {
                    headSwayAngle = 0;
                }
            }
            else
            {
                headSwayAngle = 0;
            }
        }

        return MathF.Sin(headSwayAngle) / 3f;
    }

    void HandleInteraction()
    {
        if (!isOccupied) //if our hands are empty
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.collider != null)
            {
                interactedObject = hit.collider.transform;
                float distance = Vector3.Distance(interactedObject.position, transform.position);
                if (interactedObject.CompareTag("Inspectable") && distance <= 3.0f)//if an inspectable object is hit and is within 3 meter radius
                {
                    lastInteractedObject = interactedObject;
                    interactedObject.GetComponent<Outline>().enabled = true;
                    HandleUIText(Mode.Inspect);
                    if (Input.GetKeyDown(interactKey))
                    {
                        HandleUIText(Mode.Off);
                        interactedObject.GetComponent<ObjectRotateHandler>().enabled = true;
                        interactedObject.GetComponent<Outline>().enabled = false;
                        objectLastPos = interactedObject.position;
                        objectLastRot = interactedObject.rotation;
                        playerActive = false;
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        isOccupied = true;
                        interactedObject.SetParent(mainCamera.transform);
                        Vector3 interactablePos = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y - 0.75f, mainCamera.transform.localPosition.z + 0.75f);
                        interactedObject.SetLocalPositionAndRotation(interactablePos, Quaternion.Euler(0, -90, 75));
                    }
                }
                else
                {
                    if(lastInteractedObject != null)
                    {
                        lastInteractedObject.GetComponent<Outline>().enabled = false;
                    }
                    HandleUIText(Mode.Off);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(interactKey)) //if the inspectable object is put down
            {
                interactedObject.GetComponent<ObjectRotateHandler>().enabled = false;
                interactedObject.SetPositionAndRotation(objectLastPos, objectLastRot);
                playerActive = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                interactedObject.parent = null;
                isOccupied = false;

            }

        }

    }

    void HandleUIText(Mode mode)
    {
        switch (mode)
        {
            case Mode.Use:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("Press " + interactKey.ToString() + " To Use '" + interactedObject.name + "'");

                break;
            case Mode.Inspect:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("Press " + interactKey.ToString() + " To Inspect '" + interactedObject.name + "'");

                break;
            case Mode.Off:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("");
                break;
        }
    }


}
