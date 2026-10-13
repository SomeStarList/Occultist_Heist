using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("UI Prompts")]
    [SerializeField] private TextMeshProUGUI promptText;

    private PlayerInventory inventory;
    private LootItem currentTargetItem;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    private void CheckForInteractable()
    {
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            // ตรวจหา LootItem ทั้งที่ตัวมันเอง และตัวพ่อ (Parent)
            LootItem item = hit.collider.GetComponent<LootItem>();
            if (item == null)
            {
                item = hit.collider.GetComponentInParent<LootItem>();
            }

            if (item != null)
            {
                currentTargetItem = item;
                UpdatePromptUI(item);
                return;
            }
        }

        currentTargetItem = null;
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void UpdatePromptUI(LootItem item)
    {
        if (promptText == null) return;

        promptText.gameObject.SetActive(true);

        // จัดการแสดงผลทศนิยมของน้ำหนักไอเทมที่เล็งอยู่
        string itemWeightStr = (item.Weight % 1 == 0) ? item.Weight.ToString("F0") : item.Weight.ToString("F1");

        // ตรวจสอบเงื่อนไขว่าเก็บได้หรือไม่
        if (inventory.CanCarry(item.Weight, out string reason))
        {
            promptText.color = Color.white;
            promptText.text = $"(E) {item.ItemName} {itemWeightStr}KG";
        }
        else
        {
            // ถ้าน้ำหนักเกินหรือช่องเต็ม ให้ขึ้นเตือนสีแดงพร้อมระบุชื่อและน้ำหนักของไอเทมนั้น
            promptText.color = Color.red;
            promptText.text = $"{reason} {item.ItemName} {itemWeightStr}KG";
        }
    }

    private void HandleInteractionInput()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentTargetItem != null)
            {
                inventory.TryCollectLoot(currentTargetItem);
            }
        }
    }
}