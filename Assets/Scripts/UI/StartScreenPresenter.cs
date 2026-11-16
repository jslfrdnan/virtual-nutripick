using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutriPick.UI
{
    public class StartScreenPresenter : MonoBehaviour
    {
        public GameObject root;
        public Button startButton;
        public Button profileButton;
        public GameObject profilePanel;
        public Button profileCloseButton;

        private Action onStart;
        private Action onProfile;

        private void Awake()
        {
            if (root == null)
                root = gameObject;
        }

        public void Show(Action startAction, Action profileAction)
        {
            onStart = startAction;
            onProfile = profileAction;

            if (root != null)
                root.SetActive(true);
            if (profilePanel != null)
                profilePanel.SetActive(false);

            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(HandleStart);
            }
            if (profileButton != null)
            {
                profileButton.onClick.RemoveAllListeners();
                profileButton.onClick.AddListener(HandleProfile);
            }
            if (profileCloseButton != null)
            {
                profileCloseButton.onClick.RemoveAllListeners();
                profileCloseButton.onClick.AddListener(CloseProfile);
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void HandleStart()
        {
            onStart?.Invoke();
        }

        private void HandleProfile()
        {
            if (profilePanel != null)
                profilePanel.SetActive(true);
            else
                onProfile?.Invoke();
        }

        private void CloseProfile()
        {
            if (profilePanel != null)
                profilePanel.SetActive(false);
        }
    }
}
