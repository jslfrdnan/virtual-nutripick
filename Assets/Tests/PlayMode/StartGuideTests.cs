using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Flow;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class StartGuideTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator MainVRShowsGuideOnceAndStartsAfterConfirm()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.startGuide, "Start guide missing.");
            Assert.IsTrue(flow.startGuide.IsOpen, "MainVR should open the start guide.");
            Assert.IsFalse(flow.IsLevelStarted, "The level should wait for the guide.");
            Assert.IsNotNull(flow.startGuide.startButton, "Start button missing.");
            Assert.AreEqual(flow.startGuide.startButton.gameObject, EventSystem.current.currentSelectedGameObject,
                "The start button should receive focus.");

            flow.startGuide.startButton.onClick.Invoke();
            yield return null;

            Assert.IsFalse(flow.startGuide.IsOpen, "The guide should close after MULAI.");
            Assert.IsFalse(flow.IsLevelStarted, "The level should wait for a quest choice.");
            Assert.IsTrue(flow.questSelect.IsOpen, "Quest select should open after the guide.");
            Assert.IsTrue(GameSession.Instance.HasSeenIntro, "The intro should be marked as seen.");

            flow.questSelect.questButtons[0].onClick.Invoke();
            yield return null;

            Assert.IsTrue(flow.IsLevelStarted, "Gameplay should start after choosing a quest.");
            Assert.IsTrue(GameSession.Instance.HasQuest, "The chosen quest should be stored.");

            SceneManager.LoadScene("Level02_Warteg");
            yield return null;
            yield return null;
            var level2 = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(level2, "Level 2 flow missing.");
            Assert.IsFalse(level2.startGuide.IsOpen, "Level 2 must not show the Scene 1 guide.");
        }
    }
}
