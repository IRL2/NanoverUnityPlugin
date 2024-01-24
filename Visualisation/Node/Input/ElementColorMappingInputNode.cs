// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using NanoVer.Core;
using NanoVer.Core.Science;
using NanoVer.Visualisation.Properties;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Node.Input
{
    /// <summary>
    /// Input for the visualisation system that provides a <see cref="IMapping{TFrom,TTo}" /> value.
    /// </summary>
    [Serializable]
    public class ElementColorMappingInputNode : InputNode<ElementColorMappingProperty>
    {
    }
}