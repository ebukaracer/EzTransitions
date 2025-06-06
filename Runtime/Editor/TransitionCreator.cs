#if UNITY_EDITOR
using System;
using System.IO;
using Racer.EzTransitions.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Racer.EzTransitions.Editor
{
    internal partial class EzTransitionsEditor
    {
        private static Transition _assignedTransition;
        private const string DefaultTransitionName = "SimpleFade";
        private const string TransitionBasePath = ElementsAssetPath + "/Transitions";
        private string _transitionNameField = "NewTransition";


        private void OnGUI()
        {
            GUILayout.Space(10);

            _assignedTransition =
                (Transition)EditorGUILayout.ObjectField("Existing Transition", _assignedTransition, typeof(Transition),
                    false);

            _transitionNameField = EditorGUILayout.TextField("Transition Name", _transitionNameField);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!_isElementsImported);

            if (GUILayout.Button(Styles.DuplicateBtn))
            {
                try
                {
                    CreateFromExistingTransition();
                }
                catch (Exception e)
                {
                    Debug.LogError($"An error occurred while creating the transition: {e.Message}\n{e.StackTrace}");
                }
            }

            EditorGUI.EndDisabledGroup();

            switch (_isElementsImported)
            {
                case true when _assignedTransition:
                    EditorGUILayout.HelpBox(
                        $"A new transition directory, based upon '{_assignedTransition.name}' transition will be created. " +
                        "Feel free to customize the animations within it to match your desired style.",
                        MessageType.Info);
                    break;
                case true when !_assignedTransition:
                    EditorGUILayout.HelpBox("Please assign an existing transition first.", MessageType.Warning);
                    break;
                case false:
                {
                    EditorGUILayout.HelpBox(
                        "Some of this package's elements are not available." +
                        "\nClick the button below to fully import them.",
                        MessageType.Warning);

                    GUILayout.Space(5);

                    if (GUILayout.Button(Styles.ImportElementsBtn))
                        ImportElements();
                    break;
                }
            }
            
            // Detect mouse click outside controls and remove focus
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                GUI.FocusControl(null);
        }

        private static void AutoAssignExistingDefaultTransition()
        {
            _assignedTransition =
                AssetDatabase.LoadAssetAtPath<Transition>(
                    $"{TransitionBasePath}/{DefaultTransitionName}/{DefaultTransitionName}.asset");
        }

        private void CreateFromExistingTransition()
        {
            var assignedTransitionName = _assignedTransition?.name;

            if (string.IsNullOrWhiteSpace(assignedTransitionName))
            {
                Debug.LogError("The existing transition field cannot be null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_transitionNameField))
            {
                Debug.LogError("The new transition name field cannot be empty.");
                return;
            }


            var newTransitionPath = Path.Combine(TransitionBasePath, _transitionNameField);

            // Check if the folder already exists
            if (AssetDatabase.IsValidFolder(newTransitionPath))
            {
                Debug.LogError(
                    $"A transition with the name '{_transitionNameField}' already exists, delete the directory and try again.");
                return;
            }

            // Create the transition folder
            AssetDatabase.CreateFolder(TransitionBasePath, _transitionNameField);

            // Duplicate the template assets
            string[] filesToDuplicate =
            {
                $"{assignedTransitionName}.asset",
                $"{assignedTransitionName}.controller",
                $"{assignedTransitionName}_In.anim",
                $"{assignedTransitionName}_Out.anim",
                $"{assignedTransitionName}.prefab"
            };

            var duplicatedPaths = new string[filesToDuplicate.Length];

            for (var i = 0; i < filesToDuplicate.Length; i++)
            {
                var sourcePath = Path.Combine($"{TransitionBasePath}/{assignedTransitionName}", filesToDuplicate[i]);
                var destPath = Path.Combine(newTransitionPath, _transitionNameField);

                switch (i)
                {
                    case 2:
                        destPath += "_In";
                        break;
                    case 3:
                        destPath += "_Out";
                        break;
                }

                destPath += Path.GetExtension(filesToDuplicate[i]);

                if (File.Exists(sourcePath))
                {
                    if (AssetDatabase.CopyAsset(sourcePath, destPath))
                        duplicatedPaths[i] = destPath;
                    else
                        Debug.LogWarning($"Skipped some files!.\nSource: {sourcePath}, Destination: {destPath}");
                }
                else
                {
                    Debug.LogError(
                        $"Operation failed! Something is missing.\nEither the 'Elements' folder or the path: '{sourcePath}' for the existing transition is missing.");

                    DirUtils.DeleteDirectory(newTransitionPath);
                    AssetDatabase.Refresh();

                    return;
                }
            }

            // Transition ScriptableObject
            var newTransition = AssetDatabase.LoadAssetAtPath<Transition>(duplicatedPaths[0]);
            // Animator Controller
            var newController = AssetDatabase.LoadAssetAtPath<AnimatorController>(duplicatedPaths[1]);
            // In Animation
            var newInClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(duplicatedPaths[2]);
            // Out Animation
            var newOutClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(duplicatedPaths[3]);
            // Prefab
            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(duplicatedPaths[4]);

            // Ensure both animations are assigned in the Animator Controller
            if (newController)
            {
                var inState = InitOrCreateState(newController, $"{assignedTransitionName}_In");
                var outState = InitOrCreateState(newController, $"{assignedTransitionName}_Out");

                if (inState)
                {
                    inState.motion = newInClip;
                    inState.name = newInClip.name;
                }

                if (outState)
                {
                    outState.motion = newOutClip;
                    outState.name = newOutClip.name;
                }
            }

            // Assign the Animator Controller to the prefab
            if (newPrefab)
            {
                var animator = newPrefab.GetComponent<Animator>();

                if (!animator)
                    animator = newPrefab.AddComponent<Animator>();

                animator.runtimeAnimatorController = newController;

                // Update the prefab
                PrefabUtility.SaveAsPrefabAsset(newPrefab, duplicatedPaths[4]);
            }

            // Update the transition gameobject reference
            if (newTransition)
                newTransition.TransitionGo = newPrefab;

            // Refresh and focus on the created folder
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(newTransitionPath);

            Debug.Log(
                $"Transition '{_transitionNameField}' created successfully. Feel free to customize the animations to your taste.",
                newTransition);
        }

        private static AnimatorState InitOrCreateState(AnimatorController controller, string stateName)
        {
            foreach (var layer in controller.layers)
            {
                if (layer.stateMachine.states == null) continue;

                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                        return state.state;
                }
            }

            // If not found, create a new state
            return controller.layers[0].stateMachine.AddState(stateName);
        }
    }
}
#endif