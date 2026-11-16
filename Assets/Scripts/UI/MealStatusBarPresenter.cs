using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class MealStatusBarPresenter : MonoBehaviour
    {
        public Image fill;
        public RectTransform marker;
        public TMP_Text statusLabel;
        public TMP_Text detailLabel;
        public TMP_Text countsLabel;
        public Color neutralColor = new Color(0.62f, 0.62f, 0.62f);
        public Color balancedColor = new Color(0.18f, 0.49f, 0.20f);
        public Color warningColor = new Color(0.78f, 0.16f, 0.16f);

        public void SetNeutral()
        {
            if (fill != null)
            {
                fill.fillAmount = 0.5f;
                fill.color = neutralColor;
            }
            SetMarker(0.5f);
            if (statusLabel != null)
            {
                statusLabel.text = "BELUM DINILAI";
                statusLabel.color = neutralColor;
            }
            if (detailLabel != null) detailLabel.text = "Pilih makanan untuk mulai";
            if (countsLabel != null) countsLabel.text = "Sehat 0 · Kurang sehat 0";
        }

        public void SetPreview(MealResult result)
        {
            Apply(result);
        }

        public void SetFinal(MealResult result)
        {
            Apply(result);
        }

        private void Apply(MealResult result)
        {
            if (result == null)
            {
                SetNeutral();
                return;
            }

            float value = Mathf.Clamp01(result.score / 100f);
            var color = ColorFor(result.band);
            if (fill != null)
            {
                fill.fillAmount = value;
                fill.color = color;
            }
            SetMarker(value);
            if (statusLabel != null)
            {
                statusLabel.text = LabelFor(result.band);
                statusLabel.color = color;
            }
            if (detailLabel != null) detailLabel.text = result.bandLabel;
            if (countsLabel != null)
                countsLabel.text = $"Sehat {result.healthyCount} · Kurang sehat {result.lessHealthyCount}";
        }

        private void SetMarker(float value)
        {
            if (marker == null)
                return;
            marker.anchorMin = new Vector2(value, 0.5f);
            marker.anchorMax = new Vector2(value, 0.5f);
            marker.anchoredPosition = Vector2.zero;
        }

        private static string LabelFor(ScoreBand band)
        {
            return band switch
            {
                ScoreBand.Balanced => "SEIMBANG",
                ScoreBand.Adequate => "CUKUP",
                ScoreBand.NeedsImprovement => "PERLU PERBAIKAN",
                _ => "BELUM DINILAI"
            };
        }

        private Color ColorFor(ScoreBand band)
        {
            return band switch
            {
                ScoreBand.Balanced => balancedColor,
                ScoreBand.Adequate => new Color(0.98f, 0.66f, 0.15f),
                ScoreBand.NeedsImprovement => warningColor,
                _ => neutralColor
            };
        }
    }
}
