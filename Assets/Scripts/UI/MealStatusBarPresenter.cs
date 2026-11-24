using TMPro;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class MealStatusBarPresenter : MonoBehaviour
    {
        public TMP_Text statusLabel;
        public TMP_Text countsLabel;
        public Color neutralColor = new Color(0.62f, 0.62f, 0.62f);
        public Color balancedColor = new Color(0.18f, 0.49f, 0.20f);
        public Color warningColor = new Color(0.78f, 0.16f, 0.16f);

        public void SetNeutral()
        {
            if (statusLabel != null)
            {
                statusLabel.text = "BELUM DINILAI";
                statusLabel.color = neutralColor;
            }
            SetRequirements(MealRequirementState.Empty);
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

            var color = ColorFor(result.band);
            if (statusLabel != null)
            {
                statusLabel.text = LabelFor(result.band);
                statusLabel.color = color;
            }
        }

        public void SetRequirements(MealRequirementState requirement)
        {
            if (countsLabel == null)
                return;

            countsLabel.text =
                $"KARBO {Mark(requirement.carbohydrate)}  " +
                $"PROTEIN {Mark(requirement.protein)}  " +
                $"SAYUR {Mark(requirement.vegetable)}  " +
                $"TAMBAHAN {Mark(requirement.extra)}";
        }

        private static string Mark(bool value)
        {
            return value ? "[x]" : "[ ]";
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
