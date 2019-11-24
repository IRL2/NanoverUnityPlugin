using System.Collections.Generic;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    public static class VisualisationComponentExtensions
    {
        /// <summary>
        /// Get all visualiation nodes that are in children of the game object.
        /// </summary>
        public static IEnumerable<TNode> GetVisualisationNodesInChildren<TNode>(this GameObject go)
        {
            foreach(var comp in go.GetComponentsInChildren<VisualisationComponent>())
                if (comp.GetWrappedVisualisationNode() is TNode node)
                    yield return node;
        }
        
        public static IEnumerable<TNode> GetVisualisationNodes<TNode>(this GameObject go)
        {
            foreach(var comp in go.GetComponents<VisualisationComponent>())
                if (comp.GetWrappedVisualisationNode() is TNode node)
                    yield return node;
        }
    }
}