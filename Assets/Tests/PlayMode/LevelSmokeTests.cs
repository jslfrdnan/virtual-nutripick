using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;

namespace NutriPick.Tests
{
    public class LevelSmokeTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator Level01SpawnsFoodAndWiresHud()
        {
            SceneManager.LoadScene("Level01_Warteg");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(1, stands.Length, "Level 1 should have one stand.");
            Assert.AreEqual(8, stands[0].Spawned.Count, "Level 1 should spawn 8 foods.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.AreEqual(4, plate.slots.Length, "Plate should have 4 slots.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.bar, "NutritionBar missing.");
            Assert.IsNotNull(flow.timer, "Timer missing.");
            Assert.IsNotNull(flow.infoCard, "Info card missing.");
            Assert.IsNotNull(flow.report, "Report missing.");
            Assert.IsNotNull(flow.player, "Player missing.");
            Assert.IsNotNull(flow.reticle, "Reticle missing.");
            Assert.AreEqual(1, GameSession.Instance.currentLevelIndex);
        }

        [UnityTest]
        public IEnumerator Level01CanPlateAndServe()
        {
            SceneManager.LoadScene("Level01_Warteg");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(stands, "No stands.");
            Assert.IsNotNull(plate, "No plate.");
            Assert.IsNotNull(flow, "No flow.");

            var spawned = stands[0].Spawned;
            Assert.GreaterOrEqual(spawned.Count, 3, "Not enough food spawned.");
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(spawned[i]), "Could not place food " + i);

            yield return null;
            Assert.AreEqual(3, plate.Count, "Plate should contain 3 items.");
            flow.TryServe();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(flow.IsReportOpen, "Report did not open.");
            Assert.IsNotNull(GameSession.Instance.GetResult(1), "Level 1 result was not recorded.");
            Assert.IsTrue(flow.report.root.activeSelf, "Report panel is not active.");
        }

        [UnityTest]
        public IEnumerator CapturePlayModeCamera()
        {
            SceneManager.LoadScene("Level01_Warteg");
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.5f);

            var camera = Camera.main;
            Assert.IsNotNull(camera, "No main camera.");
            var rt = new RenderTexture(1280, 720, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;

            var directory = Path.Combine(Path.GetTempPath(), "opencode");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "playmode-camera-Level01.png");
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.Destroy(texture);
            rt.Release();
            Object.Destroy(rt);
            Assert.IsTrue(File.Exists(path), "Camera preview was not written.");
        }

        [UnityTest]
        public IEnumerator Level02SpawnsSevenFoods()
        {
            SceneManager.LoadScene("Level02_Padang");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(1, stands.Length, "Level 2 should have one stand.");
            Assert.AreEqual(7, stands[0].Spawned.Count, "Level 2 should spawn 7 foods.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(2, flow.level.levelIndex);
        }

        [UnityTest]
        public IEnumerator Level03HasFourStandsAndDrinkRequirement()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(4, stands.Length, "Level 3 should have four stands.");

            int spawned = 0;
            foreach (var stand in stands)
                spawned += stand.Spawned.Count;
            Assert.AreEqual(8, spawned, "Level 3 should spawn 8 foods across the stands.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(3, flow.level.levelIndex);
            Assert.IsTrue(flow.level.requiresDrink);
            Assert.IsNotNull(flow.plate, "Plate missing.");
            Assert.AreEqual(4, flow.plate.slots.Length);
        }
    }
}
