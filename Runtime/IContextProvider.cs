using Newtonsoft.Json;

namespace HierarchyContext
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IContextProvider
    {
        [JsonProperty("HierarchyContext")]
        string Context { get; }

        [JsonProperty]
        bool IsValid { get; }

        event ContextChangedEventHandler ContextChanged;

        public delegate void ContextChangedEventHandler(IContextProvider sender, string context);
    }
}