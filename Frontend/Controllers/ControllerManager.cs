// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Manager class for accessing the left and right controller.
    /// </summary>
    public class ControllerManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private VrController leftController;

        [SerializeField]
        private VrController rightController; 
#pragma warning restore 0649

        /// <summary>
        /// The left <see cref="VrController" />.
        /// </summary>
        public VrController LeftController => leftController;

        /// <summary>
        /// The right <see cref="VrController" />.
        /// </summary>
        public VrController RightController => rightController;
    }
}