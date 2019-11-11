// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Node.Adaptor;

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
    }
}