using TMPro;
using UnityEngine;
using NutriPick.Utilities;

namespace NutriPick.UI
{
    public class LevelTimerDisplay : MonoBehaviour
    {
        public TMP_Text text;
        public Color normalColor = Color.white;
        public Color warningColor = new Color(0.78f, 0.16f, 0.16f);

        public void SetTime(float seconds)
        {
            if (text == null)
                return;
            text.text = NutritionFormat.Time(seconds);
            text.color = seconds <= 30f ? warningColor : normalColor;
        }

        public void SetGrace(float seconds)
        {
            if (text == null)
                return;
            text.text = $"Waktu habis · {Mathf.CeilToInt(seconds)}";
            text.color = warningColor;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
