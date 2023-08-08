#if UNITY_EDITOR
#endif

namespace HierarchyContext
{
    public interface IDestroyable
    {
        bool IsMarkedForDestruction { get; }
    }
}