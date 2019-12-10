using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Visualisation
{
    [ExecuteAlways]
    public class BoxVisualiser : MonoBehaviour
    {
        [SerializeField]
        private AffineTransformation box;

        [Range(0, 0.2f)]
        [SerializeField]
        private float width = 0.1f;

        [SerializeField]
        private Mesh mesh;

        [SerializeField]
        private Material material;

        private readonly Vector3[] axesMidpoints =
        {
            new Vector3(0.5f, 0, 0), 
            new Vector3(0.5f, 1, 0),
            new Vector3(0.5f, 0, 1), 
            new Vector3(0.5f, 1, 1),
            new Vector3(0, 0.5f, 0), 
            new Vector3(1, 0.5f, 0),
            new Vector3(0, 0.5f, 1), 
            new Vector3(1, 0.5f, 1),
            new Vector3(0, 0, 0.5f), 
            new Vector3(1, 0, 0.5f),
            new Vector3(0, 1, 0.5f), 
            new Vector3(1, 1, 0.5f)
        };

        private readonly Vector3[] axesLength =
        {
            Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.up, Vector3.up,
            Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.forward
        };

        private void Update()
        {
            for (var i = 0; i < 12; i++)
            {
                var offset = LinearTransformation.Dot(axesMidpoints[i], box.LinearTransformation) + box.origin;
                var size = Vector3.Scale(axesLength[i], box.AxesMagnitudes);
                var x = box.xAxis.normalized;
                var y = box.yAxis.normalized;
                var z = box.zAxis.normalized;
                var transformation = new AffineTransformation(x * (size.x + width),
                                                              y * (size.y + width),
                                                              z * (size.z + width),
                                                              offset);
                var matrix = transformation.AsMatrix();
                Graphics.DrawMesh(mesh, transform.localToWorldMatrix * matrix, material, 0);
            }
        }

        public void SetBox(AffineTransformation transformation)
        {
            this.box = transformation;
        }
    }
}