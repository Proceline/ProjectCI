using UnityEditor;
using UnityEngine;

namespace IndAssets.Editor.Scripts.Debug
{
    [CustomEditor(typeof(PvDebugGridHint))]
    public class PvDebugGridHintEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Show Dial Algorithm Result"))
            {
                if (target is PvDebugGridHint gridHint)
                {
                    gridHint.ShowTargetPawnField();
                }
            }
        }
    }
}