using System;
using UnityEngine;

namespace Narupa.Visualisation.Node.Input
{
    [Serializable]
    public abstract class InputNode<TProperty> where TProperty : new()
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private TProperty input = new TProperty();
    }
}