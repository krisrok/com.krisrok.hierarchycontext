using OdinAddons;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace Navigation
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker, HideLabel]
    public class ContextProviderProperty : IContextProvider
    {
        private static Type[] _disallowedTypes = new[] { typeof(ContextProviderProperty) };

        // Component this property lives on, providing access to the hierarchy and so on
        private Component _unityComponent;

        [OnValueChanged(nameof(UpdateContextProvider))]
        [HorizontalGroup(nameof(_contextProvider), DisableAutomaticLabelWidth = true), PropertyOrder(0)]
        [HideIf(nameof(_autoSearchContextProvider))]
        [SerializeField, HideReferenceObjectPicker]
        // Serialized value of user-specified override, we have to serialize this
        private InterfacePropertyWrapper<IContextProvider> _contextProviderOverride = new InterfacePropertyWrapper<IContextProvider>(disallowedTypes: _disallowedTypes);

        // Non-serialized, volatile value we found in parent Hierarchy
        private IContextProvider _contextProviderAuto;

        private bool _contextProviderAutoIsDirty;

        [HorizontalGroup(nameof(_contextProvider)), PropertyOrder(0)]
        [ShowIf(nameof(Editor_ShowNonUnityObject))]
        [ShowInInspector, ReadOnly, LabelText("Context Provider")]
        // Display the actually used IContextProvider if it's a non-Unity object
        private IContextProvider _contextProvider;

        public IContextProvider InnerContextProvider => _contextProvider;

        [HorizontalGroup(nameof(_contextProvider)), PropertyOrder(0)]
        [ShowIf(nameof(Editor_ShowUnityObject))]
        [ShowInInspector, ReadOnly, LabelText("Context Provider")]
        // Display the actually used IContextProvider if it's a Unity object
        private UnityEngine.Object _contextProviderUnityObject => _contextProvider as UnityEngine.Object;

        private bool Editor_ShowNonUnityObject => _autoSearchContextProvider && _contextProvider is UnityEngine.Object == false;
        private bool Editor_ShowUnityObject => _autoSearchContextProvider && _contextProvider is UnityEngine.Object;

        [HorizontalGroup(nameof(_contextProvider), Width = 16f), PropertyOrder(1), HideLabel]
        [OnInspectorGUI("@EditorGUI.LabelField($property.LastDrawnValueRect, new GUIContent(string.Empty, \"Auto search ContextProvider in parent hierarchy\"))")]
        [SerializeField, OnValueChanged(nameof(UpdateContextProvider))]
        private bool _autoSearchContextProvider = true;

        public bool AutoSearchContextProvider
        {
            get => _autoSearchContextProvider;
            set
            {
                if (_autoSearchContextProvider == value)
                    return;

                _autoSearchContextProvider = value;
                UpdateContextProvider();
            }
        }

        [HideInInspector]
        private string _context;

        [NonSerialized, HideInInspector]
        private bool _hideContextProperty;

        [NonSerialized]
        private bool _isRaisingContextChanged;

        [ShowInInspector, Indent, ReadOnly]
        [HideIf(nameof(_hideContextProperty))]
        public string Context
        {
            get
            {
                if (string.IsNullOrEmpty(_context))
                    UpdateContextProvider();

                return _contextProvider?.Context ?? null;
            }
        }

        public bool IsValid => _contextProvider?.IsValid == true;

        public event IContextProvider.ContextChangedEventHandler ContextChanged;

        [field: NonSerialized]
        public bool IsInited { get; private set; }

        public ContextProviderProperty()
        { }

        public ContextProviderProperty(bool hideContextProperty)
        {
            _hideContextProperty = hideContextProperty;
        }

        public void Init(Component unityComponent, IContextProvider.ContextChangedEventHandler contextChangedHandler = null, bool allowContextChangedImmediately = true)
        {
            if (_unityComponent != unityComponent)
            {
                _contextProviderAutoIsDirty = true;
                _unityComponent = unityComponent;
            }

            ContextChanged = null;
            if (contextChangedHandler != null)
            {
                if (allowContextChangedImmediately)
                    ContextChanged += contextChangedHandler;
            }

            IsInited = true;

            UpdateContextProvider();

            if (contextChangedHandler != null && allowContextChangedImmediately == false)
                ContextChanged += contextChangedHandler;

            _contextProviderOverride.ValueChanged -= _contextProviderOverride_ValueChanged;
            _contextProviderOverride.ValueChanged += _contextProviderOverride_ValueChanged;
        }

        private void AddContextChangedHandler(IContextProvider.ContextChangedEventHandler contextChangedHandler)
        {
            ContextChanged += contextChangedHandler;
        }

        public void Destroy()
        {
            ContextChanged = null;
            _contextProviderOverride.ValueChanged -= _contextProviderOverride_ValueChanged;
        }

        [OnInspectorInit]
        public void SetParentDirty()
        {
            _contextProviderAutoIsDirty = true;
            UpdateContextProvider();
        }

        private void _contextProviderOverride_ValueChanged(InterfacePropertyWrapper<IContextProvider> source, IContextProvider value)
        {
            UpdateContextProvider();
        }

        private void UpdateContextProvider()
        {
            var contextProvider = GetContextProvider();
            if (contextProvider != _contextProvider)
            {
                if (_contextProvider != null)
                {
                    _contextProvider.ContextChanged -= _contextProvider_ContextChanged;
                }

                var oldContext = _contextProvider?.Context;

                _contextProvider = contextProvider;

                if (oldContext != contextProvider?.Context)
                {
                    RaiseContextChanged();
                }

                if (contextProvider != null)
                {
                    contextProvider.ContextChanged += _contextProvider_ContextChanged;
                }
            }
        }

        private void _contextProvider_ContextChanged(IContextProvider sender, string context)
        {
            if (_context == context)
                return;

            _context = context;
            RaiseContextChanged();
        }

        private void RaiseContextChanged()
        {
            if (_isRaisingContextChanged)
            {
                _contextProviderAuto = null;

                if(_contextProvider != null)
                {
                    _contextProvider.ContextChanged -= _contextProvider_ContextChanged;
                    _contextProvider = null;
                }

                if (_contextProviderOverride != null)
                {
                    _contextProviderOverride.ValueChanged -= _contextProviderOverride_ValueChanged;
                }
                _contextProviderOverride = new InterfacePropertyWrapper<IContextProvider>(disallowedTypes: _disallowedTypes);

                IsInited = false;
                throw new Exception("Cycling context graph detected.");
            }

            try
            {
                _isRaisingContextChanged = true;
                ContextChanged?.Invoke(this, _context);
            }
            finally
            {
                _isRaisingContextChanged = false;
            }
        }

        private IContextProvider GetContextProvider()
        {
            if (_autoSearchContextProvider)
            {
                if (_contextProviderAuto != null && _contextProviderAutoIsDirty == false)
                    return _contextProviderAuto;

                if ((object)_unityComponent == null || _unityComponent == null)
                    return null;

                if (_unityComponent is IContextProvider && _unityComponent.transform.parent == null)
                    return null;

                _contextProviderAuto = _unityComponent.GetComponentsInParent<IContextProvider>(includeInactive: true)
                    .FirstOrDefault((IContextProvider p) => p != (object)_unityComponent && (p as IDestroyable)?.IsMarkedForDestruction == false);

                _contextProviderAutoIsDirty = false;

                return _contextProviderAuto;
            }

            _contextProviderAuto = null;
            return _contextProviderOverride?.Value;
        }
    }
}