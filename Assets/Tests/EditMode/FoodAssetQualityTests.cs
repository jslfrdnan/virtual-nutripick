using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class FoodAssetQualityTests
    {
        private const int PolyBudget = 8000;

        private static FoodData[] LoadFoods()
        {
            return AssetDatabase.FindAssets("t:FoodData", new[] { "Assets/ScriptableObjects/Food" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<FoodData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(food => food != null)
                .ToArray();
        }

        [Test]
        public void EveryFoodHasAnIcon()
        {
            foreach (var food in LoadFoods())
                Assert.IsNotNull(food.icon, $"{food.id} has no icon.");
        }

        [Test]
        public void EveryFoodPrefabStaysUnderPolyBudget()
        {
            foreach (var food in LoadFoods())
            {
                Assert.IsNotNull(food.prefab, $"{food.id} has no prefab.");
                int triangles = food.prefab.GetComponentsInChildren<MeshFilter>(true)
                    .Where(filter => filter.sharedMesh != null)
                    .Sum(filter => filter.sharedMesh.triangles.Length / 3);

                Assert.LessOrEqual(triangles, PolyBudget, $"{food.id} uses {triangles} triangles (budget {PolyBudget}).");
            }
        }

        [Test]
        public void EveryFoodUsesATexturedUnlitMaterial()
        {
            foreach (var food in LoadFoods())
            {
                var materials = food.prefab.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material != null)
                    .ToArray();

                Assert.IsNotEmpty(materials, $"{food.id} has no material.");
                foreach (var material in materials)
                {
                    Assert.AreEqual("Universal Render Pipeline/Unlit", material.shader.name,
                        $"{food.id}/{material.name} is not URP/Unlit.");
                    Assert.IsNotNull(material.GetTexture("_BaseMap"), $"{food.id}/{material.name} has no albedo texture.");
                }
            }
        }
    }
}
