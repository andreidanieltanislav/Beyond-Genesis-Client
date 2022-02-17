using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotsClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity;

    public SlotsClass()
    {
        item = null;
        quantity = 0;
    }

    public SlotsClass (ItemClass _item, int _quantity)
    {
        item = _item;
        quantity = _quantity;
    }

    public ItemClass GetItem() { return item; }
    public int GetQuantity() { return quantity; }
    public void AddQuantity(int _quantity)
    {
        quantity += _quantity;
    }
    public void SubQuantity(int _quantity)
    {
        quantity -= _quantity;
    }
}
