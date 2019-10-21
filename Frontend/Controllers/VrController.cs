// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Component that indicates the root game object responsible for representing a controller.
    /// </summary>
    public class VrController : MonoBehaviour
    {
        private bool isControllerActive = false;

        [SerializeField]
        private ControllerCursor cursor;

        /// <summary>
        /// Indicate the controller has been reset (connected or disconnected).
        /// </summary>
        /// <param name="controller"></param>
        public void ResetController(GameObject controller)
        {
            isControllerActive = controller != null;
            if (isControllerActive)
            {
                cursor = controller.transform.GetComponentInChildren<ControllerCursor>();
            }
            else
            {
                cursor = null;
            }

            ControllerReset?.Invoke();
        }

        public ControllerCursor Cursor => cursor;

        public bool IsControllerActive => isControllerActive;

        public event Action ControllerReset;
    }
}