// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Frontend.Input;
using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Component that indicates the root game object responsible for representing a controller that is in the scene.
    /// </summary>
    public class VrController : MonoBehaviour
    {
        /// <summary>
        /// Indicate the controller has been reset (connected or disconnected).
        /// </summary>
        /// <param name="controller"></param>
        public void ResetController(VrControllerPrefab controller)
        {
            IsControllerActive = controller != null;
            if (IsControllerActive)
            {
                Cursor = controller.Cursor;
                GripPose = controller.GripPose;
            }
            else
            {
                Cursor = null;
                GripPose = null;
            }

            ControllerReset?.Invoke();
        }

        /// <summary>
        /// The cursor point where tools should be centered.
        /// </summary>
        public ControllerCursor Cursor { get; private set; }

        /// <summary>
        /// The pose marking the location of a gripped hand.
        /// </summary>
        public IPosedObject GripPose { get; private set; }

        /// <summary>
        /// Is the controller currently active?
        /// </summary>
        public bool IsControllerActive { get; private set; } = false;

        public event Action ControllerReset;
    }
}