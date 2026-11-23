using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NutriPick.UI
{
    /// <summary>
    /// Adds a visible focus ring for controller selection and pointer hover.
    /// Keeps the button color readable while the EventSystem owns the selection.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class SelectableFocusHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Color outlineColor = new Color(1f, 0.78f, 0.22f, 1f);
        public Vector2 outlineDistance = new Vector2(4f, -4f);
        public Color normalColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        public Color highlightedColor = Color.white;

        private Selectable selectable;
        private Outline outline;
        private bool hovered;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            if (selectable == null || selectable.targetGraphic == null)
                return;

            outline = selectable.targetGraphic.GetComponent<Outline>();
            if (outline == null)
                outline = selectable.targetGraphic.gameObject.AddComponent<Outline>();

            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDistance;
            outline.enabled = false;
            ApplyColors();
        }

        private void OnDisable()
        {
            if (outline != null)
                outline.enabled = false;
        }

        private void OnEnable()
        {
            if (outline != null)
                outline.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            UpdateOutline();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            UpdateOutline();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            UpdateOutline();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            UpdateOutline();
        }

        private void UpdateOutline()
        {
            if (outline == null)
                return;

            bool selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
            outline.enabled = hovered || selected;
        }

        private void ApplyColors()
        {
            if (selectable == null)
                return;

            var colors = selectable.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor;
            colors.selectedColor = highlightedColor;
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.colorMultiplier = 1f;
            selectable.colors = colors;
        }
    }
}
