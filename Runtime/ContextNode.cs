using Sirenix.OdinInspector;
using System;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace HierarchyContext
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ContextNode : MonoBehaviour, IContextProvider, IDestroyable
    {
        private static HierarchyContextSettings _settings => HierarchyContextSettings.Instance;

        [LabelText("Use parent Context Provider")]
        [OnInspectorGUI("@EditorGUI.LabelField($property.LastDrawnValueRect, new GUIContent(string.Empty, \"Auto fill from parent context\"))")]
        [SerializeField, OnValueChanged(nameof(UpdateContext))]
        //[ShowIf(nameof(HasParentContext))]
        private bool _autoFillFromParentContext = true;

        public bool AutoFillFromParentContext
        {
            get => _autoFillFromParentContext;
            set
            {
                if (value == _autoFillFromParentContext)
                    return;

                _autoFillFromParentContext = value;
                UpdateContext();
            }
        }

        [SerializeField, HideLabel, HideReferenceObjectPicker, PropertySpace(spaceBefore: 0, spaceAfter: 8)]
        [ShowIf(nameof(_autoFillFromParentContext))]
        private ContextProviderProperty _contextProviderProperty = new ContextProviderProperty(hideContextProperty: true);

        //[HorizontalGroup(nameof(_fragment), DisableAutomaticLabelWidth = true), PropertyOrder(0)]
        [SerializeField, OnValueChanged(nameof(UpdateContext))]
#if UNITY_EDITOR
        [ValidateInput(nameof(Editor_IsValidFragment), "$" + nameof(Editor_IsValidFragment_Result))]
#endif
        private string _fragment;

        public string Fragment
        {
            get => _fragment;
            set
            {
                if (_fragment == value)
                    return;

                var validationResult = IsValidFragment(value);
                if (validationResult.IsValid == false)
                {
                    throw new ArgumentException(validationResult.ErrorDescription);
                }

                _fragment = value;
                UpdateContext();
            }
        }

        private string _context;
        [NonSerialized]
        private bool _contextProviderPropertyInited;

        [HorizontalGroup(nameof(_fragment), DisableAutomaticLabelWidth = true), PropertyOrder(0)]
        [ShowInInspector]
        [CustomContextMenu("Reevaluate", nameof(UpdateContext))]
        public string Context
        {
            get
            {
                if (string.IsNullOrEmpty(_context))
                    UpdateContext();

                return _context;
            }
        }

        public bool IsValid => IsValidFragment(_fragment).IsValid && _settings.IsValidContext(Context);

        public event IContextProvider.ContextChangedEventHandler ContextChanged;

        public bool IsMarkedForDestruction { get; private set; }

        private void OnEnable()
        {
            InitContextProviderProperty(allowContextChangeImmediately: true);
        }

        private void OnTransformParentChanged()
        {
            _contextProviderProperty.SetParentDirty();
        }

        private void OnDestroy()
        {
            IsMarkedForDestruction = true;
            _contextProviderProperty.Destroy();
        }

        private void OnContextChanged(IContextProvider sender, string context)
        {
            UpdateContext();
        }

        private void UpdateContext()
        {
#if UNITY_EDITOR
            if (gameObject.scene == null)
                return;
#endif

            var oldContext = _context;

            InitContextProviderProperty(allowContextChangeImmediately: false);
            var parentContext = _contextProviderProperty.Context;

            var validationResult = IsValidFragment(_fragment);
            if(validationResult.IsValid == false)
            {
                Debug.LogError(validationResult.ErrorDescription, this);
                return;
            }

            var fragment = FragmentFormatter.Format(_fragment, gameObject);

            if (_autoFillFromParentContext && parentContext != null)
            {
                _context = string.Join(_settings.Separator, parentContext, fragment);
            }
            else
            {
                _context = fragment;
            }

            if (_context != oldContext)
            {
                //Debug.Log($"Context changed {_context}", this);
                ContextChanged?.Invoke(this, _context);
            }
        }

        private void InitContextProviderProperty(bool allowContextChangeImmediately)
        {
            if (_contextProviderProperty.IsInited)
                return;

            _contextProviderProperty.Init(this, OnContextChanged, allowContextChangeImmediately);
        }

        private void Reset()
        {
            _fragment = gameObject.name;
        }

        private void OnValidate()
        {
            UpdateContext();
        }

#if UNITY_EDITOR
        private bool Editor_IsValidFragment(InspectorProperty prop)
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prop.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object))
                return true;

            var result = IsValidFragment(prop.ValueEntry.WeakSmartValue as string);
            Editor_IsValidFragment_Result = result.ErrorDescription;
            return result.IsValid;
        }

        private string Editor_IsValidFragment_Result;
#endif

        private struct FragmentValidationResult
        {
            public bool IsValid;
            public string ErrorDescription;
        }

        private FragmentValidationResult IsValidFragment(string fragment)
        {
            var originalFragment = fragment;
            bool hasFormatted;
            try
            {
                hasFormatted = FragmentFormatter.Format(fragment, out fragment, gameObject);
            }
            catch (Exception e)
            {
                return new FragmentValidationResult
                {
                    IsValid = false,
                    ErrorDescription = e.Message
                };
            }

            var isValidFragment = _autoFillFromParentContext == false ? _settings.IsValidContext(fragment) : _settings.IsValidFragment(fragment);
            if (isValidFragment == false)
            {
                if (hasFormatted)
                {
                    return new FragmentValidationResult
                    {
                        IsValid = false,
                        ErrorDescription = $"Invalid fragment: \"{fragment}\" (transformed from \"{originalFragment}\")"
                    };
                }
                else
                {
                    return new FragmentValidationResult
                    {
                        IsValid = false,
                        ErrorDescription = $"Invalid fragment: \"{fragment}\""
                    };
                }
            }

            return new FragmentValidationResult
            {
                IsValid = true
            };
        }

        private bool HasParentContext => _contextProviderProperty.Context != null;
    }
}
