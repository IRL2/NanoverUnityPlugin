using System;
using UnityEngine;
using UnityEngine.XR;

namespace SteamVRStub
{
    public class SteamVR_Action_Boolean
    {
        public bool state;
        public Action<SteamVR_Action_Boolean, InputDevice> onStateDown;
        public Action<SteamVR_Action_Boolean, InputDevice> onStateUp;
    }
}
