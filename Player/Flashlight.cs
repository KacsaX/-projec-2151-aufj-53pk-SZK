using UnityEngine;

public class FlashlightSway : MonoBehaviour
{
    public Transform playerCamera;
    public float swayAmount = 0.2f;
    public float swaySpeed = 4f;
    public float delaySpeed = 8f;
    private PlayerControls controls;
    private Vector2 lookInput;
    private Vector3 initialLocalPos;
    private Quaternion targetRotation;
    private Quaternion currentRotation;

    public bool isRunning = false; 
    public bool isWalking = false;
    private float currentPan = 0f;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        currentRotation = transform.localRotation;
        if (playerCamera == null)
            playerCamera = Camera.main.transform;
    }

    void Awake()
    {
        controls = new PlayerControls();
        controls.Enable();
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        // követi a kamera mozgását, de késleltetéssel, hogy ne legyen túl merev
        targetRotation = playerCamera.rotation;
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * delaySpeed);
        transform.rotation = currentRotation;

        // Jobbra,balra kilengés a mozgás irányától függően, futásnál nagyobb kilengés
        float swingAmount = isRunning ? 0.4f : (isWalking ? 0.2f : 0f);
        float swingSpeed = isRunning ? 10f : (isWalking ? 6f : 0f);

        float swing = (isRunning || isWalking) ? Mathf.Sin(Time.time * swingSpeed) * swingAmount : 0f;

        // Lefelé billenés futás közben
        float targetPan = isRunning ? -2f : 0f;
        currentPan = Mathf.Lerp(currentPan, targetPan, Time.deltaTime * 6f);

        // Alkalmazza a kilengést (oldalirányban) és a lefelé billenést
        Vector3 offset = playerCamera.forward * 0.2f
            + playerCamera.up * (-0.1f + currentPan)
            + playerCamera.right * swing;

        transform.position = playerCamera.position + offset;
    }
}