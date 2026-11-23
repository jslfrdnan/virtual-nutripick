using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NutriPick.Flow;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class PauseMenuTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.ResetSession();
                GameSession.Instance.MarkIntroSeen();
                GameSession.Instance.sessionActive = true;
            }
        }

        [UnityTest]
        public IEnumerator PauseSelectsResumeAndHighlightsBothButtons()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.pausePanel, "Pause panel missing.");

            var pause = typeof(LevelFlowController).GetMethod("Pause", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(pause, "Pause method missing.");
            pause.Invoke(flow, new object[] { false, "Jeda" });
            yield return null;

            var resume = flow.pauseResumeButton;
            Button help = null;
            foreach (var button in flow.pausePanel.GetComponentsInChildren<Button>(true))
            {
                if (button.name == "HelpButton")
                    help = button;
            }
            var quit = flow.pauseQuitButton;
            Assert.IsNotNull(resume, "Resume button missing.");
            Assert.IsNotNull(help, "Help button missing.");
            Assert.IsNotNull(quit, "Quit button missing.");

            Assert.AreEqual(resume.gameObject, EventSystem.current.currentSelectedGameObject,
                "Pause should select Resume first.");
            Assert.AreEqual(help, resume.navigation.selectOnDown, "Resume down should reach Help.");
            Assert.AreEqual(quit, help.navigation.selectOnDown, "Help down should reach Quit.");
            Assert.AreEqual(help, quit.navigation.selectOnUp, "Quit up should return to Help.");

            foreach (var button in new[] { resume, help, quit })
            {
                Assert.IsNotNull(button.GetComponent<SelectableFocusHighlight>(),
                    $"{button.name} should have a focus highlight.");
                var colors = button.colors;
                Assert.Greater(colors.selectedColor.grayscale - colors.normalColor.grayscale, 0.15f,
                    $"{button.name} selected color must be brighter than normal.");
            }

            EventSystem.current.SetSelectedGameObject(quit.gameObject);
            yield return null;
            Assert.AreEqual(quit.gameObject, EventSystem.current.currentSelectedGameObject,
                "Quit should be selectable with the controller.");
        }
    }
}
