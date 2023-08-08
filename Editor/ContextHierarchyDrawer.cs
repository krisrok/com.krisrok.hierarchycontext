using UnityEditor;
using UnityEngine;

namespace HierarchyContext.Editor
{
    [InitializeOnLoad]
    class ContextHierarchyDrawer
    {
        static ContextHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        static void HierarchyItemCB(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go == null)
                return;

            var contextProvider = go.GetComponent<IContextProvider>();
            if (contextProvider == null)
                return;

            Rect r = new Rect(selectionRect);
            r.xMin += 16 + EditorStyles.largeLabel.CalcSize(new GUIContent(go.name)).x;

            var guiContent = new GUIContent(contextProvider.Context);
            var style = new GUIStyle(EditorStyles.miniLabel);
            if (contextProvider.IsValid)
            {
                style.normal.textColor = Color.grey;
            }
            else
            {
                style.normal.textColor = Color.red;
            }
            GUI.Label(r, guiContent, style);
        }
    }
}