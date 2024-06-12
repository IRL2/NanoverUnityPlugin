using System;
using System.Collections.Generic;
using Nanover.Core.Science;
using Nanover.Visualisation.Node.Protein;
using Nanover.Visualisation.Properties.Collections;
using Nanover.Visualisation.Property;
using UnityEngine;

namespace Nanover.Visualisation.Node.Adaptor
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ExtendedSecondaryStructureAdaptorNode : SecondaryStructureAdaptorNode
    {

        public ExtendedSecondaryStructureAdaptorNode() : base()
        {
            // Currently no additional setup is required. In the future the ability to pass a
            // gradient may be added, at which point one will need to create a default gradient
            // if one is not supplied. However, for the time being this is left blank.
        }

        public IReadOnlyProperty<float[]> ResiduesNormalisedMetric =>
            GetOrCreateProperty<float[]>("residue.normalised_metric_c");
    }
}
