using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NutriPick.UI
{
    public class FriedWarningPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text bodyText;
        public TMP_Text listText;
        public Button retryButton;

        private Action onRetry;

        public bool IsOpen => root != null && root.activeSelf;

        public void Show(IReadOnlyList<string> friedNames, Action retry)
        {
            if (root == null)
                root = gameObject;

            onRetry = retry;
            root.SetActive(true);

            if (bodyText != null)
                bodyText.text = "COBALAH MEMILIH MAKANAN SEHAT";
            if (listText != null)
            {
                listText.text = friedNames == null || friedNames.Count == 0
                    ? "Piringmu masih mengandung makanan gorengan."
                    : "Makanan gorengan di piringmu: " + string.Join(", ", friedNames) + ".\nPilih versi sehat dan ulangi level.";
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(Retry);
            }

            if (EventSystem.current != null && retryButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Retry()
        {
            Hide();
            var callback = onRetry;
            onRetry = null;
            callback?.Invoke();
        }
    }
}
