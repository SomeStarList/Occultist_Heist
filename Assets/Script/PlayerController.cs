using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Weight Penalty Settings")]
    [Tooltip("ความเร็วต่ำสุดเมื่อแบกน้ำหนักเต็ม 10KG (เช่น 0.5 คือลดเหลือ 50% ของความเร็วปกติ)")]
    [Range(0.2f, 1.0f)]
    [SerializeField] private float minSpeedMultiplier = 0.5f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.0f;
    [SerializeField] private TextMeshProUGUI staminaText;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upDownLimit = 85f;
    private bool isLookLocked = false;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchingHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private Vector3 standingCameraPos = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 crouchingCameraPos = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private LayerMask obstacleLayer = ~0;

    [Header("Intro Settings")]
    [SerializeField] private GameObject characterModel;
    [SerializeField] private Vector3 introCameraOffset = new Vector3(0f, 0.4f, -2.5f);
    [SerializeField] private float introDuration = 2.0f;

    private CharacterController controller;
    private PlayerInventory inventory; // อ้างอิงระบบกระเป๋า
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private float verticalRotation = 0f;
    private bool isIntroFinished = false;

    // ตัวแปรสถานะ
    private bool isCrouching = false;
    private bool isSprinting = false;
    private float currentHeight;
    private float currentStamina;
    private float regenTimer = 0f;

    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public float CurrentStamina => currentStamina;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inventory = GetComponent<PlayerInventory>(); // ดึงคอมโพเนนต์กระเป๋า
        currentHeight = standingHeight;
        currentStamina = maxStamina;

        controller.height = standingHeight;
        controller.center = new Vector3(0f, standingHeight * 0.5f, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = standingCameraPos;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        isLookLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateStaminaUI();
        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        ApplyGravity();

        if (!isIntroFinished) return;

        ReadInputs();
        HandleLook();
        HandleCrouch();
        HandleStamina();
        HandleMovement();
    }

    private IEnumerator PlayIntroSequence()
    {
        if (cameraTransform == null)
        {
            isIntroFinished = true;
            yield break;
        }

        Vector3 finalLocalPos = standingCameraPos;
        Vector3 startLocalPos = finalLocalPos + introCameraOffset;

        cameraTransform.localPosition = startLocalPos;
        cameraTransform.localRotation = Quaternion.identity;

        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            t = t * t * (3f - 2f * t);

            cameraTransform.localPosition = Vector3.Lerp(startLocalPos, finalLocalPos, t);
            yield return null;
        }

        cameraTransform.localPosition = finalLocalPos;

        if (characterModel != null)
        {
            characterModel.SetActive(false);
        }

        isIntroFinished = true;
    }

    private void ReadInputs()
    {
        if (Keyboard.current != null)
        {
            Vector2 rawInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) rawInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) rawInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) rawInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) rawInput.x += 1f;
            moveInput = rawInput.normalized;

            bool wantsToCrouch = Keyboard.current.cKey.isPressed || Keyboard.current.leftCtrlKey.isPressed;
            isCrouching = wantsToCrouch || !CanStandUp();

            bool wantsToSprint = Keyboard.current.leftShiftKey.isPressed;
            isSprinting = wantsToSprint && !isCrouching && moveInput.magnitude > 0.1f && currentStamina > 0f;
        }

        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity;
        }
    }

    private void HandleStamina()
    {
        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isSprinting = false;
            }
            regenTimer = 0f;
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= staminaRegenDelay)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    if (currentStamina > maxStamina)
                    {
                        currentStamina = maxStamina;
                    }
                }
            }
        }

        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaText != null)
        {
            staminaText.text = $"Stamina : {Mathf.CeilToInt(currentStamina)}/{Mathf.CeilToInt(maxStamina)}";
        }
    }

    private bool CanStandUp()
    {
        Vector3 sphereCastStart = transform.position + Vector3.up * crouchingHeight;
        float checkDistance = standingHeight - crouchingHeight;
        return !Physics.SphereCast(sphereCastStart, controller.radius * 0.85f, Vector3.up, out _, checkDistance, obstacleLayer);
    }

    private void HandleLook()
    {
        // ถ้าระบบโดนล็อกอยู่ (เช่น ตอนขึ้นหน้าต่างจบภารกิจ) ให้ข้ามการหมุนกล้องไปเลย
        if (isLookLocked) return;

        transform.Rotate(Vector3.up * lookInput.x);

        verticalRotation -= lookInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownLimit, upDownLimit);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    // ฟังก์ชันสำหรับสั่งล็อก/ปลดล็อกการหันหน้า
    public void SetLockLook(bool isLocked)
    {
        isLookLocked = isLocked;

        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        Vector3 targetCameraPos = isCrouching ? crouchingCameraPos : standingCameraPos;

        currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        controller.height = currentHeight;
        controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = Vector3.MoveTowards(cameraTransform.localPosition, targetCameraPos, crouchTransitionSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (!isIntroFinished)
        {
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }
    }

    // คำนวณตัวคูณความเร็วจากน้ำหนักปัจจุบัน
    private float GetWeightSpeedMultiplier()
    {
        if (inventory == null || inventory.MaxWeight <= 0f) return 1f;

        // อัตราส่วนน้ำหนักปัจจุบัน 0.0 ถึง 1.0
        float weightRatio = Mathf.Clamp01(inventory.CurrentWeight / inventory.MaxWeight);

        // คำนวณความเร็วลดหลั่นลงมา: 0KG = 1.0 (100%), 10KG = minSpeedMultiplier
        return Mathf.Lerp(1.0f, minSpeedMultiplier, weightRatio);
    }

    private void HandleMovement()
    {
        float baseSpeed;
        if (isCrouching)
        {
            baseSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            baseSpeed = sprintSpeed;
        }
        else
        {
            baseSpeed = walkSpeed;
        }

        // ปรับความเร็วตามน้ำหนักกระเป๋า
        float finalSpeed = baseSpeed * GetWeightSpeedMultiplier();

        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x);
        Vector3 finalMovement = (moveDirection * finalSpeed) + (Vector3.up * verticalVelocity);

        controller.Move(finalMovement * Time.deltaTime);
    }
}