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
    GameObject crosshair;
    Transform keychain;
    private bool isOccupied = false;
    bool playerActive = true;
    Vector3 objectLastPos = Vector3.zero;
    Quaternion objectLastRot = Quaternion.identity;
    public AudioSource src;
    public AudioClip sfx1, sfx2, sfx3, sfx4, sfx5;
    enum Mode
    {
        Use, Inspect, Off
    }
    private void Start()
    {
        crosshair = GameObject.Find("Crosshair Group");
        textObject = GameObject.Find("Text");
        keychain = transform.Find("KeyChain");
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
        HandleUsage();

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
                if (interactedObject.CompareTag("Inspectable") && distance <= 2.0f)//if an inspectable object is hit and is within 3 meter radius
                {
                    lastInteractedObject = interactedObject;
                    lastInteractedObject.GetComponent<Outline>().enabled = true;
                    HandleUIText(Mode.Inspect);
                    if (Input.GetKeyDown(interactKey))
                    {
                        crosshair.SetActive(false);
                        HandleUIText(Mode.Off);
                        lastInteractedObject.GetComponent<ObjectRotationHandler>().enabled = true;
                        lastInteractedObject.GetComponent<Outline>().enabled = false;
                        objectLastPos = lastInteractedObject.position;
                        objectLastRot = lastInteractedObject.rotation;
                        playerActive = false;
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        isOccupied = true;
                        lastInteractedObject.SetParent(mainCamera.transform);
                        Vector3 interactablePos = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y - 0.75f, mainCamera.transform.localPosition.z + 0.75f);
                        lastInteractedObject.SetLocalPositionAndRotation(interactablePos, Quaternion.Euler(0, -90, 75));
                    }
                }
                else
                {
                    if (lastInteractedObject != null)
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
                lastInteractedObject.GetComponent<ObjectRotationHandler>().enabled = false;
                lastInteractedObject.SetPositionAndRotation(objectLastPos, objectLastRot);
                playerActive = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                lastInteractedObject.parent = null;
                isOccupied = false;
                crosshair.SetActive(true);

            }

        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleInspectableSecrets();
        }


    }

    void HandleUIText(Mode mode)
    {
        switch (mode)
        {
            case Mode.Use:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("Press " + interactKey.ToString() + " To Interact '" + interactedObject.name + "'");

                break;
            case Mode.Inspect:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("Press " + interactKey.ToString() + " To Inspect '" + interactedObject.name + "'");

                break;
            case Mode.Off:
                textObject.transform.GetComponent<TMPro.TextMeshProUGUI>().SetText("");
                break;
        }
    }


    void HandleInspectableSecrets()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.transform.parent == interactedObject)
            {
                Debug.Log("You have found a secret");
                src.clip = sfx4;
                src.Play();
            }

        }
    }

    void HandleUsage()
    {
        if (!isOccupied)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit) && hit.collider != null)
            {
                interactedObject = hit.collider.transform;
                float distance = Vector3.Distance(interactedObject.position, transform.position);

                if (distance <= 2.0f)
                {
                    if (interactedObject.CompareTag("Door"))
                    {
                        lastInteractedObject = interactedObject;
                        lastInteractedObject.GetComponent<Outline>().enabled = true;
                        HandleUIText(Mode.Use);
                        if (Input.GetKeyDown(interactKey))
                        {
                            Transform key = transform.Find(lastInteractedObject.name + " Key");
                            if (key != null)
                            {
                                key.SetPositionAndRotation(lastInteractedObject.Find("Keyhole").position, Quaternion.Euler(-90, 0, 0));
                                key.parent = lastInteractedObject;
                                key.GetComponent<MeshCollider>().enabled = false;
                                key.name = "Key";
                                lastInteractedObject.GetComponent<DoorRotateHandler>().enabled = true;
                                Debug.Log("Opened a door");
                                src.clip = sfx1;
                                src.Play();

                            }
                            else
                            {
                                if (interactedObject.Find("Key"))
                                {
                                    Debug.Log("Door is already opened");
                                    src.clip = sfx3;
                                    src.Play();
                                }
                                    
                                else
                                {
                                    
                                    Debug.Log("You don't have the required key...");
                                    src.clip = sfx2;
                                    src.Play();
                                }
                                    
                            }
                        }
                    }
                    else if (interactedObject.CompareTag("Key"))
                    {
                        lastInteractedObject = interactedObject;
                        lastInteractedObject.GetComponent<Outline>().enabled = true;
                        HandleUIText(Mode.Use);
                        if (Input.GetKeyDown(interactKey))
                        {
                            Debug.Log("Added Key");
                            src.clip = sfx5;
                            src.Play();
                            interactedObject.SetPositionAndRotation(keychain.position, Quaternion.identity);
                            interactedObject.parent = transform;
                        }
                    }
                }


            }
        }

    }


}
