using TMPro;
using UnityEngine;

namespace NutriPick.UI
{
    public class InteractionPrompt : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text text;

        public void Show(string message)
        {
            if (root != null)
                root.SetActive(true);
            if (text != null)
                text.text = message;
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
