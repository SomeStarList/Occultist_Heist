using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerItemHolder : MonoBehaviour
{
    [Header("Hold Settings")]
    [Tooltip("จุดว่างใต้กล้องที่ไว้สปอว์นโมเดลไอเทมในมือ")]
    [SerializeField] private Transform itemHoldPoint;

    [Header("Drop / Throw Settings")]
    [Tooltip("กล้องมุมมองบุคคลที่หนึ่ง")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("แรงโยนไปข้างหน้า")]
    [SerializeField] private float throwForce = 4.0f;
    [Tooltip("แรงโยนเสยขึ้นเล็กน้อยให้ดูเป็นธรรมชาติ")]
    [SerializeField] private float throwUpwardForce = 1.5f;

    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI equippedItemText;

    private PlayerInventory inventory;
    private int selectedIndex = -1;
    private GameObject currentSpawnedItem;

    public int SelectedIndex => selectedIndex;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        if (inventory.Items.Count > 0)
        {
            SelectItem(0);
        }
        else
        {
            UpdateEquippedUI(null);
        }
    }

    private void Update()
    {
        HandleNumberKeys();
        HandleScrollWheel();
        HandleDropInput();
    }

    private void HandleDropInput()
    {
        // ตรวจสอบการกดปุ่ม G เพื่อโยนของที่ถืออยู่
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropCurrentItem();
        }
    }

    public void DropCurrentItem()
    {
        if (selectedIndex < 0 || selectedIndex >= inventory.Items.Count) return;

        ItemData droppedItemData = inventory.RemoveItemAt(selectedIndex);
        if (droppedItemData == null) return;

        if (currentSpawnedItem != null)
        {
            Destroy(currentSpawnedItem);
            currentSpawnedItem = null;
        }

        // เสกไอเทมหน้ากล้อง
        Vector3 spawnPosition = cameraTransform != null
            ? cameraTransform.position + (cameraTransform.forward * 0.8f)
            : transform.position + (transform.forward * 0.8f) + Vector3.up * 1.0f;

        Quaternion spawnRotation = cameraTransform != null ? cameraTransform.rotation : transform.rotation;

        GameObject prefabToSpawn = droppedItemData.dropPrefab != null ? droppedItemData.dropPrefab : droppedItemData.heldModelPrefab;

        if (prefabToSpawn != null)
        {
            GameObject droppedObj = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);

            // 1. ให้แน่ใจว่าเปิดใช้งาน GameObject
            droppedObj.SetActive(true);

            // 2. จัดการ Collider ให้พร้อมเก็บ
            Collider[] existingCols = droppedObj.GetComponentsInChildren<Collider>(true);
            if (existingCols.Length == 0)
            {
                // ถ้าไม่มี Collider เลย ให้สร้าง BoxCollider ขึ้นมาใหม่
                BoxCollider boxCol = droppedObj.AddComponent<BoxCollider>();
                boxCol.enabled = true;
                boxCol.isTrigger = false;
            }
            else
            {
                // เปิด Collider ทั้งหมดที่มีในโมเดล
                foreach (var c in existingCols)
                {
                    c.enabled = true;
                    c.isTrigger = false;
                }
            }

            // 3. จัดการ Rigidbody
            Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = droppedObj.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            rb.useGravity = true;

            // 4. ใส่แรงโยน
            Vector3 throwDirection = cameraTransform != null ? cameraTransform.forward : transform.forward;
            rb.linearVelocity = (throwDirection * throwForce) + (Vector3.up * throwUpwardForce);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            // 5. ติดตั้ง LootItem และเซ็ตข้อมูลเดิม
            LootItem loot = droppedObj.GetComponent<LootItem>();
            if (loot == null)
            {
                loot = droppedObj.AddComponent<LootItem>();
            }
            loot.Initialize(droppedItemData);

            // 6. ตั้ง Layer ให้เป็น Default (หรือตรงกับ interactableLayer)
            droppedObj.layer = LayerMask.NameToLayer("Default");
            foreach (Transform child in droppedObj.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }

        // จัดการของในมือต่อ
        if (inventory.Items.Count > 0)
        {
            int nextIndex = Mathf.Clamp(selectedIndex, 0, inventory.Items.Count - 1);
            SelectItem(nextIndex);
        }
        else
        {
            EquipHandsFree();
        }
    }

    private void HandleNumberKeys()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectItem(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectItem(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectItem(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectItem(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectItem(4);
    }

    private void HandleScrollWheel()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.1f) return;

        int totalCount = inventory.Items.Count;
        if (totalCount == 0) return;

        if (scroll > 0f)
        {
            int nextIndex = selectedIndex + 1;
            if (nextIndex >= totalCount) nextIndex = 0;
            SelectItem(nextIndex);
        }
        else if (scroll < 0f)
        {
            int prevIndex = selectedIndex - 1;
            if (prevIndex < 0) prevIndex = totalCount - 1;
            SelectItem(prevIndex);
        }
    }

    public void SelectItem(int index)
    {
        IReadOnlyList<ItemData> items = inventory.Items;

        if (index < 0 || index >= items.Count)
        {
            EquipHandsFree();
            return;
        }

        selectedIndex = index;
        ItemData selectedItem = items[selectedIndex];

        // 1. ทำลายโมเดลเก่าทิ้ง
        if (currentSpawnedItem != null)
        {
            Destroy(currentSpawnedItem);
            currentSpawnedItem = null;
        }

        // 2. ตรวจสอบว่ามี Prefab สำหรับถือหรือไม่
        GameObject modelToSpawn = selectedItem.heldModelPrefab != null ? selectedItem.heldModelPrefab : selectedItem.dropPrefab;

        if (modelToSpawn != null && itemHoldPoint != null)
        {
            currentSpawnedItem = Instantiate(modelToSpawn, itemHoldPoint);

            // รีเซ็ตตำแหน่ง การหมุน และขนาด ให้ตรงกับจุดถือเสมอ
            currentSpawnedItem.transform.localPosition = Vector3.zero;
            currentSpawnedItem.transform.localRotation = Quaternion.identity;

            // ถ้าโมเดลเดิมมีขนาดแปลกๆ ให้คืนค่าเป็นขนาดปกติของ Prefab หรือ Vector3.one
            currentSpawnedItem.transform.localScale = modelToSpawn.transform.localScale;

            // ปิด Collider ของโมเดลที่ถือในมือ เพื่อไม่ให้ชนตัวผู้เล่นหรือดันกล้องหลุด
            Collider[] colliders = currentSpawnedItem.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // ถ้ามี Rigidbody ให้ปิดฟิสิกส์ ไม่ให้ตกหล่น
            Rigidbody rb = currentSpawnedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
        else
        {
            Debug.LogWarning($"[ItemHolder] ไอเทม '{selectedItem.itemName}' ไม่มี heldModelPrefab กำหนดไว้!");
        }

        UpdateEquippedUI(selectedItem);
    }

    public void EquipHandsFree()
    {
        selectedIndex = -1;
        if (currentSpawnedItem != null)
        {
            Destroy(currentSpawnedItem);
        }
        UpdateEquippedUI(null);
    }

    private void UpdateEquippedUI(ItemData item)
    {
        if (equippedItemText != null)
        {
            if (item != null)
            {
                equippedItemText.text = $"[{selectedIndex + 1}] {item.itemName}";
            }
            else
            {
                equippedItemText.text = "[Hands Free]";
            }
        }
    }

    public void OnInventoryUpdated()
    {
        if (selectedIndex == -1 && inventory.Items.Count > 0)
        {
            SelectItem(0);
        }
        else if (selectedIndex >= inventory.Items.Count)
        {
            SelectItem(inventory.Items.Count - 1);
        }
    }
}