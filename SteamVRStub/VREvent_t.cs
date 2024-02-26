using System;
using UnityEngine;

namespace SteamVRStub
{
    public class VREvent_t
    {
        public class Keyboard
        {
            public byte cNewInput0;
            public byte cNewInput4;
            public byte cNewInput1;
            public byte cNewInput5;
            public byte cNewInput6;
            public byte cNewInput2;
            public byte cNewInput3;
            public byte cNewInput7;
        }

        public class Data
        {
            public Keyboard keyboard;
        }

        public Data data => new Data();
    }
}
