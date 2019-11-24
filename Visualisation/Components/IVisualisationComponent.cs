namespace Narupa.Visualisation.Components
{
    public interface IVisualisationComponent<out TNode>
    {
        TNode Node { get; }   
    }
}