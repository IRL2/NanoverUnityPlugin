// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Output
{
    /// <summary>
    /// Generic output for the visualisation system that provides some value with a
    /// given key.
    /// </summary>
    [Serializable]
    public abstract class OutputNode<TProperty> : IOutputNode where TProperty : Property.Property, new()
    {
        IReadOnlyProperty IOutputNode.Output => output;
        
        [SerializeField]
        private string name;

        private TProperty output = new TProperty();

        [SerializeField]
        private TProperty input = new TProperty();

        public OutputNode()
        {
            output.TrySetLinkedProperty(input);
        }
        
        public string Name => name;

        public TProperty Output => output;
    }
    
    public interface IOutputNode
    { 
        string Name { get; }
        
        IReadOnlyProperty Output { get; }
    }
}