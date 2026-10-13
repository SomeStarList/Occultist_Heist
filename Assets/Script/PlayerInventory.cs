using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Capacity Settings")]
    [SerializeField] private int maxSlots = 5;       // ปรับจำนวนช่องได้ใน Inspector
    [SerializeField] private float maxWeight = 10f;   // น้ำหนักสูงสุด 10KG
    [SerializeField] private float currentWeight = 0f;

    [Header("Starting Equipment")]
    [Tooltip("ไอเทมที่พกมาตั้งแต่เริ่ม เช่น ไฟฉาย")]
    [SerializeField] private List<LootItem> startingEquipments = new List<LootItem>();

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI slotText; // UI แสดงจำนวนช่อง เช่น "Slots: 2/5"
    [SerializeField] private NotificationUI notificationUI;

    // รายการของในตัว
    private List<ItemData> items = new List<ItemData>();

    public float CurrentWeight => currentWeight;
    public float MaxWeight => maxWeight;
    public int CurrentSlotCount => items.Count;
    public int MaxSlots => maxSlots;
    public IReadOnlyList<ItemData> Items => items;

    private PlayerItemHolder itemHolder;

    private void Awake()
    {
        itemHolder = GetComponent<PlayerItemHolder>();
    }

    private void Start()
    {
        // โหลดไอเทมเริ่มต้นที่ซื้อติดตัวมา
        foreach (var equip in startingEquipments)
        {
            if (equip != null)
            {
                TryAddItem(equip.ToItemData());
            }
        }

        UpdateUI();
    }

    // ตรวจสอบเงื่อนไขว่าเก็บได้ไหม
    public bool CanCarry(float additionalWeight, out string reason)
    {
        if (items.Count >= maxSlots)
        {
            reason = "Inventory Full!";
            return false;
        }

        if ((currentWeight + additionalWeight) > maxWeight)
        {
            reason = "Too Heavy!";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    // เก็บผ่าน LootItem ในฉาก
    public bool TryCollectLoot(LootItem loot)
    {
        if (loot == null) return false;

        ItemData data = loot.ToItemData();
        if (TryAddItem(data))
        {
            loot.OnCollected();
            return true;
        }

        return false;
    }

    // เพิ่มข้อมูลไอเทมเข้ากระเป๋า
    public bool TryAddItem(ItemData newItem)
    {
        if (newItem == null) return false;

        if (CanCarry(newItem.weight, out string failReason))
        {
            items.Add(newItem);

                if (itemHolder != null)
                {
                    itemHolder.OnInventoryUpdated();
                }

            currentWeight += newItem.weight;
            currentWeight = Mathf.Round(currentWeight * 100f) / 100f;

            UpdateUI();

            // แจ้งเตือน Popup
            if (notificationUI != null)
            {
                string itemWeightStr = (newItem.weight % 1 == 0) ? newItem.weight.ToString("F0") : newItem.weight.ToString("F1");
                string currentStr = (currentWeight % 1 == 0) ? currentWeight.ToString("F0") : currentWeight.ToString("F1");
                string maxStr = (maxWeight % 1 == 0) ? maxWeight.ToString("F0") : maxWeight.ToString("F1");

                string alertMessage = $"Pick up {newItem.itemName} +{itemWeightStr}KG ({currentStr}/{maxStr}KG) [{items.Count}/{maxSlots}]";
                notificationUI.ShowNotification(alertMessage);
            }

            return true;
        }

        // กรณีเก็บไม่ได้ แล้วมี Notification เตือน
        if (notificationUI != null)
        {
            notificationUI.ShowNotification(failReason);
        }

        return false;
    }

    public void UpdateUI()
    {
        if (weightText != null)
        {
            string currentStr = (currentWeight % 1 == 0) ? currentWeight.ToString("F0") : currentWeight.ToString("F1");
            string maxStr = (maxWeight % 1 == 0) ? maxWeight.ToString("F0") : maxWeight.ToString("F1");
            weightText.text = $"{currentStr}/{maxStr} KG";
        }

        if (slotText != null)
        {
            slotText.text = $"Slots: {items.Count}/{maxSlots}";
        }
    }

    // ลบไอเทมตามตำแหน่ง index ออกจากกระเป๋าและคืนข้อมูลไอเทมนั้นออกมา พร้อมแจ้งเตือน Popup
    public ItemData RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return null;

        ItemData removedItem = items[index];
        items.RemoveAt(index);

        currentWeight -= removedItem.weight;
        currentWeight = Mathf.Max(0f, Mathf.Round(currentWeight * 100f) / 100f);

        UpdateUI();

        // แจ้งเตือน Popup เมื่อโยน/ทิ้งของ
        if (notificationUI != null)
        {
            string itemWeightStr = (removedItem.weight % 1 == 0) ? removedItem.weight.ToString("F0") : removedItem.weight.ToString("F1");
            string currentStr = (currentWeight % 1 == 0) ? currentWeight.ToString("F0") : currentWeight.ToString("F1");
            string maxStr = (maxWeight % 1 == 0) ? maxWeight.ToString("F0") : maxWeight.ToString("F1");

            // รูปแบบ: Dropped Box -2.5KG (0/10KG) [0/5]
            string alertMessage = $"Dropped {removedItem.itemName} -{itemWeightStr}KG ({currentStr}/{maxStr}KG) [{items.Count}/{maxSlots}]";
            notificationUI.ShowNotification(alertMessage);
        }

        return removedItem;
    }


}