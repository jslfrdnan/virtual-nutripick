using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NutriPick.UI
{
    public class StartGuidePresenter : MonoBehaviour
    {
        public GameObject root;
        public Button startButton;
        public TMP_Text controlsText;
        public TMP_Text rulesText;
        public TMP_Text questText;

        private Action onClosed;

        public bool IsOpen => root != null && root.activeSelf;

        public void SetQuestSummary(string text)
        {
            if (questText != null)
                questText.text = text;
        }

        public void Show(Action onClose)
        {
            if (root == null)
                root = gameObject;

            onClosed = onClose;
            root.SetActive(true);

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(Close);
                startButton.onClick.AddListener(Close);
            }

            if (EventSystem.current != null && startButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Close()
        {
            Hide();
            var callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }
    }
}
