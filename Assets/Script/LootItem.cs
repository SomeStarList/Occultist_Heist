using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Header("Item Configuration")]
    [SerializeField] private string itemName;
    [SerializeField] private float weight = 2.0f;
    [SerializeField] private int value = 150;
    [SerializeField] private ItemType itemType = ItemType.Loot;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject dropPrefab;
    [Tooltip("โมเดลที่จะแสดงที่มือผู้เล่นเมื่อเลือกไอเทมนี้")]
    [SerializeField] private GameObject heldModelPrefab;

    public string ItemName => itemName;
    public float Weight => weight;
    public int Value => value;
    public ItemType Type => itemType;

    public ItemData ToItemData()
    {
        return new ItemData(itemName, weight, value, itemType, icon, dropPrefab != null ? dropPrefab : gameObject, heldModelPrefab);
    }

    public void OnCollected()
    {
        Destroy(gameObject);
    }

    public void Initialize(ItemData data)
    {
        if (data == null) return;

        itemName = data.itemName;
        weight = data.weight;
        value = data.value;
        itemType = data.itemType;
        icon = data.icon;
        heldModelPrefab = data.heldModelPrefab;
        dropPrefab = data.dropPrefab;
    }

}