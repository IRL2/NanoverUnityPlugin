using System;
using System.Collections;
using System.Linq;
using Nanover.Core.Math;
using Nanover.Frontend.XR;
using UnityEngine;
using UnityEngine.XR;

namespace Nanover.Frontend.Controllers
{
    public class VrControllerShim : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private VrController left;
        [SerializeField]
        private VrController right;
        [SerializeField]
        private GameObject[] uiCanvases;
#pragma warning restore 0649

        private Coroutine updatePosesCoroutine;

        public bool UiIsVisible => uiCanvases.Any(canvas => canvas.activeInHierarchy);

        private void OnEnable()
        {
            updatePosesCoroutine = StartCoroutine(UpdatePoses());
        }

        private void OnDisable()
        {
            StopCoroutine(updatePosesCoroutine);
        }

        private IEnumerator UpdatePoses()
        {
            var leftHand = InputDeviceCharacteristics.Left.WrapAsPosedObject();
            var rightHand = InputDeviceCharacteristics.Right.WrapAsPosedObject();

            while (true)
            {
                if (leftHand.Pose is { } leftPose)
                    SetPose(left.transform, leftPose);
                if (rightHand.Pose is { } rightPose)
                    SetPose(right.transform, rightPose);

                yield return null;
            }

            void SetPose(Transform transform, Transformation pose)
            {
                transform.position = pose.Position;
                transform.rotation = pose.Rotation;
            }
        }
    }
}