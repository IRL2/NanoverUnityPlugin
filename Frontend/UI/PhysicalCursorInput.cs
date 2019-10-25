using System.Collections.Generic;
using Narupa.Frontend.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Implementation of <see cref="BaseInput" /> that uses a physical object near a canvas as a
    /// mouse pointer.
    /// </summary>
    public class WorldSpaceCursorInput : BaseInput
    {
        [SerializeField]
        private SteamVR_Action_Boolean inputAction;

        private bool isInitialised = false;

        private void Update()
        {
            if (!isInitialised && EventSystem.current.currentInputModule != null)
            {
                var eventSystem = EventSystem.current;
                var inputModule = eventSystem.currentInputModule;

                inputModule.inputOverride = this;
                isInitialised = true;
            }

            UpdateInput();
        }

        /// <summary>
        /// Physical object which provides the cursor location.
        /// </summary>
        [SerializeField]
        private ControllerPivot cursor;

        [SerializeField]
        private Camera camera;

        [SerializeField]
        private List<Canvas> canvases = new List<Canvas>();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (cursor != null)
                Gizmos.DrawLine(CursorObjectPosition, CursorProjectedPosition);
        }

        /// <summary>
        /// World space position of the cursor object, including the offset
        /// </summary>
        protected Vector3 CursorObjectPosition => cursor.transform.position;

        protected Vector3 CursorProjectedPosition => ProjectOntoXyPlane(CursorObjectPosition, canvases);

        protected Vector2 CursorScreenPosition => camera.WorldToScreenPoint(CursorProjectedPosition);

        /// <summary>
        /// Get the world position if worldPoint is projected onto the xy plane of the nearest canvas.
        /// </summary>
        public Vector3 ProjectOntoXyPlane(Vector3 worldPoint, IReadOnlyCollection<Canvas> canvases)
        {
            var maxSqrDistance = float.MaxValue;
            var pt = Vector3.zero;
            foreach (var canvas in canvases)
            {
                var planeTransform = canvas.transform;
                var local = planeTransform.InverseTransformPoint(worldPoint);
                local.z = 0;
                var world = planeTransform.TransformPoint(local);
                var projSqrDistance = Vector3.SqrMagnitude(world - CursorObjectPosition);
                if (projSqrDistance < maxSqrDistance)
                {
                    maxSqrDistance = projSqrDistance;
                    pt = world;
                }
            }

            return pt;
        }

        private bool prevState = false;
        private bool currentState = false;

        private void UpdateInput()
        {
            prevState = currentState;
            currentState = mousePresent && inputAction.GetState(SteamVR_Input_Sources.RightHand);
        }

        public override Vector2 mousePosition => CursorScreenPosition;

        public override bool mousePresent => cursor != null &&
                                             Vector3.Distance(CursorProjectedPosition,
                                                              CursorObjectPosition) < cursor.Radius;

        public override bool GetMouseButtonDown(int button)
        {
            return button == 0 && currentState && !prevState;
        }

        public override bool GetMouseButton(int button)
        {
            return button == 0 && currentState;
        }

        public override bool GetMouseButtonUp(int button)
        {
            return button == 0 && !currentState && prevState;
        }
    }
}