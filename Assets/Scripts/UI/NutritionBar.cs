using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class NutritionBar : MonoBehaviour
    {
        public Image fill;
        public TMP_Text valueText;
        public TMP_Text labelText;
        public TMP_Text countsText;
        public Color green = new Color(0.18f, 0.49f, 0.20f);
        public Color yellow = new Color(0.98f, 0.66f, 0.15f);
        public Color red = new Color(0.78f, 0.16f, 0.16f);
        public Color gray = new Color(0.62f, 0.62f, 0.62f);

        public void SetNeutral()
        {
            StopAllCoroutines();
            if (fill != null)
            {
                fill.fillAmount = 0.5f;
                fill.color = gray;
            }
            if (valueText != null) valueText.text = "50";
            if (labelText != null) labelText.text = "Belum dinilai";
            if (countsText != null) countsText.text = "Sehat 0 · Kurang sehat 0";
        }

        public void SetPreview(MealResult result)
        {
            if (result == null)
                return;
            StopAllCoroutines();
            StartCoroutine(Animate(result, 0.15f));
        }

        public void SetFinal(MealResult result)
        {
            if (result == null)
                return;
            StopAllCoroutines();
            StartCoroutine(Animate(result, 1.0f));
        }

        private IEnumerator Animate(MealResult result, float duration)
        {
            float start = fill != null ? fill.fillAmount : 0f;
            float target = Mathf.Clamp01(result.score / 100f);
            Color startColor = fill != null ? fill.color : gray;
            Color targetColor = ColorFor(result.band);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (fill != null)
                {
                    fill.fillAmount = Mathf.Lerp(start, target, t);
                    fill.color = Color.Lerp(startColor, targetColor, t);
                }
                yield return null;
            }

            if (fill != null)
            {
                fill.fillAmount = target;
                fill.color = targetColor;
            }
            if (valueText != null) valueText.text = result.score.ToString();
            if (labelText != null) labelText.text = result.bandLabel;
            if (countsText != null)
                countsText.text = $"Sehat {result.healthyCount} · Kurang sehat {result.lessHealthyCount}";
        }

        private Color ColorFor(ScoreBand band)
        {
            return band switch
            {
                ScoreBand.Balanced => green,
                ScoreBand.Adequate => yellow,
                ScoreBand.NeedsImprovement => red,
                _ => gray
            };
        }
    }
}
