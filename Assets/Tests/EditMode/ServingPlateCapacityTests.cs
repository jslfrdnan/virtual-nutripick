using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using NutriPick.Interaction;

namespace NutriPick.Tests
{
    public class ServingPlateCapacityTests
    {
        [Test]
        public void ServingPlateHasSixWiredSlots()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Interaction/ServingPlate.prefab");
            Assert.IsNotNull(prefab, "ServingPlate prefab missing.");

            var plate = prefab.GetComponent<PlateController>();
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.AreEqual(6, plate.slots.Length, "Serving plate should have six slots.");

            for (int i = 0; i < plate.slots.Length; i++)
            {
                var slot = plate.slots[i];
                Assert.IsNotNull(slot, "Slot " + i + " is missing.");
                Assert.IsNotNull(slot.snapPoint, "Slot " + i + " has no snap point.");
                Assert.IsNotNull(slot.trigger, "Slot " + i + " has no trigger.");
                Assert.IsNotNull(slot.highlight, "Slot " + i + " has no highlight.");
                Assert.AreEqual(plate, slot.plate, "Slot " + i + " is not wired to the plate.");
            }
        }

        [Test]
        public void ServingPlateStillRequiresFiveItems()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Interaction/ServingPlate.prefab");
            var plate = prefab.GetComponent<PlateController>();
            Assert.AreEqual(5, plate.minItems, "Minimum items should remain five.");
        }
    }
}
