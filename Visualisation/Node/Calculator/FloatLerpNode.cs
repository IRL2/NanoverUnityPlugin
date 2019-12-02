using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class FloatLerpNode : LerpNode<float, FloatArrayProperty>
    {
        [SerializeField]
        private FloatProperty speed = new FloatProperty()
        {
            Value = 1f
        };

        private float Delta => speed * Time.deltaTime;

        protected override float MoveTowards(float current, float target)
        {
            return Mathf.MoveTowards(current, target, Delta);
        }
    }
}