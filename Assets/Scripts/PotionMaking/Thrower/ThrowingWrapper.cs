using UnityEngine;
using Zenject;

public class ThrowingWrapper : MonoBehaviour
{
    [Inject] private readonly DiContainer _container;
    [Inject] private readonly Inventory _inventory;
    
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private ThrowingMechanic _throwingMechanic;

    private InventoryItemData _selectedItem;
    
    private void Start()
    {
        _inventoryUI.OnInventoryItemSelected += OnInventoryItemSelected;
        _throwingMechanic.Init(GetNewInventoryItem, CheckDraggingCondition, OnObjectDropped);
    }

    private void OnInventoryItemSelected(InventoryItemData data)
    {
        _selectedItem = data;
    }

    private ThrowingObject GetNewInventoryItem()
    {
        return _container.InstantiatePrefabForComponent<ThrowingObject>(_selectedItem.PotItemPrefab);
    }

    private bool CheckDraggingCondition()
    {
        return _selectedItem != null;
    }

    private void OnObjectDropped()
    {
        _inventory.RemoveItem(_selectedItem);
        _selectedItem = null;
    }
}
