using System;
using UnityEngine;

namespace SteamVRStub
{
    public class SteamVR_Events
    {
        public class SystemObject
        {
            public void Listen(Action<VREvent_t> onKeyboard) => throw new NotImplementedException();
        }

        public static SystemObject System(EVREventType vREvent_KeyboardCharInput) => throw new NotImplementedException();
    }
}
