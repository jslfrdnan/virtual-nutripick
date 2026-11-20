using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Content;
using NutriPick.Interaction;

namespace NutriPick.Tests
{
    public class Scene1TableLayoutTests
    {
        [UnityTest]
        public IEnumerator ServeTableHasOnlyFoodPlatesAndServeObjects()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var activeTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var transform in activeTransforms)
            {
                if (transform.name == "TableVase"
                    || transform.name.StartsWith("TablePlant")
                    || transform.name.StartsWith("Tray_")
                    || transform.name == "LaukPlatter"
                    || transform.name == "KarboSayurPlatter")
                {
                    Assert.Fail($"Non-food object is still active on the serve table: {transform.name}");
                }
            }

            var plates = Object.FindObjectsByType<FoodStand>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Assert.AreEqual(8, plates.Length, "The serve table should have eight food plates.");
            Assert.IsTrue(plates.All(plate => plate.name.StartsWith("FoodPlate_")), "Every station should be a food plate.");
            Assert.IsTrue(plates.All(plate => plate.foods.Length == 1), "Each plate should hold exactly one food type.");

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Length, "The serve table should have eight foods.");
            foreach (var food in foods)
            {
                var p = food.transform.position;
                Assert.That(p.x, Is.InRange(-1.8f, -0.2f), $"{food.name} is outside the table on x.");
                Assert.That(p.z, Is.InRange(-1.55f, -0.65f), $"{food.name} is outside the table on z.");
                Assert.That(p.y, Is.InRange(0.75f, 0.9f), $"{food.name} is not resting on its plate.");
            }

            var servingPlate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(servingPlate, "Serving plate missing.");
            Assert.That(servingPlate.transform.position.x, Is.InRange(-1.8f, -0.2f));
            Assert.That(servingPlate.transform.position.z, Is.InRange(-1.55f, -0.65f));

            var station = Object.FindFirstObjectByType<ServeStation>();
            Assert.IsNotNull(station, "Serve station missing.");
            Assert.That(station.transform.position.x, Is.InRange(-1.8f, -0.2f));
            Assert.That(station.transform.position.z, Is.InRange(-1.55f, -0.65f));
        }
    }
}
