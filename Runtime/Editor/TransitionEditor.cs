# if UNITY_EDITOR
using Racer.EzTransitions.Core;
using UnityEditor;

namespace Racer.EzTransitions.Editor
{
    [CustomEditor(typeof(Transition))]
    public class TransitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _overrideColor;

        private void OnEnable()
        {
            _overrideColor = serializedObject.FindProperty("overrideColor");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            var transitionRef = (Transition)target;

            if (!transitionRef.PreserveColor)
                EditorGUILayout.PropertyField(_overrideColor, false);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif