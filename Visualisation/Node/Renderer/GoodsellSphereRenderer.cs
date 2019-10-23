using System;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using UnityEngine;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Renders goodsell style spheres, by writing the residue ID to a separate texture.
    /// </summary>
    [Serializable]
    public class GoodsellSphereRenderer : ParticleSphereRenderer
    {
        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();

        public IntArrayProperty ParticleResidues => particleResidues;

        protected override void UpdateBuffers()
        {
            base.UpdateBuffers();
            UpdateResidueIdsIfDirty();
        }
        
        private void UpdateResidueIdsIfDirty()
        {
            if (particleResidues.IsDirty && particleResidues.HasNonEmptyValue())
            {
                DrawCommand.SetDataBuffer("ResidueIds", particleResidues.Value);
                particleResidues.IsDirty = false;
            }
        }

        public override void ResetBuffers()
        {
            base.ResetBuffers();
            particleResidues.IsDirty = true;
        }
    }
}