using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class FoodCardGridPresenter : MonoBehaviour
    {
        public RectTransform cardContainer;
        public FoodCardView cardPrefab;
        public Sprite placeholderSprite;

        private readonly List<FoodCardView> cards = new List<FoodCardView>();
        private string focusedId;
        private string highlightedId;

        public IReadOnlyList<FoodCardView> Cards => cards;
        public int CardCount => cards.Count;

        public void Bind(IReadOnlyList<FoodData> foods, NutritionRules rules)
        {
            ClearCards();
            focusedId = null;
            highlightedId = null;

            if (foods == null || cardContainer == null || cardPrefab == null)
                return;

            foreach (var food in foods)
            {
                if (food == null)
                    continue;

                var card = Instantiate(cardPrefab, cardContainer);
                card.Bind(FoodCardModel.From(food, rules), placeholderSprite);
                cards.Add(card);
            }

            ResizeToContent();
        }

        private void ResizeToContent()
        {
            var root = transform as RectTransform;
            var layout = cardContainer != null ? cardContainer.GetComponent<GridLayoutGroup>() : null;
            if (root == null || layout == null || cards.Count == 0)
                return;

            int columns = Mathf.Max(1, layout.constraintCount);
            int rows = Mathf.CeilToInt(cards.Count / (float)columns);
            var padding = layout.padding;
            float height = padding.top + padding.bottom + rows * layout.cellSize.y +
                Mathf.Max(0, rows - 1) * layout.spacing.y;
            root.sizeDelta = new Vector2(root.sizeDelta.x, height);
        }

        public void Highlight(string foodId)
        {
            if (highlightedId == foodId)
                return;
            highlightedId = foodId;
            Refresh();
        }

        public void Focus(string foodId)
        {
            focusedId = foodId;
            highlightedId = foodId;
            Refresh();
        }

        public void ClearFocus()
        {
            focusedId = null;
            Refresh();
        }

        public void ClearHighlight()
        {
            highlightedId = null;
            Refresh();
        }

        public void ClearCards()
        {
            foreach (var card in cards)
            {
                if (card == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(card.gameObject);
                else
                    DestroyImmediate(card.gameObject);
            }
            cards.Clear();
        }

        private void Refresh()
        {
            foreach (var card in cards)
            {
                if (card == null)
                    continue;

                card.SetFocused(card.FoodId == focusedId);
                card.SetHighlighted(card.FoodId == highlightedId);
            }
        }
    }
}
