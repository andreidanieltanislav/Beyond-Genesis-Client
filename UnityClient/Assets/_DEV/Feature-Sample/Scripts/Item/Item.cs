using UnityEngine;

namespace _DEV.Feature_Sample.Scripts.Item {
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject {

        public new string name = "New Item";
        public Sprite icon = null;
        public GameObject modelPrefab;
        public bool isStackable = false;

        public virtual void Use() {
            Debug.Log("Using " + name);
        }

        public void RemoveFromInventory() {
            Instantiate(modelPrefab, PlayerManager.instance.transform);
        }
    }
}
