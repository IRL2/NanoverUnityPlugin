using System;
using UnityEngine;

namespace SteamVRStub
{
    public class SteamVR_Behaviour_Pose
    {
        public class DeviceIndexChangedEvent
        {
            public void AddListener(Action<SteamVR_Behaviour_Pose, SteamVR_Input_Sources, int> onDeviceIndexChanged) => throw new NotImplementedException();
        }

        public DeviceIndexChangedEvent onDeviceIndexChanged;
    }
}
