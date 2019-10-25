using Narupa.Frontend.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Implementation of <see cref="BaseInput" /> that uses a physical object's near a canvas as a
    /// mouse pointer.
    /// </summary>
    public class WorldSpaceCursorInput : BaseInput
    {
        private bool isInitialised = false;

        public static WorldSpaceCursorInput Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

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
        /// Register a canvas with a controller pivot and a click action.
        /// </summary>
        public void RegisterCanvas(Canvas canvas,
                                   IPosedObject cursor,
                                   IButton click)
        {
            this.canvas = canvas;
            this.cursor = cursor;
            clickButton = click;
        }

        /// <summary>
        /// Unregister the current canvas.
        /// </summary>
        public void UnregisterCanvas()
        {
            canvas = null;
            cursor = null;
        }

        private IPosedObject cursor;
        private Canvas canvas;
        private IButton clickButton;

        [SerializeField]
        private Camera camera;

        private float distanceToCanvas;
        private Vector2 screenPosition;
        private bool isCursorOnCanvas;

        private void UpdateInput()
        {
            Vector3? worldPoint;
            worldPoint = GetProjectedCursorPoint();

            isCursorOnCanvas = worldPoint.HasValue;
            if (worldPoint.HasValue)
            {
                screenPosition = camera.WorldToScreenPoint(worldPoint.Value);
            }

            prevState = currentState;
            currentState = mousePresent && clickButton.IsPressed;
        }


        /// <summary>
        /// Get the projection of the cursor onto the canvas, returning null if it is too far away
        /// </summary>
        public Vector3? GetProjectedCursorPoint()
        {
            if (cursor?.Pose == null)
                return null;
            var cursorRadius = cursor.Pose.Value.Scale.x * 1.5f;
            var planeTransform = canvas.transform;
            var local = planeTransform.InverseTransformPoint(cursor.Pose.Value.Position);
            local.z = 0;
            var world = planeTransform.TransformPoint(local);
            var projSqrDistance = Vector3.SqrMagnitude(world - cursor.Pose.Value.Position);
            if (projSqrDistance > cursorRadius * cursorRadius)
                return null;
            return world;
        }

        private bool prevState = false;
        private bool currentState = false;

        public override Vector2 mousePosition => screenPosition;

        public override bool mousePresent => isCursorOnCanvas;

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