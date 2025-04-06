using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class Inventory : IInitializable
{
    public Action OnItemsUpdated;
    
    private Dictionary<InventoryItemData, int> _items = new ();
    
    public void Initialize()
    {
        //[Todo]: заменить на работу с сохранениями;
        var datas = Resources.LoadAll<InventoryItemData>("Inventory/Items/");
        
        foreach (var data in datas)
        {
            var numberOfItems = Random.Range(1, 5);
            for (var i = 0; i < numberOfItems; i++)
            {
                AddItemInner(data);
            }
        }
        
        OnItemsUpdated?.Invoke();
    }

    public Dictionary<InventoryItemData, int> GetItems()
    {
        return _items;
    }

    public void AddItem(InventoryItemData data)
    {
        AddItemInner(data);
        OnItemsUpdated?.Invoke();
    }

    private void AddItemInner(InventoryItemData data)
    {
        if (_items.ContainsKey(data))
        {
            _items[data]++;
        }
        else
        {
            _items.Add(data, 1);
        }
    }

    public void RemoveItem(InventoryItemData data)
    {
        RemoveItemInner(data);
        OnItemsUpdated?.Invoke();
    }
    
    public void RemoveItemInner(InventoryItemData data)
    {
        if (!_items.ContainsKey(data)) return;

        _items[data]--;

        if (_items[data] <= 0)
        {
            _items.Remove(data);
        }
    }
}
