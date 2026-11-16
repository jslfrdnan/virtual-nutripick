using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutriPick.UI
{
    public class FoodCardView : MonoBehaviour
    {
        public Image iconImage;
        public Sprite placeholderSprite;
        public TMP_Text nameText;
        public TMP_Text proteinTypeText;
        public TMP_Text caloriesText;
        public TMP_Text proteinText;
        public TMP_Text carbsText;
        public TMP_Text fatText;
        public Image cardBackground;
        public Image healthAccent;
        public GameObject focusOutline;

        public string FoodId { get; private set; }

        private bool focused;
        private bool highlighted;

        public void Bind(FoodCardModel model, Sprite placeholder)
        {
            if (model == null)
            {
                Clear();
                return;
            }

            FoodId = model.foodId;
            if (iconImage != null)
            {
                var sprite = model.icon != null ? model.icon : (placeholder != null ? placeholder : placeholderSprite);
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
            if (nameText != null) nameText.text = model.displayName;
            if (proteinTypeText != null) proteinTypeText.text = model.proteinTypeLabel;
            if (caloriesText != null) caloriesText.text = model.caloriesText;
            if (proteinText != null) proteinText.text = "P " + model.proteinText;
            if (carbsText != null) carbsText.text = "K " + model.carbsText;
            if (fatText != null) fatText.text = "L " + model.fatText;
            if (healthAccent != null)
                healthAccent.color = model.isHealthy
                    ? new Color(0.18f, 0.49f, 0.20f)
                    : new Color(0.98f, 0.66f, 0.15f);
            if (cardBackground != null)
                cardBackground.color = new Color(1f, 1f, 1f, 0.96f);
            SetFocused(false);
            SetHighlighted(false);
        }

        public void SetFocused(bool value)
        {
            focused = value;
            RefreshOutline();
        }

        public void SetHighlighted(bool value)
        {
            highlighted = value;
            RefreshOutline();
            if (cardBackground != null)
                cardBackground.color = highlighted
                    ? new Color(0.92f, 0.98f, 0.92f, 1f)
                    : new Color(1f, 1f, 1f, 0.96f);
        }

        public void Clear()
        {
            FoodId = string.Empty;
            focused = false;
            highlighted = false;
            RefreshOutline();
        }

        private void RefreshOutline()
        {
            if (focusOutline != null)
                focusOutline.SetActive(focused || highlighted);
        }
    }
}
