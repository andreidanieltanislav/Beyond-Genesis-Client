using System.Collections.Generic;
using UnityEngine;

namespace _DEV.Feature_Sample.Scripts {
    [CreateAssetMenu(fileName = "New Equipment Item", menuName = "Inventory/Equipment Item")]
    public class EquipmentItem : Item {

        public EquipmentSlot slot;
        public List<StatModifier> statModifiers;

        public override void Use() {
            base.Use();
            //Equip item
            //Remove From Inventory
        }
    }
}