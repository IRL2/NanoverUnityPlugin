// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    /// <summary>
    /// Extension methods for <see cref="IPropertyProvider" />.
    /// </summary>
    public static class PropertyProviderExtensions
    {
        /// <inheritdoc cref="IPropertyProvider.CanProvideProperty{T}"/>
        public static bool CanProvideProperty(this IPropertyProvider provider,
                                              string name,
                                              Type type)
        {
            return (bool) typeof(IPropertyProvider)
                .GetMethod(nameof(provider.CanProvideProperty),
                           BindingFlags.Public
                           | BindingFlags.NonPublic
                           | BindingFlags.Instance)
                .MakeGenericMethod(type)
                .Invoke(provider, new object[]
                {
                    name
                });
        }

        /// <inheritdoc cref="IDynamicPropertyProvider.GetOrCreateProperty{T}"/>
        public static IReadOnlyProperty GetOrCreateProperty(this IDynamicPropertyProvider provider,
                                                            string name,
                                                            Type type)
        {
            return typeof(IDynamicPropertyProvider)
                .GetMethod(nameof(provider.GetOrCreateProperty),
                           BindingFlags.Public
                           | BindingFlags.NonPublic
                           | BindingFlags.Instance)
                .MakeGenericMethod(type)
                .Invoke(provider, new object[]
                {
                    name
                }) as IReadOnlyProperty;
        }
    }
}