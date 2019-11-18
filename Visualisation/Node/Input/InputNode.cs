// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using UnityEngine;

namespace Narupa.Visualisation.Node.Input
{
    /// <summary>
    /// Generic input for the visualisation system that provides some value with a
    /// given key.
    /// </summary>
    [Serializable]
    public abstract class InputNode<TProperty> where TProperty : new()
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private TProperty input = new TProperty();

        public string Name => name;

        public TProperty Input => input;
    }
}