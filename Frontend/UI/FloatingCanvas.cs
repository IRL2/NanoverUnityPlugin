﻿using UnityEngine;
using UnityEngine.Assertions;

namespace Narupa.Frontend.UI
{
    public class FloatingCanvas : MonoBehaviour
    {
        [SerializeField]
        private float distance;

        [SerializeField]
        private float verticalOffset;

        [SerializeField]
        private Camera camera;

        [SerializeField]
        private float smoothTime;

        [SerializeField]
        private float acceptableDistance = 0.1f;

        [SerializeField]
        private float unacceptableDistance = 0.8f;

        private bool isMoving = true;

        private void Start()
        {
            Assert.IsNotNull(camera);
        }

        private Vector3 velocity = Vector3.zero;

        private bool setup = false;

        private void Update()
        {
            if (!setup)
            {
                UpdatePosition(true);
                setup = true;
            }
            UpdatePosition();
        }

        private void UpdatePosition(bool force = false)
        {
            var cameraPos = camera.transform.position;
            var cameraForward = camera.transform.forward;

            cameraForward.y = 0;
            cameraForward = cameraForward.normalized;

            var currentPosition = transform.position;
            var idealPosition = cameraPos + cameraForward * distance + verticalOffset * Vector3.up;

            var currentFacing = currentPosition - cameraPos;
            currentFacing.y = 0;
            
            if (force)
            {
                transform.position = idealPosition;
                transform.rotation = Quaternion.LookRotation(currentFacing, Vector3.up);
            }
            else
            {
                var distance = Vector3.Distance(currentPosition, idealPosition);

                if (distance > unacceptableDistance && !isMoving)
                    isMoving = true;
                else if (distance < acceptableDistance && isMoving)
                    isMoving = false;

                if (isMoving)
                {
                    transform.position = Vector3.SmoothDamp(currentPosition,
                                                            idealPosition,
                                                            ref velocity,
                                                            smoothTime);
                    transform.rotation = Quaternion.LookRotation(currentFacing, Vector3.up);
                }
            }

           
        }
    }
}