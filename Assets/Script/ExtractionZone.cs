using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ExtractionZone : MonoBehaviour
{
    [Header("Extraction Settings")]
    [Tooltip("เงิน/คะแนนสะสมรวมทั้งหมดที่ส่งของสำเร็จ")]
    [SerializeField] private int totalLootValue = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI zonePromptText; // UI แสดงปุ่มกด เช่น "[E] Deposit Loot | [F] Escape Mission"
    [SerializeField] private GameObject endMissionPanel;    // หน้าต่าง UI สรุปผลหลังจบภารกิจ
    [SerializeField] private TextMeshProUGUI summaryText;    // ข้อความสรุปเงินที่ได้ทั้งหมด

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

        // กด E เพื่อส่งของทั้งหมดที่มีในตัว
        if (Keyboard.current.eKey.wasPressedThisFrame && playerInventory.Items.Count > 0)
        {
            DepositAllLoot();
        }

        // กด F เพื่อจบภารกิจ
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            EscapeMission();
        }
    }

    private void DepositAllLoot()
    {
        int depositedValue = 0;
        int itemAmount = playerInventory.Items.Count;

        // ดึงรายการไอเทมทั้งหมดมาคำนวณมูลค่า
        var itemsList = new System.Collections.Generic.List<ItemData>(playerInventory.Items);
        for (int i = itemsList.Count - 1; i >= 0; i--)
        {
            depositedValue += itemsList[i].value;
            playerInventory.RemoveItemAt(i);
        }

        totalLootValue += depositedValue;

        // ให้ PlayerItemHolder ถือมือเปล่าหลังจากล้างกระเป๋า
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
        // 0. หยุดการทำงานของ Trigger ภายในสคริปต์นี้เพื่อไม่ให้ Update() ทำงานต่อ
        isPlayerInZone = false;

        // 1. ล็อกการหันหน้าและปลดล็อกเคอร์เซอร์เมาส์
        PlayerController playerController = playerInventory != null ? playerInventory.GetComponent<PlayerController>() : FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetLockLook(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. หยุดเวลาเกม (Pause)
        Time.timeScale = 0f;

        // 3. ปิด UI บนหน้าจอทั้งหมด
        HideAllGameplayUI();

        // 4. แสดงเฉพาะ UI หน้าสรุปผลภารกิจ
        if (endMissionPanel != null)
        {
            endMissionPanel.SetActive(true);
        }

        if (summaryText != null)
        {
            summaryText.text = $"MISSION COMPLETED!\n\nTotal Loot : ${totalLootValue}";
        }
    }

    private void HideAllGameplayUI()
    {
        // สั่งปิดข้อความ ExtractionPromptText โดยตรงเป็นอันดับแรก
        if (zonePromptText != null)
        {
            zonePromptText.gameObject.SetActive(false);
        }

        // ซ่อน Child ทุกตัวใต้ Canvas (ยกเว้น endMissionPanel)
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas != null)
        {
            foreach (Transform child in mainCanvas.transform)
            {
                if (endMissionPanel != null && (child.gameObject == endMissionPanel || child.IsChildOf(endMissionPanel.transform)))
                {
                    continue; // ข้าม Panel สรุปผล
                }
                child.gameObject.SetActive(false);
            }
        }
    }

    // ฟังก์ชันวาดรูปสี่เหลี่ยมสีเหลืองในหน้า Gizmos (Editor View)
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.4f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}