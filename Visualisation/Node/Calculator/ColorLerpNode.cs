using System;
using NanoVer.Visualisation.Properties;
using NanoVer.Visualisation.Properties.Collections;
using NanoVer.Visualisation.Property;
using UnityEngine;

namespace NanoVer.Visualisation.Node.Calculator
{
    [Serializable]
    public class ColorLerpNode : LerpNode<UnityEngine.Color, ColorArrayProperty>
    {
        [SerializeField]
        private FloatProperty speed = new FloatProperty()
        {
            Value = 1f
        };

        private float Delta => Application.isPlaying ? speed * Time.deltaTime : 999f;

        protected override UnityEngine.Color MoveTowards(UnityEngine.Color current, UnityEngine.Color target)
        {
            return new UnityEngine.Color(
                Mathf.MoveTowards(current.r, target.r, Delta),
                Mathf.MoveTowards(current.g, target.g, Delta),
                Mathf.MoveTowards(current.b, target.b, Delta),
                Mathf.MoveTowards(current.a, target.a, Delta)
            );
        }
    }
}