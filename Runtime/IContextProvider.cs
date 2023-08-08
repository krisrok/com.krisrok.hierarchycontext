public interface IContextProvider
{
    string Context { get; }
    bool IsValid { get; }

    event ContextChangedEventHandler ContextChanged;

    public delegate void ContextChangedEventHandler(IContextProvider sender, string context);
}

//[SerializeField]
//public class InterfaceWrapper<T>
//{
//    [ValidateType(typeof(IContextProvider))]
//    [SerializeField]
//    [HideLabel]
//    private UnityEngine.Object _valueObject;
//    public T Value { get; }
//}