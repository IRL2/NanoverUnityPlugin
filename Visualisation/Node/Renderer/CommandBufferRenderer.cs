using System.Collections.Generic;
using Narupa.Core;
using Narupa.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Narupa.Visualisation
{
    public abstract class CommandBufferRenderer
    {
        private Dictionary<Camera, List<(CameraEvent, CommandBuffer)>> buffers =
            new Dictionary<Camera, List<(CameraEvent, CommandBuffer)>>();

        // Remove command buffers from all cameras we added into
        public virtual void Cleanup()
        {
            foreach (var (camera, buffers) in buffers)
            {
                if (camera)
                {
                    foreach (var (evnt, buffer) in buffers)
                        camera.RemoveCommandBuffer(evnt, buffer);
                }
            }

            buffers.Clear();
            foreach (var material in materials)
                Object.DestroyImmediate(material);
        }

        private List<Material> materials = new List<Material>();

        protected Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            materials.Add(material);
            return material;
        }

        public virtual void Render(Camera cam)
        {
            if (buffers.ContainsKey(cam))
                return;
            buffers[cam] = new List<(CameraEvent, CommandBuffer)>();
            foreach (var (cameraEvent, buffer) in GetBuffers(cam))
            {
                cam.AddCommandBuffer(cameraEvent, buffer);
                buffers[cam].Add((cameraEvent, buffer));
            }
        }

        protected int? AddAO(CommandBuffer buffer, Shader shader, int depthId)
        {
            if (shader == null)
                return null;

            var material = CreateMaterial(shader);

            var aoId = Shader.PropertyToID("_AmbientOcclusion");
            buffer.GetTemporaryRT(aoId, -1, -1, 0,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.ARGB32,
                                  RenderTextureReadWrite.Linear);

            buffer.SetRenderTarget(aoId);

            buffer.ClearRenderTarget(true, true, Color.white);

            buffer.SetGlobalTexture("_DepthTex", depthId);

            buffer.Blit(depthId, aoId, material);

            return aoId;
        }


        protected int ApplyShading(CommandBuffer buffer, Shader shader, RenderTargetIdentifier src, int dest)
        {
            if (shader == null)
                return dest;

            var blurId = Shader.PropertyToID("_Temp2");

            buffer.GetTemporaryRT(blurId, -2, -2, 24,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.ARGB32,
                                  RenderTextureReadWrite.Linear);

            var material = CreateMaterial(shader);

            buffer.SetGlobalTexture("_MainTex", dest);
            buffer.SetGlobalTexture("_ShadeTex", src);

            buffer.SetRenderTarget(blurId);

            buffer.Blit(dest, blurId, material);

            return blurId;
        }
        
        protected int ColorBlur(CommandBuffer buffer, Shader shader, int src, float radius)
        {
            if (shader == null)
                return src;

            var material = CreateMaterial(shader);

            var blurID = Shader.PropertyToID("_Temp1");

            const float blurDistance = 1f;

            buffer.GetTemporaryRT(blurID, -2, -2, 24,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.ARGB32,
                                  RenderTextureReadWrite.Linear);

            // horizontal blur
            buffer.SetGlobalVector("offsets",
                                   new Vector4(radius * blurDistance / Screen.width, 0, 0, 0));
            buffer.SetGlobalTexture("_MainTex", src);
            buffer.Blit(src, blurID, material);

            // vertical blur
            buffer.SetGlobalVector("offsets",
                                   new Vector4(0, radius * blurDistance / Screen.height, 0, 0));
            buffer.SetGlobalTexture("_MainTex", blurID);
            buffer.Blit(blurID, src, material);

            return src;
        }


        protected int DepthBlur(CommandBuffer buffer, Shader shader, int src, float radius)
        {
            if (shader == null)
                return src;

            var material = CreateMaterial(shader);

            var blurID = Shader.PropertyToID("_BlurTemp");

            const float blurDistance = 2f;

            buffer.GetTemporaryRT(blurID, -2, -2, 24,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.RFloat,
                                  RenderTextureReadWrite.Linear);

            // horizontal blur
            buffer.SetGlobalVector("offsets",
                                   new Vector4(radius * blurDistance / Screen.width, 0, 0, 0));
            buffer.SetGlobalTexture("_DepthTex", src);
            buffer.Blit(src, blurID, material);

            // vertical blur
            buffer.SetGlobalVector("offsets",
                                   new Vector4(0, radius * blurDistance / Screen.height, 0, 0));
            buffer.SetGlobalTexture("_DepthTex", blurID);
            buffer.Blit(blurID, src, material);

            return src;
        }

        protected abstract IEnumerable<(CameraEvent Event, CommandBuffer Buffer)> GetBuffers(Camera camera);
    }
    
    
}