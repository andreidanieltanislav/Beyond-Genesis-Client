using UnityEngine;

namespace _DEV.Feature_Sample.Scripts {
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject {

        public new string name = "New Item";
        public Sprite icon = null;
        public GameObject modelPrefab;

        public virtual void Use() {
            Debug.Log("Using " + name);
        }

        public void RemoveFromInventory() {
            // TODO - Add Inventory removal method
            // TODO - Instantiate item
        }
    }
}
