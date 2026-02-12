#if UNITY_EDITOR
using Racer.EzTransitions.Core;
using UnityEditor;

namespace Racer.EzTransitions.Editor
{
    [CustomEditor(typeof(SceneLoader))]
    internal class SceneLoaderEditor : UnityEditor.Editor
    {
        private SerializedProperty _loadTime;
        private SerializedProperty _loadDelay;
        private SerializedProperty _startupDelay;
        private SerializedProperty _delayBeforeLoad;
        private SerializedProperty _delayAfterLoad;
        private SerializedProperty _useTransition;
        private SerializedProperty _transition;

        private void OnEnable()
        {
            _loadTime = serializedObject.FindProperty("loadTime");
            _loadDelay = serializedObject.FindProperty("loadDelay");
            _startupDelay = serializedObject.FindProperty("startupDelay");
            _delayBeforeLoad = serializedObject.FindProperty("delayBeforeLoad");
            _delayAfterLoad = serializedObject.FindProperty("delayAfterLoad");
            _useTransition = serializedObject.FindProperty("useTransition");
            _transition = serializedObject.FindProperty("transition");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw fields with default inspectors
            EditorGUILayout.PropertyField(_loadTime);

            // Disable dependent fields when useTransition is true
            EditorGUI.BeginDisabledGroup(_useTransition.boolValue);
            EditorGUILayout.PropertyField(_loadDelay);
            EditorGUILayout.PropertyField(_startupDelay);
            EditorGUILayout.PropertyField(_delayBeforeLoad);
            EditorGUILayout.PropertyField(_delayAfterLoad);
            EditorGUI.EndDisabledGroup();

            if (_useTransition.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "NB: You have enabled 'Use Transition' all delays will be ignored, and depend solely on the transition used.",
                    MessageType.Info);
            }

            // Draw useTransition and transition fields
            EditorGUILayout.PropertyField(_useTransition);
            EditorGUILayout.PropertyField(_transition);

            // Show warning if useTransition is enabled but no transition assigned
            if (_useTransition.boolValue && _transition.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Warning: You have enabled 'Use Transition' but have not assigned a Transition component.",
                    MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif