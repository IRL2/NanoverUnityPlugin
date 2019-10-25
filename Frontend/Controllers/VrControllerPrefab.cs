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