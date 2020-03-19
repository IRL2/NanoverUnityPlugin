namespace Narupa.Frontend.UI
{
    public class HoverCanvas : NarupaCanvas
    {
        protected override void RegisterCanvas()
        {
            WorldSpaceCursorInput.SetCanvasAndCursor(Canvas,
                                                     Controller.HeadPose);
        }

    }
}