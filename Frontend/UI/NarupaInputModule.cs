using UnityEngine;
using UnityEngine.EventSystems;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Override of the default Unity <see cref="StandaloneInputModule"/> that exposes the current hovered game object.
    /// </summary>
    public class NarupaInputModule : StandaloneInputModule
    {
        private static PointerEventData GetPointerEventData(int pointerId = -1)
        {
            PointerEventData eventData;
            Instance.GetPointerData(pointerId, out eventData, true);
            return eventData;
        }

        private static NarupaInputModule Instance
            => EventSystem.current.currentInputModule as NarupaInputModule;

        /// <summary>
        /// Get the current hovered over game object.
        /// </summary>
        public GameObject CurrentHoverTarget => GetPointerEventData().pointerEnter;
    }
}