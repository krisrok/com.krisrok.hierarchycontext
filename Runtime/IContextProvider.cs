namespace HierarchyContext
{
    public interface IContextProvider
    {
        string Context { get; }
        bool IsValid { get; }

        event ContextChangedEventHandler ContextChanged;

        public delegate void ContextChangedEventHandler(IContextProvider sender, string context);
    }
}