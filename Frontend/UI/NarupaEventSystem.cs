using UnityEngine.EventSystems;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Override the Unity <see cref="EventSystem"/> so that losing application focus does not affect the
    /// UI.
    /// </summary>
    public class NarupaEventSystem : EventSystem
    {
        protected override void OnApplicationFocus(bool hasFocus)
        {
            // Prevent application focus from affecting the event system
        }
    }
}