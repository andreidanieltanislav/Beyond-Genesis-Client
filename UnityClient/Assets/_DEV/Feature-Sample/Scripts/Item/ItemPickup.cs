using UnityEngine;

namespace _DEV.Feature_Sample.Scripts.Item {
    public class ItemPickup : Interactable {
        public Item item;

        public override void Interact() {
            base.Interact();
            Pickup();
        }

        private void Pickup() {
            Debug.Log("Picked up " + item.name);
            if (InventoryManager.instance.Add(item))
                Destroy(gameObject);
        }
    }
}