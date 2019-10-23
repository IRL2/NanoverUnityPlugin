using System;
using System.Collections.Generic;
using Narupa.Visualisation.Property;
using UnityEngine;
using UnityEngine.Rendering;

namespace Narupa.Visualisation.Node.Renderer
{
    [ExecuteAlways]
    [Serializable]
    public class GoodsellRenderer : CommandBufferRenderer
    {
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        public IProperty<Vector3[]> ParticlePositions => particlePositions;

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();
        
        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();

        
        [SerializeField]
        private IntArrayProperty particleFilter = new IntArrayProperty();

        public Transform Transform { get; set; }

        public override void Cleanup()
        {
            base.Cleanup();
            renderer.ResetBuffers();
        }

        public void Setup()
        {
            renderer.ParticlePositions.LinkedProperty = particlePositions;
            renderer.ParticleColors.LinkedProperty = particleColors;
            renderer.ParticleResidues.LinkedProperty = particleResidues;
            renderer.ParticleFilter.LinkedProperty = particleFilter;
        }

        [SerializeField]
        private CameraEvent cameraEvent = CameraEvent.AfterForwardOpaque;

        [SerializeField]
        private Shader shader;

        [SerializeField]
        private GoodsellSphereRenderer renderer = new GoodsellSphereRenderer();

        private bool isRendering = false;
        
        public override void Render(Camera cam)
        {
            if (renderer.IsInputDirty && renderer.ShouldRender != isRendering)
            {
                Cleanup();
                isRendering = renderer.ShouldRender;
            }
            renderer.UpdateRenderer();
            base.Render(cam);
        }

        protected override IEnumerable<(CameraEvent Event, CommandBuffer Buffer)> GetBuffers(Camera camera)
        {
            if (GetColorBuffer(camera) is CommandBuffer colorBuffer)
                yield return (cameraEvent, colorBuffer);
        }

        private CommandBuffer GetColorBuffer(Camera camera)
        {
            var buffer = new CommandBuffer
            {
                name = "Goodsell Pass"
            };

            if (shader == null)
                return null;

            var material = CreateMaterial(shader);

            renderer.Transform = Transform;

            var colorId = Shader.PropertyToID("_ColorTexture");
            var depthId = Shader.PropertyToID("_DepthTexture");
            var resId = Shader.PropertyToID("_ResidueTexture");

            buffer.GetTemporaryRT(colorId, -1, -1, 24,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.ARGB32,
                                  RenderTextureReadWrite.Linear);
            buffer.GetTemporaryRT(depthId, -1, -1, 0,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.RFloat,
                                  RenderTextureReadWrite.Linear);
            buffer.GetTemporaryRT(resId, -1, -1, 0,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.RFloat,
                                  RenderTextureReadWrite.Linear);

            buffer.SetRenderTarget(new RenderTargetIdentifier[]
            {
                colorId, depthId, resId
            }, colorId);

            buffer.ClearRenderTarget(true, true, UnityEngine.Color.clear);

            renderer.AddToCommandBuffer(buffer);

            buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            buffer.SetGlobalTexture("_DepthTex", depthId);
            buffer.SetGlobalTexture("_MainTex", colorId);
            buffer.SetGlobalTexture("_ResidueTex", resId);

            buffer.Blit(colorId, BuiltinRenderTextureType.CameraTarget, material);

            buffer.ReleaseTemporaryRT(colorId);
            buffer.ReleaseTemporaryRT(depthId);

            return buffer;
        }


       

    }
}