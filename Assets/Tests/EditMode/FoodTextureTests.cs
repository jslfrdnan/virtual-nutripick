using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NutriPick.Tests
{
    public class FoodTextureTests
    {
        [Test]
        public void FoodMaterialsUseReferenceAlbedoTextures()
        {
            var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials/Home" });
            int foodMaterials = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || !material.name.StartsWith("M_Food_"))
                    continue;

                foodMaterials++;
                Assert.AreEqual("Universal Render Pipeline/Unlit", material.shader.name, material.name);
                Assert.IsNotNull(material.GetTexture("_BaseMap"), $"{material.name} has no albedo texture.");
            }

            Assert.AreEqual(23, foodMaterials, "Expected 23 textured food materials.");
        }
    }
}
