using System;
using System.Collections.Generic;
using System.Linq;
using _DEV.Feature_Sample.Scripts.Item;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    #region Singleton

    public static InventoryManager instance;

    private void Awake() {
        if (instance == null)
            instance = this;
    }

    #endregion

    [SerializeField] private GameObject slotHolder;
    public GameObject inventory;
    private List<InventorySlot> slots = new List<InventorySlot>();

    public void Start() //pickup
    {
        for (int i = 0; i < slotHolder.transform.childCount; i++) {
            slots.Add(slotHolder.transform.GetChild(i).GetComponent<InventorySlot>());
        }
    }

    private void Update() {
        CheckInput();
    }

    public bool Add(Item item) {
        //  items.Add(item);
        InventorySlot slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable)
            slot.AddQuantity(1);
        else {
            var emptySlot = slots.Find(inventorySlot => inventorySlot.GetItem() == null);
            if (emptySlot != null)
                emptySlot.SetItem(item);
            else
                return false;
        }

        return true;
    }

    public bool Remove(Item item) {
        //  items.Remove(itemToRemove);
        InventorySlot te = Contains(item);
        if (te != null) {
            if (te.GetQuantity() > 1)
                te.SubQuantity(1);
            else {
                InventorySlot slotToRemove = Contains(item);
                if (slotToRemove) {
                    slotToRemove.RemoveItem();
                }
            }
        } else {
            return false;
        }

        return true;
    }

    public InventorySlot Contains(Item item) {
        return slots.FirstOrDefault(slot => slot.GetItem() == item);
    }

    private void CheckInput() {
        if (Input.GetKeyDown(KeyCode.I)) {
            inventory.SetActive(!inventory.activeSelf);
        }
    }
}