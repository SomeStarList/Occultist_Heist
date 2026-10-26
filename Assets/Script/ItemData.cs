using UnityEngine;

public enum ItemType
{
    Loot,
    Tool,
    Key
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
    public GameObject heldModelPrefab;

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