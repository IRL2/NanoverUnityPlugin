using System.Collections.Generic;
using System.Collections.Specialized;

namespace Narupa.Grpc.Selection
{
    public class ParticleSelection : INotifyCollectionChanged
    {
        public string ID { get; }

        public IReadOnlyList<int> Selection { get; }

        public string Name { get; private set; }

        private List<int> selection = new List<int>();

        private Dictionary<string, object> properties;

        public ParticleSelection(Dictionary<string, object> obj)
        {
            ID = obj["id"] as string;
            UpdateFromObject(obj);
        }

        public void UpdateFromObject(Dictionary<string, object> obj)
        {
            Name = obj["name"] as string;
            properties = obj["properties"] as Dictionary<string, object>;
            var selectedDict = obj["selected"] as Dictionary<string, object>;
            var ids = selectedDict["particle_ids"] as List<object>;
            selection.Clear();
            foreach (var id in ids)
                selection.Add((int) id);

            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}