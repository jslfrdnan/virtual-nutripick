using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene1ReplicationTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator LevelDefinitionAndPlatePositionCanBeSwapped()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var finalResult = Object.FindFirstObjectByType<FinalResultPresenter>(FindObjectsInactive.Include);
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.IsNotNull(finalResult, "FinalResultPresenter missing.");

            var original = flow.level;
            var temp = ScriptableObject.CreateInstance<LevelDefinition>();
            temp.levelId = "replication_test";
            temp.displayName = "Replication Test";
            temp.sceneName = "MainVR";
            temp.nextSceneName = "Level02_Warteg";
            temp.levelIndex = 1;
            temp.foodPool = original.foodPool.Reverse().ToArray();
            temp.minItems = 3;
            temp.requiresDrink = false;
            temp.minimumStands = 1;
            flow.RebindLevel(temp);

            plate.transform.position += new Vector3(0.05f, 0f, 0f);
            yield return null;
            yield return null;

            var grid = Object.FindFirstObjectByType<FoodCardGridPresenter>();
            Assert.IsNotNull(grid, "FoodCardGridPresenter missing after level swap.");
            Assert.AreEqual(8, grid.CardCount, "Replicated grid should still show 8 cards.");
            Assert.AreEqual(temp.foodPool[0].id, grid.Cards[0].FoodId, "Card order should follow the swapped level pool.");

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(foods[i]), "Could not place replicated food " + i);

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(finalResult.root.activeSelf, "Final Result should still work after replication.");

            Object.Destroy(temp);
        }
    }
}
