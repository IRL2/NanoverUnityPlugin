using System;
using System.Text;
using UnityEngine;

namespace SteamVRStub
{
    public class OpenVR
    {
        public class ChaperoneObject
        {
            public void GetPlayAreaSize(ref float x, ref float z) => throw new NotImplementedException();
            public ChaperoneCalibrationState GetCalibrationState() => throw new NotImplementedException();
            public bool GetPlayAreaRect(ref HmdQuad_t rect) => throw new NotImplementedException();
        }

        public static ChaperoneObject Chaperone;
    }
}
