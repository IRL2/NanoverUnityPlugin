using Nanover.Frontend.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nanover.Core.Async;
using Nanover.Core.Math;
using UnityEngine;
using UnityEngine.XR;

namespace Nanover.Frontend.XR
{
    /// <summary>
    /// Extensions for Unity's XR system, in which you make queries about
    /// XRNode types (e.g LeftHand, TrackingReference, etc) and receive
    /// XRNodeState objects containing identifier and tracking information
    /// for that XR node.
    /// </summary>
    public static partial class UnityXRExtensions
    {
        /// <summary>
        /// Return the pose matrix for a given InputDevice, if available.
        /// </summary>
        public static Transformation? GetSinglePose(this InputDevice device)
        {
            if (device.isValid
             && device.TryGetFeatureValue(CommonUsages.devicePosition, out var position)
             && device.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation))
                return new Transformation(position, rotation, Vector3.one);

            return null;
        }

        public static IPosedObject WrapAsPosedObject(this InputDeviceCharacteristics characteristics)
        {
            var devices = new List<InputDevice>();
            var wrapper = new DirectPosedObject();

            UpdatePoseInBackground().AwaitInBackground();

            async Task UpdatePoseInBackground()
            {
                while (true)
                {
                    wrapper.SetPose(GetDevice().GetSinglePose());
                    await Task.Delay(1);
                }
            }

            InputDevice GetDevice()
            {
                InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
                return devices.FirstOrDefault();
            }

            return wrapper;
        }
    }
}