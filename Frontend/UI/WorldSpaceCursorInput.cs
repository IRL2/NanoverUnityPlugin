// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections;
using Narupa.Frontend.Input;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Implementation of <see cref="BaseInput" /> that uses a physical object's near a
    /// canvas as a mouse pointer.
    /// </summary>
    public class WorldSpaceCursorInput : BaseInput
    {
        private static WorldSpaceCursorInput Instance { get; set; }

        [SerializeField]
        private Camera camera;

        private IPosedObject cursor;
        private Canvas canvas;
        private IButton clickButton;

        private float distanceToCanvas;
        private Vector2 screenPosition;
        private bool isCursorOnCanvas;

        private bool previousClickState = false;
        private bool currentClickState = false;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void Start()
        {
            base.Start();
            Assert.IsNotNull(camera);
            StartCoroutine(InitialiseWhenInputModuleReady());
        }

        private void Update()
        {
            Vector3? worldPoint;
            worldPoint = GetProjectedCursorPoint();

            isCursorOnCanvas = worldPoint.HasValue;
            if (worldPoint.HasValue)
            {
                screenPosition = camera.WorldToScreenPoint(worldPoint.Value);
            }

            previousClickState = currentClickState;
            currentClickState = mousePresent && clickButton.IsPressed;
        }

        /// <summary>
        /// Sets the canvas with an <see cref="IPosedObject" /> to provide the location
        /// of the physical cursor and an <see cref="IButton" /> to provide information on
        /// if a click is occuring.
        /// </summary>
        public static void SetCanvasAndCursor(Canvas canvas,
                                              IPosedObject cursor,
                                              IButton click)
        {
            Assert.IsNotNull(Instance);
            Instance.canvas = canvas;
            Instance.cursor = cursor;
            Instance.clickButton = click;
        }

        /// <summary>
        /// Coroutine that waits until the <see cref="EventSystem" /> has prepared the
        /// input module before overriding the input.
        /// </summary>
        private IEnumerator InitialiseWhenInputModuleReady()
        {
            while (EventSystem.current.currentInputModule == null)
                yield return new WaitForEndOfFrame();

            var eventSystem = EventSystem.current;
            var inputModule = eventSystem.currentInputModule;

            inputModule.inputOverride = this;
        }

        /// <summary>
        /// Get the projection of the cursor onto the canvas, returning null if it is too
        /// far away.
        /// </summary>
        private Vector3? GetProjectedCursorPoint()
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

        /// <inheritdoc cref="BaseInput.mousePosition" />
        public override Vector2 mousePosition => screenPosition;

        /// <inheritdoc cref="BaseInput.mousePresent" />
        public override bool mousePresent => isCursorOnCanvas;

        /// <inheritdoc cref="BaseInput.GetMouseButtonDown" />
        public override bool GetMouseButtonDown(int button)
        {
            return button == 0 && currentClickState && !previousClickState;
        }

        /// <inheritdoc cref="BaseInput.GetMouseButton" />
        public override bool GetMouseButton(int button)
        {
            return button == 0 && currentClickState;
        }

        /// <inheritdoc cref="BaseInput.GetMouseButtonUp" />
        public override bool GetMouseButtonUp(int button)
        {
            return button == 0 && !currentClickState && previousClickState;
        }

        public static void ReleaseCanvas(Canvas canvas)
        {
            if (Instance != null && Instance.canvas == canvas)
            {
                Instance.canvas = null;
                Instance.cursor = null;
            }
        }
    }
}