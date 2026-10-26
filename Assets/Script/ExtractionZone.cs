using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ExtractionZone : MonoBehaviour
{
    [Header("Extraction Settings")]
    [SerializeField] private int totalLootValue = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI zonePromptText;
    [SerializeField] private GameObject endMissionPanel;
    [SerializeField] private TextMeshProUGUI summaryText;

    private bool isPlayerInZone = false;
    private PlayerInventory playerInventory;

    public int TotalLootValue => totalLootValue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerInventory>() != null)
        {
            isPlayerInZone = true;
            playerInventory = other.GetComponent<PlayerInventory>();
            UpdateZonePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerInventory>() != null)
        {
            isPlayerInZone = false;
            playerInventory = null;

            if (zonePromptText != null)
            {
                zonePromptText.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!isPlayerInZone || playerInventory == null) return;

        UpdateZonePrompt();
        HandleInputs();
    }

    private void UpdateZonePrompt()
    {
        if (zonePromptText == null) return;

        zonePromptText.gameObject.SetActive(true);

        if (playerInventory.Items.Count > 0)
        {
            zonePromptText.text = $"[E] Deposit Loot \nTotal Stash: ${totalLootValue}";
        }
        else
        {
            zonePromptText.text = $"[F] Escape Mission \nTotal Stash: ${totalLootValue}";
        }
    }

    private void HandleInputs()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && playerInventory.Items.Count > 0)
        {
            DepositAllLoot();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            EscapeMission();
        }
    }

    private void DepositAllLoot()
    {
        int depositedValue = 0;
        int itemAmount = playerInventory.Items.Count;

        var itemsList = new System.Collections.Generic.List<ItemData>(playerInventory.Items);
        for (int i = itemsList.Count - 1; i >= 0; i--)
        {
            depositedValue += itemsList[i].value;
            playerInventory.RemoveItemAt(i);
        }

        totalLootValue += depositedValue;

        PlayerItemHolder itemHolder = playerInventory.GetComponent<PlayerItemHolder>();
        if (itemHolder != null)
        {
            itemHolder.EquipHandsFree();
        }

        Debug.Log($"[Extraction] Deposited {itemAmount} items for ${depositedValue}. Total: ${totalLootValue}");
        UpdateZonePrompt();
    }

    private void EscapeMission()
    {
        isPlayerInZone = false;

        UnityEngine.EventSystems.EventSystem currentEventSys = UnityEngine.EventSystems.EventSystem.current;
        if (currentEventSys != null)
        {
            currentEventSys.enabled = true;
        }

        PlayerController playerController = playerInventory != null ? playerInventory.GetComponent<PlayerController>() : FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetLockLook(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        HideAllGameplayUI();

        if (endMissionPanel != null)
        {
            endMissionPanel.SetActive(true);
        }

        if (summaryText != null)
        {
            summaryText.text = $"MISSION COMPLETED!\n\nTotal Loot : ${totalLootValue}";
        }

        Time.timeScale = 0f;
    }

    private void HideAllGameplayUI()
    {
        if (zonePromptText != null)
        {
            zonePromptText.gameObject.SetActive(false);
        }

        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas != null)
        {
            foreach (Transform child in mainCanvas.transform)
            {
                if (endMissionPanel != null && (child.gameObject == endMissionPanel || child.IsChildOf(endMissionPanel.transform)))
                {
                    continue;
                }

                if (child.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }
    }

}