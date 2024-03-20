using Sirenix.OdinInspector;
using UnityEngine;

namespace HierarchyContext
{
    [ExecuteAlways]
    public abstract class ContextListenerBase : MonoBehaviour
    {
        [SerializeField, PropertySpace(0, 8)]
        private ContextProviderProperty _contextProviderProperty = new ContextProviderProperty();

        protected string Context
        {
            get
            {
                if(_contextProviderProperty.IsInited == false)
                {
                    _contextProviderProperty.Init(this, OnContextChanged, allowContextChangedImmediately: true);
                }

                return _contextProviderProperty.Context;
            }
        } 

        protected virtual void OnEnable()
        {
            if (_contextProviderProperty.IsInited)
                return;

            _contextProviderProperty.Init(this, OnContextChanged, allowContextChangedImmediately: true);
        }

        protected abstract void OnContextChanged(IContextProvider sender, string context);

        protected virtual void OnTransformParentChanged()
        {
            _contextProviderProperty.SetParentDirty();
        }

        protected virtual void OnDestroy()
        {
            _contextProviderProperty.Destroy();
        }
    }
}