using Navigation;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, null, null, "StringContextProvider")]
[Serializable]
public class SimpleContextProvider : IContextProvider
{
    [SerializeField, OnValueChanged(nameof(RaiseContextChanged)), ValidateInput(nameof(IsValid))]
    private string _context;

    public string Context
    {
        get => _context;
        set
        {
            if (value == _context)
                return;

            _context = value;
            RaiseContextChanged();
        }
    }

    public bool IsValid => NavigationSettings.instance.IsValidContext(_context);

    private void RaiseContextChanged()
    {
        ContextChanged?.Invoke(this, _context);
    }

    public event IContextProvider.ContextChangedEventHandler ContextChanged;
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