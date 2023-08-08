#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using OdinAddons;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Navigation.Editor
{
    public class PlayWithPatternWindow : OdinEditorWindow
    {
        private bool Editor_HasContextProvider => _contextProvider.Value?.Context != null;

        [SerializeField]
        private InterfacePropertyWrapper<IContextProvider> _contextProvider = new InterfacePropertyWrapper<IContextProvider>();

        public IContextProvider ContextProvider
        {
            get => _contextProvider.Value;
            set
            {
                _contextProvider.Value = value;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            _contextProvider.ValueChanged += _contextProvider_ValueChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _contextProvider.ValueChanged -= _contextProvider_ValueChanged;
        }

        private void _contextProvider_ValueChanged(InterfacePropertyWrapper<IContextProvider> source, IContextProvider value)
        {
            if(_subscribedContextProvider != null)
            {
                _subscribedContextProvider.ContextChanged -= Value_ContextChanged;
            }

            _subscribedContextProvider = value;

            if (_subscribedContextProvider != null)
            {
                _subscribedContextProvider.ContextChanged += Value_ContextChanged;
            }

            UpdatePatternFromFormat();
        }

        private void Value_ContextChanged(IContextProvider sender, string context)
        {
            UpdatePatternFromFormat();
        }

        [ShowIf(nameof(Editor_HasContextProvider))]
        [SerializeField, OnValueChanged(nameof(UpdatePatternFromFormat))]
        private string _patternFormat;

        public string PatternFormat
        {
            get => _patternFormat;
            set
            {
                _patternFormat = value;
                UpdatePatternFromFormat();
            }
        }

        private void UpdatePatternFromFormat()
        {
            if(string.IsNullOrEmpty(_patternFormat) || _contextProvider.Value?.Context == null)
                 return;

            Pattern = Formatter.Format(_patternFormat, _contextProvider.Value?.Context);
            
            Repaint();
        }

        [DisableIf(nameof(Editor_HasContextProvider))]
        [SerializeField, OnValueChanged(nameof(PatternChanged))]
        private string _pattern;

        public string Pattern
        {
            get => _pattern;
            set
            {
                _pattern = value;
                PatternChanged(value);
            }
        }

        private void PatternChanged(string value)
        {
            UpdateAll();
        }

        public InputThing[] Inputs = new[] { new InputThing() };
        private IContextProvider _subscribedContextProvider;

        [Serializable]
        public class InputThing
        {
            [ShowInInspector]
            [OnValueChanged(nameof(UpdateIsMatchByProperty), false)]
            public string Input { get; set; }

            [PropertySpace]
            [ShowInInspector]
            [ReadOnly]
            public bool IsMatch { get; private set; }

            private void UpdateIsMatchByProperty(InspectorProperty prop)
            {
                var pattern = (prop.Parent.Parent.Parent.ValueEntry.WeakSmartValue as PlayWithPatternWindow).Pattern;
                UpdateIsMatch(pattern);
            }

            public void UpdateIsMatch(string Pattern)
            {
                IsMatch = false;
                if (string.IsNullOrEmpty(Input) || string.IsNullOrEmpty(Pattern))
                {
                    return;
                }
                try
                {
                    IsMatch = Regex.Match(Input, Pattern).Success;
                }
                catch
                {
                }
            }
        }

        public void UpdateAll()
        {
            foreach (var i in Inputs)
            {
                i.UpdateIsMatch(Pattern);
            }
        }
    }
}
#endif