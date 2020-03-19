// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using UnityEngine;
using Valve.VR;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Define the mode of the controller as 'interaction', setting up the correct
    /// gizmo.
    /// </summary>
    /// <remarks>
    /// This is a temporary object until a more sophisticated multi-mode tool setup is
    /// completed.
    /// </remarks>
    public class InputMode : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private ControllerManager controller;

        [SerializeField]
        private GameObject gizmo;

        [SerializeField]
        private SteamVR_ActionSet actionSet;
#pragma warning restore 0649

        private void OnEnable()
        {
            actionSet.Activate();
            
            controller.LeftController.ControllerReset += SetupLeftController;
            controller.RightController.ControllerReset += SetupRightController;
            
            SetupLeftController();
            SetupRightController();
        }

        private void OnDisable()
        {
            actionSet.Deactivate();
        }

        private void SetupLeftController()
        {
            SetupController(controller.LeftController);
        }

        private void SetupRightController()
        {
            SetupController(controller.RightController);
        }

        private void SetupController(VrController controller)
        {
            if (controller.IsControllerActive)
                controller.InstantiateCursorGizmo(gizmo);
        }
    }
}