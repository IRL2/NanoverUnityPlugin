using System;
using UnityEngine;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// A manager for a element which can display a set of options as buttons, such as a radial menu.
    /// </summary>
    public class DynamicMenu : MonoBehaviour
    {
        [SerializeField]
        private UiButton buttonPrefab;

        public void AddItem(string name, Sprite icon, Action callback)
        {
            var button = CreateButton();
            button.Text = name;
            button.Image = icon;
            button.OnClick += callback;
        }

        private UiButton CreateButton()
        {
            var button = Instantiate(buttonPrefab, transform);
            button.gameObject.SetActive(true);
            return button;
        }
    }
}