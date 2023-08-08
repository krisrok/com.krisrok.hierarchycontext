using HierarchyContext;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace HierarchyContext
{
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

        public bool IsValid => HierarchyContextSettings.Instance.IsValidContext(_context);

        private void RaiseContextChanged()
        {
            ContextChanged?.Invoke(this, _context);
        }

        public event IContextProvider.ContextChangedEventHandler ContextChanged;
    }
}