#nullable enable
using System;
using UnityEngine.InputSystem;


namespace Nanover.Frontend.InputControlSystem.Utilities
{
    public static class InputControlUtilities
    {
        public static string FullyQualifiedInputActionName(InputAction inputAction)
        {
            string assetName = inputAction.actionMap.asset.name;
            string actionMapName = inputAction.actionMap.name;
            string actionName = inputAction.name;
            return $"{assetName}/{actionMapName}/{actionName}";
        }
    }

}