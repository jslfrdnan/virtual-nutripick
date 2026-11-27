using TMPro;
using UnityEngine;

namespace NutriPick.UI
{
    public class InteractionPrompt : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text text;

        private Vector2 defaultRootSize;
        private Vector2 defaultTextSize;
        private float defaultFontSize;
        private bool defaultsCached;

        public void Show(string message)
        {
            if (message != null && message.IndexOf('\n') >= 0)
            {
                ShowHint(message);
                return;
            }

            CacheDefaults();

            if (root != null)
            {
                root.SetActive(true);
                var rect = root.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = defaultRootSize;
            }

            if (text != null)
            {
                text.fontSize = defaultFontSize;
                text.rectTransform.sizeDelta = defaultTextSize;
                text.text = message;
            }
        }

        /// <summary>
        /// Two-line hint used when a serve attempt is blocked. The prompt is
        /// temporarily enlarged and uses a smaller font so the suggestion fits.
        /// </summary>
        public void ShowHint(string message)
        {
            CacheDefaults();

            if (root != null)
            {
                root.SetActive(true);
                var rect = root.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = new Vector2(defaultRootSize.x, 112f);
            }

            if (text != null)
            {
                text.fontSize = 17f;
                text.rectTransform.sizeDelta = new Vector2(defaultTextSize.x, 108f);
                text.text = message;
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void CacheDefaults()
        {
            if (defaultsCached)
                return;

            defaultsCached = true;

            if (root != null)
            {
                var rect = root.GetComponent<RectTransform>();
                if (rect != null)
                    defaultRootSize = rect.sizeDelta;
            }

            if (text != null)
            {
                defaultFontSize = text.fontSize;
                defaultTextSize = text.rectTransform.sizeDelta;
            }
        }
    }
}
