namespace Narupa.Visualisation.Node
{
    public abstract class GenericOutputNode
    {
        protected abstract bool IsInputValid { get; }
        
        protected abstract bool IsInputDirty { get; }

        protected abstract void ClearDirty();

        protected abstract void UpdateOutput();

        protected abstract void ClearOutput();

        public void Refresh()
        {
            if (IsInputDirty)
            {
                if (IsInputValid)
                    UpdateOutput();
                else
                    ClearOutput();
                ClearDirty();
            }
        }
    }
}