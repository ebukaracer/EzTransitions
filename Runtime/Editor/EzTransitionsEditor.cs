#if UNITY_EDITOR
using System;
using System.IO;
using Racer.EzTransitions.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Racer.EzTransitions.Editor
{
    internal class EzTransitionsEditor : EditorWindow
    {
        private const float Dimension = 400;

        private static RemoveRequest _removeRequest;
        private static bool _isElementsImported;
        private const string PkgId = "com.racer.eztransitions";

        private const string ContextMenuPath = "Racer/EzTransitions/";
        private const string RootPath = "Assets/EzTransitions";
        private const string SamplesPath = "Assets/Samples/EzTransitions";
        private static readonly string SourcePath = $"Packages/{PkgId}/Elements";
        private const string ElementsPath = RootPath + "/Elements";

        private const string TransitionBasePath = ElementsPath + "/Transitions";
        private const string TemplateFolderPath = ElementsPath + "/Transitions/SimpleFade";
        private string _transitionName = "NewTransition";


        [MenuItem(ContextMenuPath + "Transition Creator", priority = 0)]
        private static void DisplayWindow()
        {
            var window = GetWindow<EzTransitionsEditor>("Transition Creator");
            window.maxSize = window.minSize = new Vector2(Dimension, Dimension / 2.5f);
        }

        [MenuItem(ContextMenuPath + "Import Elements", false, priority = 1)]
        private static void ImportElements()
        {
            if (Directory.Exists(ElementsPath))
            {
                Debug.Log(
                    $"Root directory already exists: '{ElementsPath}'" +
                    "\nIf you would like to re-import, remove and reinstall this package.");
                return;
            }

            if (!Directory.Exists(SourcePath))
            {
                Debug.LogError(
                    "Source path is missing. Please ensure this package is installed correctly," +
                    $" otherwise reinstall it.\nNonexistent Path: {SourcePath}");
                return;
            }

            try
            {
                DirUtils.CreateDirectory(RootPath);
                Directory.Move(SourcePath, ElementsPath);
                DirUtils.DeleteEmptyMetaFiles(SourcePath);
                AssetDatabase.Refresh();
                _isElementsImported = AssetDatabase.IsValidFolder(ElementsPath);
                Debug.Log($"Imported successfully at '{ElementsPath}'");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"An error occurred while importing this package's elements: {e.Message}\n{e.StackTrace}");
            }
        }

        [MenuItem(ContextMenuPath + "Import Elements", true, priority = 1)]
        private static bool ValidateImportElements()
        {
            _isElementsImported = AssetDatabase.IsValidFolder(ElementsPath);
            return !_isElementsImported;
        }

        [MenuItem(ContextMenuPath + "Remove Package(recommended)", priority = 2)]
        private static void RemovePackage()
        {
            _removeRequest = Client.Remove(PkgId);
            EditorApplication.update += RemoveProgress;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!_isElementsImported);
            _transitionName = EditorGUILayout.TextField("Transition Name", _transitionName);

            GUILayout.Space(5);

            if (GUILayout.Button(Styles.CreateBtn))
            {
                try
                {
                    CreateTransition();
                }
                catch (Exception e)
                {
                    Debug.LogError($"An error occurred while creating the transition: {e.Message}\n{e.StackTrace}");
                }
            }

            GUILayout.Space(5);
            EditorGUI.EndDisabledGroup();

            if (_isElementsImported)
            {
                EditorGUILayout.HelpBox(
                    "A new transition directory, based on the 'SimpleFade' transition, will be created. " +
                    "Feel free to customize the animations within it to match your desired style.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Some of this package's elements are not available." +
                    "\nClick the button below to fully import them.",
                    MessageType.Warning);
            }

            GUILayout.Space(5);

            if (!_isElementsImported && GUILayout.Button(Styles.ImportElementsBtn))
                ImportElements();
        }

        private void CreateTransition()
        {
            if (string.IsNullOrWhiteSpace(_transitionName))
            {
                Debug.LogError("Transition name cannot be empty.");
                return;
            }

            var newTransitionPath = Path.Combine(TransitionBasePath, _transitionName);

            // Check if the folder already exists
            if (AssetDatabase.IsValidFolder(newTransitionPath))
            {
                Debug.LogError($"A transition with the name '{_transitionName}' already exists.");
                return;
            }

            // Create the transition folder
            AssetDatabase.CreateFolder(TransitionBasePath, _transitionName);

            // Duplicate the template assets
            string[] filesToDuplicate =
            {
                "SimpleFade.asset",
                "SimpleFade.controller",
                "SimpleFade_In.anim",
                "SimpleFade_Out.anim",
                "SimpleFade.prefab"
            };

            var duplicatedPaths = new string[filesToDuplicate.Length];

            for (var i = 0; i < filesToDuplicate.Length; i++)
            {
                var sourcePath = Path.Combine(TemplateFolderPath, filesToDuplicate[i]);
                var destPath = Path.Combine(newTransitionPath, _transitionName);

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
                        "Operation failed! Ensure the elements for this package are imported and available." +
                        $"\nThe file '{sourcePath}' was also not found.");

                    DirUtils.DeleteDirectory(TransitionBasePath);
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
                var inState = FindOrCreateState(newController, "SimpleFade_In");
                var outState = FindOrCreateState(newController, "SimpleFade_Out");

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
                $"Transition '{_transitionName}' created successfully. Feel free to customize the animations to your taste.");
        }

        private static AnimatorState FindOrCreateState(AnimatorController controller, string stateName)
        {
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                        return state.state;
                }
            }

            // If not found, create a new state
            var newState = controller.layers[0].stateMachine.AddState(stateName);
            return newState;
        }


        private static void RemoveProgress()
        {
            if (!_removeRequest.IsCompleted) return;

            switch (_removeRequest.Status)
            {
                case StatusCode.Success:
                {
                    DirUtils.DeleteDirectory(RootPath);
                    DirUtils.DeleteDirectory(SamplesPath);
                    AssetDatabase.Refresh();

                    break;
                }
                case >= StatusCode.Failure:
                    Debug.LogWarning($"Failed to remove package: '{PkgId}'");
                    break;
            }

            EditorApplication.update -= RemoveProgress;
        }
    }

    internal static class DirUtils
    {
        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            Directory.Delete(path, true);
            DeleteEmptyMetaFiles(path);
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void MoveMetaFile(string source, string destination)
        {
            if (!File.Exists(source + ".meta")) return;

            File.Move(source + ".meta", destination + ".meta");
        }

        public static void DeleteEmptyMetaFiles(string directory)
        {
            if (Directory.Exists(directory)) return;

            var metaFile = directory + ".meta";

            if (File.Exists(metaFile))
                File.Delete(metaFile);
        }
    }

    internal static class Styles
    {
        public static readonly GUIContent CreateBtn =
            new("Create Transition", "Creates a new transition.");

        public static readonly GUIContent ImportElementsBtn =
            new("Import Elements", "Imports elements required for creating custom transitions.");
    }
}
#endif