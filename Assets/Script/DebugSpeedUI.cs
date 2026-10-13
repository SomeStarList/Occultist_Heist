using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugSpeedUI : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] private CharacterController playerController;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Debug Settings")]
    [Tooltip("ปุ่มเปิด-ปิด UI (ค่าเริ่มต้นคือ F3)")]
    [SerializeField] private Key toggleKey = Key.F3;
    [SerializeField] private bool showOnStart = true;

    private bool isUIVisible;

    private void Awake()
    {
        // ค้นหาคอมโพเนนต์อัตโนมัติหากไม่ได้ลากใส่ใน Inspector
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<CharacterController>();
        }

        if (playerInventory == null && playerController != null)
        {
            playerInventory = playerController.GetComponent<PlayerInventory>();
        }
    }

    private void Start()
    {
        isUIVisible = showOnStart;
        SetUIVisibility(isUIVisible);
    }

    private void Update()
    {
        HandleToggleInput();

        if (isUIVisible)
        {
            UpdateSpeedDisplay();
        }
    }

    private void HandleToggleInput()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isUIVisible = !isUIVisible;
            SetUIVisibility(isUIVisible);
        }
    }

    private void SetUIVisibility(bool visible)
    {
        if (speedText != null)
        {
            speedText.gameObject.SetActive(visible);
        }
    }

    private void UpdateSpeedDisplay()
    {
        if (speedText == null || playerController == null) return;

        // คำนวณความเร็วในแนวระนาบ XZ (ตัดแกน Y แรงโน้มถ่วงออก เพื่อดูความเร็วการเดิน/วิ่งจริง)
        Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0f, playerController.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // ข้อมูลน้ำหนักสำหรับเทียบผล
        float currentWeight = playerInventory != null ? playerInventory.CurrentWeight : 0f;
        float maxWeight = playerInventory != null ? playerInventory.MaxWeight : 10f;

        // รูปแบบการแสดงผล
        speedText.text = $"[DEBUG (F3)]\n" +
                         $"Speed: {currentSpeed:F2} m/s\n" +
                         $"Weight: {currentWeight:F1}/{maxWeight:F1} KG";
    }
}