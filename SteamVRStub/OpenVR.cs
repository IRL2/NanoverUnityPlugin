using System;
using System.Text;
using UnityEngine;

namespace SteamVRStub
{
    public class OpenVR
    {
        public class SystemObject
        {
            public int GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property, object p, int v, ref ETrackedPropertyError error) => throw new NotImplementedException();
        }

        public class ChaperoneObject
        {
            public void GetPlayAreaSize(ref float x, ref float z) => throw new NotImplementedException();
            public ChaperoneCalibrationState GetCalibrationState() => throw new NotImplementedException();
            public bool GetPlayAreaRect(ref HmdQuad_t rect) => throw new NotImplementedException();
        }

        public static SystemObject System;
        public static ChaperoneObject Chaperone;
    }
}
