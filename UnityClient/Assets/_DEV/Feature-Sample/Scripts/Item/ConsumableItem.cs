using System.Collections.Generic;
using UnityEngine;

namespace _DEV.Feature_Sample.Scripts.Item {
    [CreateAssetMenu(fileName = "New Consumable Item", menuName = "Inventory/Consumable Item")]
    public class ConsumableItem : Item {

        public List<StatModifier> statModifiers;

        public override void Use() {
            base.Use();
            // Apply stat modifiers to player
        }
    }
}
