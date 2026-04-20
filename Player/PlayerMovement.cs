using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementNewInput : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 1f;

    private CharacterController controller;
    private Vector3 velocity;
    private PlayerControls controls;
    private Transform playerCamera;
    private float xRotation = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Animator animator;
    public Transform headBone;
    public float maxHeadYaw = 60f;
    public float maxHeadPitch = 40f;

    //kamera és test yaw különválasztása a fejkorlátozáshoz
    private float bodyYaw = 0f;
    private float cameraYaw = 0f;

    //Speeds (sebességek)
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2.5f;

    //running and crouching (futás és guggolás)
    private bool isRunning = false;
    private bool isCrouching = false;

    //for hearing (halláshoz)
    public enum MovementState { Idle, Walking, Running, Crouching }
    public MovementState CurrentState { get; private set; }

    //for smooth movement (simított mozgáshoz)
    private float smoothMoveX = 0f;
    private float smoothMoveY = 0f;
    public float blendSmoothSpeed = 10f;

    //For camera (kamera)
    public Vector3 cameraOffset = new Vector3(0f, 0f, 0f);

    //For head movement fix (fejmozgás javítása)
    public float headPitchOffset = 0f;

    //Mozgáskövetés a szörny predikciójához
    public Vector3 Velocity { get; private set; }
    private Vector3 lastPosition;

    // Drunk effect
    private float drunkTimer = 0f;
    private float drunkYaw = 0f;
    private float drunkPitch = 0f;
    private float drunkStrength = 0f;

    // Drunk effect for camera
    public float baseDrunkStrength = 2f;

    // Stamina (állóképesség)
    public float maxStamina = 4f;      // Maximum stamina
    public float staminaRegenRate = 1f; // Stamina ami regenerálódik másodpercenként
    public float staminaDrainRate = 1f; // Stamina ami csökken másodpercenként futás közben

    private float currentStamina;
    public bool IsTired => currentStamina <= 0f;

    //UI
    public Slider staminaBar;
    private float staminaRechargeDelay = 1f; // másodpercek a regenerálódás előtt
    private float staminaRechargeTimer = 0f;

    // Flashlight (zseblámpa)
    public Light flashlight;
    private bool flashlightOn = false;
    public FlashlightSway flashlightSway;

    //Death (halál)
    public bool isDead = false;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Run.performed += ctx => isRunning = true;
        controls.Player.Run.canceled += ctx => isRunning = false;

        controls.Player.Crouch.performed += ctx => ToggleCrouch();

        controls.Player.Flashlight.performed += ctx => ToggleFlashlight();
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        if (staminaBar == null)
        staminaBar = GameObject.Find("StaminaBar").GetComponent<Slider>();
        currentStamina = maxStamina; //stamina
        if (flashlight == null)
        flashlight = GetComponentInChildren<Light>();
        if (flashlightSway == null)
        flashlightSway = flashlight.GetComponent<FlashlightSway>();

    // ragdoll fizika és kolliderek letiltása a kezdetekkor
    foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
    {
        if (rb.gameObject != this.gameObject) 
            rb.isKinematic = true;
    }
    foreach (Collider col in GetComponentsInChildren<Collider>())
    {
        if (col.gameObject != this.gameObject && !(col is CharacterController))
            col.enabled = false;
    }

    controller.enabled = true;
    animator.enabled = true;
    }

    void Update()
    {
        if (flashlightSway != null)
        {
            flashlightSway.isRunning = isRunning;
        }

        if (isRunning && moveInput.magnitude > 0.01f && !IsTired)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0f) currentStamina = 0f;
            staminaRechargeTimer = staminaRechargeDelay;
        }
        else
        {
            if (staminaRechargeTimer > 0f)
            {
                staminaRechargeTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }

        // Futás leállítása, ha elfogy a stamina
        if (IsTired)
        {
            isRunning = false;
        }

        // Update stamina UI
        //staminaBar.value = currentStamina / maxStamina;
        //hpBar.value = currentHP / maxHP;

        // --- Movement ---
        Vector3 camForward = playerCamera.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCamera.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;

        // testel a kamerával együtt fordul, hogy ne legyen túl merev, de csak akkor, ha van mozgás
        if (moveInput.magnitude > 0.01f)
        {
            // szépen követi a kamera yaw-ját, de simítva, hogy ne legyen túl hirtelen
            float turnSpeed = 360f;
            bodyYaw = Mathf.MoveTowardsAngle(bodyYaw, cameraYaw, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
        }

        float currentSpeed = walkSpeed;
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isRunning)
            currentSpeed = runSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Mozgás állapotának meghatározása
        if (moveInput.magnitude < 0.01f)
        {
            CurrentState = MovementState.Idle;
        }
        else if (isCrouching)
        {
            CurrentState = MovementState.Crouching;
        }
        else if (isRunning)
        {
            CurrentState = MovementState.Running;
        }
        else
        {
            CurrentState = MovementState.Walking;
        }

        if (flashlightSway != null)
        {
            flashlightSway.isRunning = (CurrentState == MovementState.Running);
            flashlightSway.isWalking = (CurrentState == MovementState.Walking);
        }
        
        // --- Animation ---
        if (animator != null)
        {
            Vector3 localMove = transform.InverseTransformDirection(move.normalized);

            float targetMoveX = Mathf.Clamp(localMove.x * move.magnitude, -1f, 1f);
            float targetMoveY = 0f;

            if (isCrouching)
                targetMoveY = Mathf.Clamp(Mathf.Abs(localMove.z * move.magnitude), 0f, 1f);
            else if (isRunning)
                targetMoveY = Mathf.Clamp(Mathf.Abs(localMove.z * move.magnitude * (runSpeed / walkSpeed)), 0f, 2f);
            else
                targetMoveY = Mathf.Clamp(Mathf.Abs(localMove.z * move.magnitude), 0f, 1f);

            
            smoothMoveX = Mathf.Lerp(smoothMoveX, targetMoveX, Time.deltaTime * blendSmoothSpeed);
            smoothMoveY = Mathf.Lerp(smoothMoveY, targetMoveY, Time.deltaTime * blendSmoothSpeed);

            animator.SetFloat("MoveX", smoothMoveX);
            animator.SetFloat("MoveY", smoothMoveY);
            animator.SetBool("IsCrouching", isCrouching);
        }

        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Mouse Look ---
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Vertical look (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxHeadPitch, maxHeadPitch);

        // Horizontal look (yaw)
        cameraYaw += mouseX;

        // --- Drunk Effect (after mouse look) ---
        float currentDrunkStrength = baseDrunkStrength;
        if (drunkTimer > 0f)
        {
            float t = drunkTimer / 3f;
            currentDrunkStrength += drunkStrength * t;
            drunkTimer -= Time.deltaTime;
        }
        if (isRunning)
        {
            currentDrunkStrength *= 1.5f; // Futás közben erősebb a hatás
        }
        drunkYaw = Mathf.Sin(Time.time * 8f) * currentDrunkStrength;
        drunkPitch = Mathf.Cos(Time.time * 6f) * currentDrunkStrength;
        cameraYaw += drunkYaw * Time.deltaTime;
        xRotation += drunkPitch * Time.deltaTime;

        // kalkulálja a fej yaw-ját a testhez képest, hogy ne forduljon túl messze
        float relativeYaw = Mathf.DeltaAngle(transform.eulerAngles.y, cameraYaw);

        // ha a fej túllépné a limitet, forgassa a testet, hogy kövesse
        if (relativeYaw > maxHeadYaw)
        {
            float excess = relativeYaw - maxHeadYaw;
            transform.Rotate(Vector3.up * excess, Space.World);
        }
        else if (relativeYaw < -maxHeadYaw)
        {
            float excess = relativeYaw + maxHeadYaw;
            transform.Rotate(Vector3.up * excess, Space.World);
        }
        // Sebesség nyomon követése (a szörny predikciójához)
        Velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Kamera forgatás alkalmazása
        playerCamera.localRotation = Quaternion.Euler(xRotation, cameraYaw - transform.eulerAngles.y, 0f);
    }

    //metódus hit reakcióhoz, hogy a nézés irányát megváltoztassa
    public void AddLookOffset(float yawOffset, float pitchOffset)
    {
        Debug.Log($"AddLookOffset called: yaw {yawOffset}, pitch {pitchOffset}");
        cameraYaw += yawOffset;
        xRotation += pitchOffset;
    }

    //ütéskor részeg effekt hozzáadása
    public void TriggerDrunkEffect(float strength, float duration)
    {
        drunkStrength = strength;
        drunkTimer = duration;
    }

    // Zseblámpa
    private void ToggleFlashlight()
    {
        flashlightOn = !flashlightOn;
        if (flashlight != null)
            flashlight.enabled = flashlightOn;
    }

    //DEATH SEQUENCE//
    public void Die()
    {
        if (isDead) return;      // already dead -> do nothing
        isDead = true;

        // 1) Input akciók letiltása, hogy a játékos ne tudjon mozogni vagy nézelődni
        if (controls != null) controls.Player.Disable();

        // 2) CharacterController letiltása, hogy ne zavarja a mozgást/ütközést
        if (controller != null) controller.enabled = false;

        // 3) Fő kolliderek letiltása, hogy az AI/nav/physics ne kezelje a játékost mozgó akadályként
        var mainCol = GetComponent<Collider>();
        if (mainCol != null) mainCol.enabled = false;

        // 4) Animator "Dead" bool beállítása, hogy átmenjen a halál animációra -> halott loop
        //    (Az Animatornek rendelkeznie kell egy átmenettel a locomotion -> Death között a "Dead" bool használatával)
        if (animator != null)
        {
            animator.SetBool("Dead", true);
            animator.enabled = true;
        }
    }


/* Spectate mode (további fejlesztéshez hasznos lehet, jelenleg nincs használva)
private IEnumerator FadeVignetteAndSpectate()
{
    // Example: fade vignette to black over 2 seconds
    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime / 2f;
        // Set vignette intensity here (using your post-processing reference)
        yield return null;
    }

    // 4. Enter spectate mode
    EnterSpectateMode();
}

private void EnterSpectateMode()
{
    // Enable ghost movement/camera
    // Disable ragdoll visuals
    // Set player to non-colliding, maybe transparent
}
*/
    void LateUpdate()
    {
        // --- Head Look ---
        if (headBone != null && playerCamera != null)
        {
            playerCamera.position = headBone.position + headBone.TransformVector(cameraOffset);
            float clampedPitch = Mathf.Clamp(xRotation, -maxHeadPitch, maxHeadPitch) + headPitchOffset;
            float relativeYaw = Mathf.DeltaAngle(transform.eulerAngles.y, cameraYaw);
            float clampedYaw = Mathf.Clamp(relativeYaw, -maxHeadYaw, maxHeadYaw);

            headBone.localRotation = Quaternion.Euler(clampedPitch, clampedYaw, 0f);
        }
    }
}