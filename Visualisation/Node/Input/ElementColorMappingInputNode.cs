// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Nanover.Core;
using Nanover.Core.Science;
using Nanover.Visualisation.Properties;
using Nanover.Visualisation.Property;

namespace Nanover.Visualisation.Node.Input
{
    /// <summary>
    /// Input for the visualisation system that provides a <see cref="IMapping{TFrom,TTo}" /> value.
    /// </summary>
    [Serializable]
    public class ElementColorMappingInputNode : InputNode<ElementColorMappingProperty>
    {
    }
}