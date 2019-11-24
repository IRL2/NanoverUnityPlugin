using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Output;
using Narupa.Visualisation.Property;
using Plugins.Narupa.Visualisation.Components;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    [ExecuteAlways]
    public class DynamicComponent : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] prefabs = new GameObject[0];

        private readonly List<GameObject> current = new List<GameObject>();

        [field: SerializeField] public FrameAdaptor FrameAdaptor { get; set; }

        private void OnEnable()
        {
            SetupSubgraphs();
        }

        private void SetupSubgraphs()
        {
            if (current.Count == 0)
                foreach (var prefab in prefabs)
                {
                    var node = Instantiate(prefab, transform);
                    node.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    SetupAllInputNodes(node);
                    current.Add(node);
                }
        }

        private void OnDisable()
        {
            if (Application.isEditor)
            {
                foreach (var obj in current)
                    DestroyImmediate(obj);
                current.Clear();
            }
        }

        private void SetupAllInputNodes(GameObject o)
        {
            foreach (var node in o.GetVisualisationNodesInChildren<IInputNode>())
                    SetupInputNode(node);
        }

        /// <summary>
        /// For a given input node, find an appropriate output port to bind to.
        /// </summary>
        /// <remarks>
        /// The order in which an appropriate property to link to is:
        /// 1) Search each of the subgraphs for an <see cref="IOutputNode"/> of the same name.
        /// 2) Search the root object for an <see cref="IInputNode"/> with the same name.
        /// 3) If there is no default value, bind to the frame adaptor.
        /// </remarks>
        private void SetupInputNode(IInputNode input)
        {
            foreach (var existing in current)
                if (GetOutputNodeWithName(existing, input.Name)?.Output is IReadOnlyProperty output)
                {
                    input.Input.TrySetLinkedProperty(output);
                    return;
                }

            foreach (var mainInput in GetInputNodes(gameObject))
                if (input.Name == mainInput.Name)
                {
                    input.Input.TrySetLinkedProperty(mainInput.Input);
                    return;
                }

            if (input.Input.HasValue)
                return;

            input.Input.TrySetLinkedProperty(
                FrameAdaptor.GetOrCreateProperty(input.Name,
                                                 input.Input.PropertyType));
        }

        private static IEnumerable<IInputNode> GetInputNodes(GameObject obj)
        {
            return obj.GetVisualisationNodes<IInputNode>();
        }

        private static IOutputNode GetOutputNodeWithName(GameObject existing, string name)
        {
            return existing.GetVisualisationNodes<IOutputNode>().FirstOrDefault(
                    c => c.Name == name);
        }

        public void SetSubgraphs(params GameObject[] subgraphs)
        {
            prefabs = subgraphs ?? new GameObject[0];
            SetupSubgraphs();
        }
    }
}