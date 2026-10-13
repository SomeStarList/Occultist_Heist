using UnityEngine;

public enum ItemType
{
    Loot,       // ของมีค่า/ขโมยมาขาย
    Tool,       // อุปกรณ์ เช่น ไฟฉาย, ไฟแช็ก
    Key         // กุญแจ/ไอเทมเควสต์
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public float weight;
    public int value;
    public ItemType itemType;
    public Sprite icon;
    public GameObject dropPrefab;
    public GameObject heldModelPrefab; // โมเดลที่จะแสดงในมือ

    public ItemData(string name, float weight, int value, ItemType type, Sprite icon = null, GameObject dropPrefab = null, GameObject heldModel = null)
    {
        this.itemName = name;
        this.weight = weight;
        this.value = value;
        this.itemType = type;
        this.icon = icon;
        this.dropPrefab = dropPrefab;
        this.heldModelPrefab = heldModel;
    }
}