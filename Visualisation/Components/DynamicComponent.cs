using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Property;
using Plugins.Narupa.Visualisation.Components;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    [ExecuteAlways]
    public class DynamicComponent : MonoBehaviour
    {
        [SerializeField]
        private FrameAdaptor adaptor;

        [SerializeField]
        private GameObject[] prefabs = new GameObject[0];

        private List<GameObject> current = new List<GameObject>();

        public FrameAdaptor FrameAdaptor
        {
            get => adaptor;
            set => adaptor = value;
        }

        private void OnEnable()
        {
            SetupSubgraphs();
        }

        private void SetupSubgraphs()
        {
            if (current.Count == 0)
            {
                foreach (var prefab in prefabs)
                {
                    var node = Instantiate(prefab, transform);
                    node.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    SetupInputs(node);
                    current.Add(node);
                }
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

        private void SetupInputs(GameObject o)
        {
            foreach (var component in o.GetComponentsInChildren<VisualisationComponent>())
            {
                if (component.GetWrappedVisualisationNode() is IInputNode input)
                {
                    FindInput(input);
                }
            }
        }

        /// <summary>
        /// Try to find either a previous component that has an output node with a matching name, or link to the frame adaptor.
        /// </summary>
        private void FindInput(IInputNode input)
        {
            foreach (var existing in current)
            {
                if (GetOutput(existing, input.Name)?.Output is IReadOnlyProperty output)
                {
                    input.Input.TrySetLinkedProperty(output);
                    return;
                }
            }

            foreach (var mainInput in GetInputs(gameObject))
            {
                if (input.Name == mainInput.Name)
                {
                    input.Input.TrySetLinkedProperty(mainInput.Input);
                    return;
                }
            }

            if (input.Input.HasValue)
                return;
        
            input.Input.TrySetLinkedProperty(
                adaptor.GetOrCreateProperty(input.Name,
                                            input.Input.PropertyType));
        }

        private static IEnumerable<IInputNode> GetInputs(GameObject obj)
        {
            return obj.GetComponents<VisualisationComponent>()
                      .Where(c => c.GetWrappedVisualisationNode() is IInputNode)
                      .Select(c => c.GetWrappedVisualisationNode() as IInputNode);
        }

        private static IOutputNode GetOutput(GameObject existing, string name)
        {
            return existing.GetComponentsInChildren<VisualisationComponent>().FirstOrDefault(
                           c => c.GetWrappedVisualisationNode() is IOutputNode node &&
                                node.Name == name)?
                       .GetWrappedVisualisationNode() as IOutputNode;
        }

        public void SetSubgraphs(params GameObject[] subgraphs)
        {
            prefabs = subgraphs ?? new GameObject[0];
            SetupSubgraphs();
        }
    }
}