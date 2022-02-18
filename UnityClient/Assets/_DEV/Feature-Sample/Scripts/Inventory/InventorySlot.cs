using _DEV.Feature_Sample.Scripts.Item;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour {
    [SerializeField] private Item item;
    [SerializeField] private int quantity = 0;

    public Image image;
    public Text quantityText;

    private void Update() {
        if (item != null) {
            image.sprite = item.icon;
            if (quantity > 0 && item.isStackable) {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            } else
                quantityText.enabled = false;
        }
    }

    public void SetItem(Item newItem) {
        item = newItem;
    }
    
    public Item GetItem() {
        return item;
    }

    public void RemoveItem() {
        item.RemoveFromInventory(); 
        item = null;
    }

    public int GetQuantity() {
        return quantity;
    }

    public void AddQuantity(int _quantity) {
        quantity += _quantity;
    }

    public void SubQuantity(int _quantity) {
        quantity -= _quantity;
    }
}