using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FalseWitness.UI
{
    /// <summary>
    /// Generic clickable list row used for suspects, statements, and evidence in the
    /// investigation UI shell. Purely presentational - it only reports clicks and
    /// renders its own selected/unselected visual state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SelectableListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        [SerializeField] private Color normalColor = new(0.16f, 0.16f, 0.18f, 1f);
        [SerializeField] private Color selectedColor = new(0.20f, 0.45f, 0.75f, 1f);

        private Action onClicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);

            button.onClick.AddListener(HandleClicked);
        }

        public void Setup(string text, Action onClick)
        {
            if (label != null) label.text = text;
            onClicked = onClick;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }

        private void HandleClicked()
        {
            onClicked?.Invoke();
        }
    }
}
