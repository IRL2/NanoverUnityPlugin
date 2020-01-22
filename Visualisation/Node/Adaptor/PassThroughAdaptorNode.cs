using System;
using Narupa.Core;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    [Serializable]
    public class PassThroughAdaptorNode : BaseAdaptorNode
    {
        [SerializeField]
        private FrameAdaptorProperty adaptor = new FrameAdaptorProperty();

        public IProperty<IDynamicPropertyProvider> ParentAdaptor => adaptor;

        protected override void OnCreateProperty<T>(string key, IProperty<T> property)
        {
            base.OnCreateProperty(key, property);
            if (adaptor.HasNonNullValue())
                property.LinkedProperty = adaptor.Value.GetOrCreateProperty<T>(key);
        }

        public override void Refresh()
        {
            if (adaptor.IsDirty)
            {
                if (adaptor.HasNonNullValue())
                {
                    foreach (var (key, property) in Properties)
                        property.TrySetLinkedProperty(adaptor.Value.GetOrCreateProperty(key, property.PropertyType));
                }
                else
                {
                    foreach (var (key, property) in Properties)
                        property.TrySetLinkedProperty(null);
                }

                adaptor.IsDirty = false;
            }
        }
    }
}