// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;
using UnityEngine.Assertions;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Describes the various poses which make up a physical controller.
    /// </summary>
    /// <remarks>
    /// This component should be on the root object of a prefab representing a given VR
    /// controller of a certain orientation. It must have references to three well
    /// defined pivots on the controller - the cursor pivot off the end of the
    /// controller where a tool gizmo would be situated, a grip pivot which indicates
    /// where the hand is roughly positioned and hence where resizes should be based
    /// around, and a head pivot which indicates the tip of the controller, which can
    /// be used to interact physically with UI.
    /// </remarks>
    public class VrControllerPrefab : MonoBehaviour
    {
        [SerializeField]
        private ControllerPivot cursor;

        [SerializeField]
        private ControllerPivot grip;

        [SerializeField]
        private ControllerPivot head;

        /// <summary>
        /// The cursor point where tools should be centered.
        /// </summary>
        public ControllerPivot Cursor => cursor;

        /// <summary>
        /// The pivot marking the grip of the controller.
        /// </summary>
        public ControllerPivot Grip => grip;

        /// <summary>
        /// The pivot marking the physical bulk of the controller.
        /// </summary>
        public ControllerPivot Head => head;

        private void Awake()
        {
            Assert.IsNotNull(grip);
            Assert.IsNotNull(head);
            Assert.IsNotNull(cursor);
        }
    }
}