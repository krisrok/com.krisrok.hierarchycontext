using UnityEditor;
using UnityEngine;
using static EditorAddons.Editor.HierarchyIcons;

namespace HierarchyContext.Editor
{
    [InitializeOnLoad]
    class ContextHierarchyDrawer : IDrawer
    {
        private static GUIStyle _normalStyle;
        private static GUIStyle _invalidStyle;

        static ContextHierarchyDrawer()
        {
            RegisterDrawer(new ContextHierarchyDrawer());
        }

        public DrawerAlignment Alignment => DrawerAlignment.AfterLabel;
        public int Priority => 10;
        public float MinWidth => 0;

        public Rect Draw(Rect rect, GameObject go)
        {
            var contextProvider = go.GetComponent<IContextProvider>();
            if (contextProvider == null)
                return rect;

            if(_normalStyle == null)
            {
                _normalStyle = new GUIStyle(EditorStyles.miniLabel);
                _normalStyle.normal.textColor = Color.grey;

                _invalidStyle = new GUIStyle(EditorStyles.miniLabel);
                _invalidStyle.normal.textColor = Color.red;
            }

            var guiContent = new GUIContent(contextProvider.Context);
            var style = contextProvider.IsValid ? _normalStyle : _invalidStyle;
            var size = style.CalcSize(guiContent);
            rect.xMax += size.x;
            GUI.Label(rect, guiContent, style);

            return rect;
        }
    }
}