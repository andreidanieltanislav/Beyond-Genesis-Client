using UnityEngine;

namespace _DEV.Feature_Sample.Scripts {
    public class ItemPickup : Interactable {

        public Item item;

        public override void Interact() {
            base.Interact();
            Pickup();
        }

        private void Pickup() {
            Debug.Log("Picked up " + item.name);
            // TODO - Check if successfully added to inventory before destroy
            Destroy(gameObject);
        }
    }
}
