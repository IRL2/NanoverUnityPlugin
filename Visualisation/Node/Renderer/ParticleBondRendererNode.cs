// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Math;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using Plugins.Narupa.Visualisation.Node.Renderer;
using UnityEngine;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Visualisation node for rendering bonds between particles.
    /// </summary>
    [Serializable]
    public class ParticleBondRendererNode : IndirectMeshRenderer, IDisposable
    {
        private readonly IndirectMeshDrawCommand drawCommand = new IndirectMeshDrawCommand();

#pragma warning disable 0649
        [SerializeField]
        private MaterialProperty material = new MaterialProperty();

        [SerializeField]
        private MeshProperty mesh = new MeshProperty();

        [SerializeField]
        private IntArrayProperty particleFilter = new IntArrayProperty();
#pragma warning restore 0649

        #region Input Properties

        /// <summary>
        /// Set of bond pairs to render.
        /// </summary>
        public IProperty<BondPair[]> BondPairs => bondPairs;

        /// <summary>
        /// Positions of particles which will be connected by bonds.
        /// </summary>
        public IProperty<Vector3[]> ParticlePositions => particlePositions;

        /// <summary>
        /// Color of particles which will be connected by bonds.
        /// </summary>
        public IProperty<UnityEngine.Color[]> ParticleColors => particleColors;

        /// <summary>
        /// Scale of particles which will be connected by bonds.
        /// </summary>
        public IProperty<float[]> ParticleScales => particleScales;

        /// <summary>
        /// Overall color of the renderer. Each particle color will be multiplied by this
        /// value.
        /// </summary>
        public IProperty<UnityEngine.Color> RendererColor => rendererColor;

        /// <summary>
        /// Scaling of the particles. Each particle scale will be multiplied by this
        /// value.
        /// </summary>
        public IProperty<float> ParticleScale => particleScale;

        public IProperty<Mesh> Mesh => mesh;

        public IProperty<Material> Material => material;

        /// <summary>
        /// Scale of the bonds.
        /// </summary>
        public FloatProperty BondScale => bondScale;

        public IProperty<int[]> BondOrders => bondOrders;

        [SerializeField]
        private FloatProperty bondScale = new FloatProperty();

        [SerializeField]
        private BondArrayProperty bondPairs = new BondArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty particleScales = new FloatArrayProperty();

        [SerializeField]
        private ColorProperty rendererColor = new ColorProperty();

        [SerializeField]
        private FloatProperty particleScale = new FloatProperty();

        [SerializeField]
        private IntArrayProperty bondOrders = new IntArrayProperty();

        #endregion

        public override bool ShouldRender => mesh.HasNonNullValue()
                                          && material.HasNonNullValue()
                                          && bondPairs.HasNonEmptyValue()
                                          && particlePositions.HasNonEmptyValue()
                                          && bondScale.HasValue
                                          && rendererColor.HasValue
                                          && particleScale.HasValue;

        private int InstanceCount => particleFilter.HasNonNullValue()
                                         ? filter.Length
                                         : bondPairs.Value.Length;

        public override bool IsInputDirty => mesh.IsDirty
                                          || material.IsDirty
                                          || rendererColor.IsDirty
                                          || bondScale.IsDirty
                                          || particleScale.IsDirty
                                          || particlePositions.IsDirty
                                          || particleColors.IsDirty
                                          || particleScales.IsDirty
                                          || particleFilter.IsDirty;

        public override void UpdateInput()
        {
            UpdateMeshAndMaterials();

            drawCommand.SetFloat("_EdgeScale", bondScale.Value);
            drawCommand.SetFloat("_ParticleScale", particleScale.Value);
            drawCommand.SetFloat("_Scale", particleScale.Value);
            drawCommand.SetColor("_Color", rendererColor.Value);

            bondScale.IsDirty = false;
            particleScale.IsDirty = false;
            rendererColor.IsDirty = false;

            UpdateBuffers();

            drawCommand.SetInstanceCount(InstanceCount);
        }

        protected void UpdateBuffers()
        {
            UpdatePositionsIfDirty();
            UpdateColorsIfDirty();
            UpdateScalesIfDirty();
            UpdateBondsIfDirty();
            UpdateFilterIfDirty();
        }

        private void UpdatePositionsIfDirty()
        {
            if (particlePositions.IsDirty && particlePositions.HasNonEmptyValue())
            {
                InstancingUtility.SetPositions(drawCommand, particlePositions.Value);

                particlePositions.IsDirty = false;
            }
        }

        private void UpdateColorsIfDirty()
        {
            if (particleColors.IsDirty && particleColors.HasNonEmptyValue())
            {
                InstancingUtility.SetColors(drawCommand, particleColors.Value);

                particleColors.IsDirty = false;
            }
        }

        private void UpdateScalesIfDirty()
        {
            if (particleScales.IsDirty && particleScales.HasNonEmptyValue())
            {
                InstancingUtility.SetScales(drawCommand, particleScales.Value);

                particleScales.IsDirty = false;
            }
        }

        private void UpdateBondsIfDirty()
        {
            if (bondPairs.IsDirty && bondPairs.HasNonEmptyValue())
            {
                InstancingUtility.SetEdges(drawCommand, bondPairs.Value);

                bondPairs.IsDirty = false;
            }

            if (bondOrders.IsDirty && bondOrders.HasNonEmptyValue())
            {
                InstancingUtility.SetEdgeCounts(drawCommand, bondOrders.Value);

                bondOrders.IsDirty = false;
            }
        }

        [SerializeField]
        private bool bondToNonFiltered = false;

        /// <summary>
        /// Should this renderer draw bonds between particles that are filtered and those which are not.
        /// </summary>
        public bool BondToNonFiltered
        {
            get => bondToNonFiltered;
            set => bondToNonFiltered = value;
        }

        private int[] filter = new int[0];

        private void UpdateFilterIfDirty()
        {
            if (particleFilter.IsDirty)
            {
                if (particleFilter.HasNonEmptyValue())
                {
                    var partFilter = particleFilter.Value;

                    var maxBondCount = bondPairs.Value.Length;
                    Array.Resize(ref filter, maxBondCount);
                    var i = 0;
                    var j = 0;
                    foreach (var bond in bondPairs.Value)
                    {
                        if (bondToNonFiltered)
                        {
                            if (SearchAlgorithms.BinarySearch(bond.A, partFilter)
                             || SearchAlgorithms.BinarySearch(bond.B, partFilter))
                                filter[i++] = j;
                        }
                        else
                        {
                            if (SearchAlgorithms.BinarySearch(bond.A, partFilter)
                             && SearchAlgorithms.BinarySearch(bond.B, partFilter))
                                filter[i++] = j;
                        }

                        j++;
                    }

                    Array.Resize(ref filter, i);

                    InstancingUtility.SetFilter(drawCommand, filter);
                }
                else if (particleFilter.HasNonNullValue())
                {
                    Array.Resize(ref filter, 0);
                    InstancingUtility.SetFilter(drawCommand, filter);
                }
                else
                {
                    InstancingUtility.SetFilter(drawCommand, null);
                }

                particleFilter.IsDirty = false;
            }
        }

        private void UpdateMeshAndMaterials()
        {
            drawCommand.SetMesh(mesh);
            drawCommand.SetMaterial(material);
        }

        protected override IndirectMeshDrawCommand DrawCommand => drawCommand;

        /// <summary>
        /// Indicate that a deserialisation or other event which resets buffers has occured.
        /// </summary>
        public override void ResetBuffers()
        {
            base.ResetBuffers();
            particleColors.IsDirty = true;
            particlePositions.IsDirty = true;
            particleScales.IsDirty = true;
            bondPairs.IsDirty = true;
        }
    }
}