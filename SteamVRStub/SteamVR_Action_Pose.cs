using System;
using UnityEngine;

namespace SteamVRStub
{
    public class SteamVR_Action_Pose
    {
        public bool GetPoseIsValid(SteamVR_Input_Sources source) => throw new NotImplementedException();
        public Vector3 GetLocalPosition(SteamVR_Input_Sources source) => throw new NotImplementedException();
        public Quaternion GetLocalRotation(SteamVR_Input_Sources source) => throw new NotImplementedException();
        public void AddOnChangeListener(SteamVR_Input_Sources poseSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources> poseUpdated) => throw new NotImplementedException();
    }
}
