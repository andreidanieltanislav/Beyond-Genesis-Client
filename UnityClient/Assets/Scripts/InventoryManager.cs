using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject slotHolder;
    public ItemClass itemToAdd;
    public ItemClass itemToRemove;

    public List<SlotsClass> items = new List<SlotsClass>();

    private GameObject[] slots;

    public void Start() //pickup
    {
        slots = new GameObject[slotHolder.transform.childCount];

        for(int i=0; i< slotHolder.transform.childCount; i++)
        {
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }

        RefreshUI();

        Add(itemToAdd);
        Remove(itemToRemove);
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                
                if(items[i].GetItem().isStackable)
                    slots[i].transform.GetChild(1).GetComponent<Text>().text = items[i].GetQuantity() + "";
                else
                    slots[i].transform.GetChild(1).GetComponent<Text>().text = "";
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<Text>().text = "";
            }
        }
    }

    public bool Add(ItemClass item)
    {
        //  items.Add(item);


        SlotsClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable)
            slot.AddQuantity(1);
        else
        {
            if (items.Count < slots.Length)
                items.Add(new SlotsClass(item, 1));
            else
                return false;
        }

        RefreshUI();
        return true;
    }

    public bool Remove(ItemClass item)
    {
        //  items.Remove(itemToRemove);
        SlotsClass te = Contains(item);
        if (te != null)
        {
            if(te.GetQuantity() > 1)
                te.SubQuantity(1);
            else
            {
                SlotsClass slotToRemove = new SlotsClass();

                foreach (SlotsClass slot in items)
                {
                    if (slot.GetItem() == item)
                    {
                        slotToRemove = slot;
                        break;
                    }
                }
                items.Remove(slotToRemove);
            }
        }
        else
        {
            return false;
        }

        RefreshUI();
        return true;
    }

    public SlotsClass Contains(ItemClass item)
    {
        foreach ( SlotsClass slot in items)
        {
            if (slot.GetItem() == item)
                return slot;
        }

        return null;
    }
}
