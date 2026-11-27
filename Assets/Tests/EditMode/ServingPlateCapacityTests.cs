using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using NutriPick.Interaction;
using NutriPick.Quests;

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
        public void ServingPlateStillRequiresFiveItemsByDefault()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Interaction/ServingPlate.prefab");
            var plate = prefab.GetComponent<PlateController>();
            Assert.AreEqual(5, plate.minItems, "Minimum items should remain five by default.");
        }

        [Test]
        public void AnakQuestUsesFourItemMinimumAndSmallerPlate()
        {
            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(
                "Assets/ScriptableObjects/Quests/QD_anak.asset");
            Assert.IsNotNull(quest, "Anak quest missing.");
            Assert.AreEqual(4, quest.plateMinItems, "Anak quest should require four foods.");
            Assert.AreEqual(0.8f, quest.plateScale, 0.001f, "Anak plate should be 80% size.");
        }

        [Test]
        public void OtherQuestsKeepFiveItemPlateAndFullSize()
        {
            foreach (var path in new[]
            {
                "Assets/ScriptableObjects/Quests/QD_dewasa.asset",
                "Assets/ScriptableObjects/Quests/QD_lansia.asset"
            })
            {
                var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
                Assert.IsNotNull(quest, "Quest missing: " + path);
                Assert.AreEqual(0, quest.plateMinItems, path + " should use the level minimum.");
                Assert.AreEqual(1f, quest.plateScale, 0.001f, path + " should keep the full plate size.");
            }
        }
    }
}
