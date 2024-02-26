using System;
using UnityEngine;

namespace SteamVRStub
{
    public class SteamVR_Action_Boolean
    {
        public bool state;
        public Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> onStateDown;
        public Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> onStateUp;

        public bool GetState(SteamVR_Input_Sources source) => throw new NotImplementedException();
        public void AddOnStateDownListener(Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> stateDown, SteamVR_Input_Sources source) => throw new NotImplementedException();
        public void RemoveOnStateDownListener(Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> stateDown, SteamVR_Input_Sources source) => throw new NotImplementedException();
        public void AddOnStateUpListener(Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> stateUp, SteamVR_Input_Sources source) => throw new NotImplementedException();
        public void RemoveOnStateUpListener(Action<SteamVR_Action_Boolean, SteamVR_Input_Sources> stateUp, SteamVR_Input_Sources source) => throw new NotImplementedException();
    }
}
