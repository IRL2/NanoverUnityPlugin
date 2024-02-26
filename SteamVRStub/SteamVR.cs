using System;
using System.Text;
using UnityEngine;

namespace SteamVRStub
{
    public class SteamVR
    {
        public class Overlay
        {
            public void HideKeyboard() => throw new NotImplementedException();
            public object GetKeyboardText(StringBuilder textBuilder, int v) => throw new NotImplementedException();
            public void ShowKeyboard(int inputMode, int lineMode, string v1, int v2, string text, bool minimalMode, int v3) => throw new NotImplementedException();
        }

        public static SteamVR instance;
        public Overlay overlay;
    }
}
