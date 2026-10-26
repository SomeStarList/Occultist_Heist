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
    [SerializeField] private Key toggleKey = Key.F3;
    [SerializeField] private bool showOnStart = true;

    private bool isUIVisible;

    private void Awake()
    {
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

        Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0f, playerController.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        float currentWeight = playerInventory != null ? playerInventory.CurrentWeight : 0f;
        float maxWeight = playerInventory != null ? playerInventory.MaxWeight : 10f;

        speedText.text = $"[DEBUG (F3)]\n" + $"Speed: {currentSpeed:F2} m/s\n" + $"Weight: {currentWeight:F1}/{maxWeight:F1} KG";
    }
}