using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Spline
{
    public class FloatLerp : VisualisationComponent<FloatLerpNode>
    {
        public void Update()
        {
            node.Refresh();
        }
    }
}