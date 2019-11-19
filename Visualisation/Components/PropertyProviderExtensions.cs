// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;

namespace Plugins.Narupa.Visualisation.Components
{
    public static class PropertyProviderExtensions
    {
        /// <summary>
        /// Non-generic version of <see cref="IPropertyProvider.CanProvideProperty{T}"/>
        /// </summary>
        public static bool CanProvideProperty(this IPropertyProvider provider,
                                              string name,
                                              Type type)
        {
            return (bool) typeof(IPropertyProvider)
                          .GetMethod(nameof(provider.CanProvideProperty))
                          .MakeGenericMethod(type)
                          .Invoke(provider, new object[]
                          {
                              name
                          });
        }

        public static IReadOnlyProperty GetOrCreateProperty(this IPropertyProvider provider,
                                                            string name,
                                                            Type type)
        {
            var method = typeof(IPropertyProvider).GetMethod(nameof(GetOrCreateProperty),
                                                             BindingFlags.Public
                                                           | BindingFlags.NonPublic
                                                           | BindingFlags.Instance);
            method = method?.MakeGenericMethod(type);

            if (method == null)
                throw new InvalidOperationException(
                    $"Failed to get method IPropertyProvider.GetOrCreateProperty with generic type {type}");
            return method.Invoke(provider, new object[]
            {
                name
            }) as IReadOnlyProperty;
        }
    }
}