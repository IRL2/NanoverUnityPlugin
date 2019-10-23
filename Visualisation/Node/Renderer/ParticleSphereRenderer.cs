// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Visualisation node for rendering particles using spheres.
    /// </summary>
    [Serializable]
    public class ParticleSphereRenderer : IDisposable
    {
        private readonly IndirectMeshDrawCommand drawCommand = new IndirectMeshDrawCommand();

        /// <summary>
        /// Positions of particles.
        /// </summary>
        public IProperty<Vector3[]> ParticlePositions => particlePositions;

        /// <summary>
        /// Color of particles.
        /// </summary>
        public IProperty<UnityEngine.Color[]> ParticleColors => particleColors;

        /// <summary>
        /// Scale of particles.
        /// </summary>
        public IProperty<float[]> ParticleScales => particleScales;

        /// <summary>
        /// Overall color of the renderer. Each particle color will be multiplied by this
        /// value.
        /// </summary>
        public IProperty<UnityEngine.Color> RendererColor => rendererColor;

        /// <summary>
        /// Overall scaling of the renderer. Each particle scale will be multiplied by this
        /// value.
        /// </summary>
        public IProperty<float> RendererScale => rendererScale;

        /// <summary>
        /// Filtered indices for particles.
        /// </summary>
        public IProperty<int[]> ParticleFilter => particleFilter;

        public IProperty<Mesh> Mesh => mesh;

        public IProperty<Material> Material => material;

#pragma warning disable 0649
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty particleScales = new FloatArrayProperty();

        [SerializeField]
        private ColorProperty rendererColor = new ColorProperty();

        [SerializeField]
        private FloatProperty rendererScale = new FloatProperty();

        [SerializeField]
        private IntArrayProperty particleFilter = new IntArrayProperty();

        [SerializeField]
        private MeshProperty mesh = new MeshProperty();

        [SerializeField]
        private MaterialProperty material = new MaterialProperty();
#pragma warning restore 0649

        public Transform Transform { get; set; }

        public bool ShouldRender => mesh.HasNonNullValue()
                                 && material.HasNonNullValue()
                                 && particlePositions.HasNonEmptyValue()
                                 && rendererColor.HasValue
                                 && rendererScale.HasValue
                                 && Transform != null
                                 && InstanceCount > 0;

        private int InstanceCount => particleFilter.HasNonNullValue()
                                         ? particleFilter.Value.Length
                                         : particlePositions.Value.Length;

        public virtual bool IsInputDirty => mesh.IsDirty
                                         || material.IsDirty
                                         || rendererColor.IsDirty
                                         || rendererScale.IsDirty
                                         || particlePositions.IsDirty
                                         || particleColors.IsDirty
                                         || particleScales.IsDirty
                                         || particleFilter.IsDirty;

        /// <summary>
        /// Render the provided bonds
        /// </summary>
        public void Render(Camera camera = null)
        {
            if (UpdateRenderer())
            {
                drawCommand.MarkForRenderingThisFrame(camera);
            }
        }

        public bool UpdateRenderer()
        {
            if (ShouldRender)
            {
                if (IsInputDirty)
                {
                    UpdateMeshAndMaterials();

                    drawCommand.SetInstanceCount(InstanceCount);
                    drawCommand.SetFloat("_Scale", rendererScale.Value);
                    drawCommand.SetColor("_Color", rendererColor.Value);
                    rendererScale.IsDirty = false;
                    rendererColor.IsDirty = false;

                    UpdateBuffers();
                }
                
                InstancingUtility.SetTransform(drawCommand, Transform);

                return true;
            }

            drawCommand.SetInstanceCount(0);
            return false;
        }

        protected virtual void UpdateBuffers()
        {
            UpdatePositionsIfDirty();
            UpdateColorsIfDirty();
            UpdateScalesIfDirty();
            UpdateFilterIfDirty();
        }

        protected IndirectMeshDrawCommand DrawCommand => drawCommand;

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            drawCommand.Dispose();
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

        private void UpdateFilterIfDirty()
        {
            if (particleFilter.IsDirty && particleFilter.HasNonEmptyValue())
            {
                InstancingUtility.SetFilter(drawCommand, particleFilter.Value);

                particleFilter.IsDirty = false;
            }
        }

        private void UpdateMeshAndMaterials()
        {
            drawCommand.SetMesh(mesh);
            drawCommand.SetMaterial(material);
            mesh.IsDirty = false;
            material.IsDirty = false;
        }

        /// <summary>
        /// Indicate that a deserialisation or other event which resets buffers has occured.
        /// </summary>
        public virtual void ResetBuffers()
        {
            drawCommand.ResetCommand();
            particleColors.IsDirty = true;
            particlePositions.IsDirty = true;
            particleScales.IsDirty = true;
            particleFilter.IsDirty = true;
        }
        
        public void AddToCommandBuffer(CommandBuffer buffer)
        {
            UpdateRenderer();
            drawCommand.AddToCommandBuffer(buffer);
        }
    }
}