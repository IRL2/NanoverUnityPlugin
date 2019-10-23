using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// A controller prefab which applies to a specific VR system.
    /// </summary>
    public class VrControllerPrefab : MonoBehaviour
    {
        [SerializeField]
        private ControllerPivot gripPose;

        [SerializeField]
        private ControllerCursor cursor;

        /// <summary>
        /// The cursor point where tools should be centered.
        /// </summary>
        public ControllerCursor Cursor => cursor;

        /// <summary>
        /// The pivot marking the center of the controller.
        /// </summary>
        public ControllerPivot GripPose => gripPose;

        private void Awake()
        {
            Assert.IsNotNull(gripPose);
            Assert.IsNotNull(cursor);
        }
    }
}