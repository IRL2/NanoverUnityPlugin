// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Narupa.Visualisation.Property
{
    /// <summary>
    /// Object which may optionally provide a value, along with a callback if this
    /// value is changed.
    /// </summary>
    [Serializable]
    public abstract class Property
    {
        /// <summary>
        /// Callback for when the value is changed or undefined.
        /// </summary>
        public event Action ValueChanged;

        /// <summary>
        /// Internal method, called when the value is changed or undefined.
        /// </summary>
        protected void OnValueChanged()
        {
            ValueChanged?.Invoke();
        }

        /// <summary>
        /// Undefine the value of this property.
        /// </summary>
        public virtual void UndefineValue()
        {
            if (isValueProvided)
            {
                isValueProvided = false;
                OnValueChanged();
            }
        }

        /// <summary>
        /// Override for indicating that the value is null. Unity does not serialize
        /// nullable types, so this is required.
        /// </summary>
        [SerializeField]
        private bool isValueProvided;

        /// <summary>
        /// Does this property define a value
        /// </summary>
        public virtual bool HasValue
        {
            get => isValueProvided;
            protected set => isValueProvided = value;
        }


        public abstract Type PropertyType { get; }
        
        public bool IsDirty { get; set; } = true;
    }

    /// <summary>
    /// Implementation of <see cref="IProperty{TValue}" /> for serialisation in Unity.
    /// </summary>
    public abstract class Property<TValue> : Property, IProperty<TValue>
    {
        /// <summary>
        /// A linked <see cref="Property" /> which can provide a value.
        /// </summary>
        [CanBeNull]
        private IReadOnlyProperty<TValue> linkedProperty;

        /// <summary>
        /// Value serialized by Unity.
        /// </summary>
        [SerializeField]
        private TValue value;

        /// <inheritdoc cref="IProperty{TValue}.HasValue" />
        public override bool HasValue
        {
            get
            {
                if (HasLinkedProperty)
                    return LinkedProperty.HasValue;
                return base.HasValue;
            }
        }

        /// <inheritdoc cref="IProperty{TValue}.Value" />
        public virtual TValue Value
        {
            get
            {
                if (HasLinkedProperty)
                    return LinkedProperty.Value;
                if (HasValue)
                    return value;
                throw new InvalidOperationException(
                    "Tried accessing value of property when it is not defined");
            }
            set
            {
                LinkedProperty = null;
                HasValue = true;
                this.value = value;
                IsDirty = true;
                OnValueChanged();
            }
        }

        /// <inheritdoc cref="IProperty{TValue}.HasLinkedProperty" />
        public bool HasLinkedProperty => LinkedProperty != null;

        /// <inheritdoc cref="IProperty{TValue}.LinkedProperty" />
        [CanBeNull]
        public IReadOnlyProperty<TValue> LinkedProperty
        {
            get => linkedProperty;
            set
            {
                if (value == this)
                    throw new ArgumentException("Cannot link property to itself!");

                // Check no cyclic linked properties will occur
                var linked = value is Property<TValue> linkable ? linkable.LinkedProperty : null;
                while (linked != null)
                {
                    if (linked == this)
                        throw new ArgumentException("Cyclic link detected!");
                    linked = linked is Property<TValue> linkable2 ? linkable2.LinkedProperty : null;
                }


                if (linkedProperty != null)
                    linkedProperty.ValueChanged -= LinkedPropertyOnValueChanged;
                linkedProperty = value;
                if (linkedProperty != null)
                    linkedProperty.ValueChanged += LinkedPropertyOnValueChanged;
                IsDirty = true;
            }
        }

        /// <inheritdoc cref="IProperty{TValue}.UndefineValue" />
        public override void UndefineValue()
        {
            LinkedProperty = null;
            base.UndefineValue();
            IsDirty = true;
        }

        /// <summary>
        /// Implicit conversion of the property to its value
        /// </summary>
        public static implicit operator TValue(Property<TValue> property)
        {
            return property.Value;
        }

        private void LinkedPropertyOnValueChanged()
        {
            IsDirty = true;
            OnValueChanged();
        }

        public override Type PropertyType => typeof(TValue);
    }
}