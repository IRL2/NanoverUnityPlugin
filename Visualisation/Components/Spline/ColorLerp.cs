using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Spline
{
    public class ColorLerp : VisualisationComponent<ColorLerpNode>
    {
        public void Update()
        {
            node.Refresh();
        }
    }
}