using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class InventoryUI : MonoBehaviour
{
    [Inject] private readonly Inventory _inventory;

    public Action<InventoryItemData> OnInventoryItemSelected;

    [SerializeField] private RectTransform _parent;
    [SerializeField] private InventoryUIItem _itemPrefab;

    private Dictionary<InventoryItemData, InventoryUIItem> _uiItems = new ();

    private void Start()
    {
        _inventory.OnItemsUpdated += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        var inventoryItems = _inventory.GetItems();

        var keysToRemove = _uiItems.Keys.Where(key => !inventoryItems.ContainsKey(key)).ToList();
        foreach (var key in keysToRemove)
        {
            Destroy(_uiItems[key].gameObject);
            _uiItems.Remove(key);
        }

        foreach (var pair in inventoryItems)
        {
            if (!_uiItems.ContainsKey(pair.Key))
            {
                var uiItem = Instantiate(_itemPrefab, _parent);
                var preview = Instantiate(pair.Key.Preview, uiItem.PreviewParent);
                uiItem.Init(preview, () => GetNumberOfItems(pair.Key), OnInventoryUIItemSelected);
                _uiItems.Add(pair.Key, uiItem);
            }
            
            _uiItems[pair.Key].Refresh();
        }
    }

    private void OnInventoryUIItemSelected(InventoryUIItem item)
    {
        OnInventoryItemSelected?.Invoke(_uiItems.FirstOrDefault(i => i.Value == item).Key);
    }

    private int GetNumberOfItems(InventoryItemData data)
    {
        return _inventory.GetItems()[data];
    }
}
