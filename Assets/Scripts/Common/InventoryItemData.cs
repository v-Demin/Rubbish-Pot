using UnityEngine;

[CreateAssetMenu(fileName = "new Inventory Item", menuName = "Inventory/new Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    [field: SerializeField] public PotItem PotItemPrefab { get; private set; }
    [field: SerializeField] public RectTransform Preview { get; private set; }
    [field: SerializeField] public float Price { get; private set; }
}
