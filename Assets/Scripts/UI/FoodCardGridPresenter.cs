using System.Collections.Generic;
using UnityEngine;
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
