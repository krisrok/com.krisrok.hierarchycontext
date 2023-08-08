#if UNITY_EDITOR
#endif

public interface IDestroyable
{
    bool IsMarkedForDestruction { get; }
}
