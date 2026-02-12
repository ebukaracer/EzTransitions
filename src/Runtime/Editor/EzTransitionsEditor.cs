#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Racer.EzTransitions.Editor
{
    internal partial class EzTransitionsEditor : EditorWindow
    {
        private const float Dimension = 400;

        private static RemoveRequest _removeRequest;
        private static bool _isElementsImported;
        private const string PkgId = "com.racer.eztransitions";
        private const string AssetPkgId = "EzTransitions.unitypackage";

        private const string ContextMenuPath = "Racer/EzTransitions/";
        private const string RootPath = "Assets/EzTransitions";
        private const string SamplesPath = "Assets/Samples/EzTransitions";
        private const string ElementsAssetPath = RootPath + "/Elements";
        private const string SceneLoaderPrefabPath = RootPath + "/Elements/Prefabs/SceneLoader.prefab";
        private const string TransitionManagerPrefabPath = RootPath + "/Elements/Prefabs/TransitionManager.prefab";

        private const string ImportElementsContextMenuPath = ContextMenuPath + "Import Elements";
        private const string ForceImportElementsContextMenuPath = ContextMenuPath + "Import Elements(Force)";
        private const string AddSceneLoaderPrefabToScenePath = ContextMenuPath + "Add SceneLoader Prefab to Scene";

        private const string AddTransitionManagerPrefabToScenePath =
            ContextMenuPath + "Add TransitionManager Prefab to Scene";


        [MenuItem(ContextMenuPath + "Transition Creator", priority = 0)]
        private static void DisplayWindow()
        {
            var window = GetWindow<EzTransitionsEditor>("Transition Creator");
            window.maxSize = window.minSize = new Vector2(Dimension, Dimension / 2.5f);

            AutoAssignExistingDefaultTransition();
        }

        private static void ReopenWindow(string pkgName = null)
        {
            if (AssetDatabase.IsValidFolder(RootPath) && HasOpenInstances<EzTransitionsEditor>())
            {
                GetWindow<EzTransitionsEditor>().Close();
                ValidateImportElements();
                DisplayWindow();
            }

            AssetDatabase.importPackageCompleted -= ReopenWindow;
        }

        [MenuItem(ImportElementsContextMenuPath, false, priority = 1)]
        private static void ImportElements()
        {
            var packagePath = $"Packages/{PkgId}/Elements/{AssetPkgId}";

            if (File.Exists(packagePath))
            {
                AssetDatabase.ImportPackage(packagePath, true);

                if (!_isElementsImported)
                    AssetDatabase.importPackageCompleted += ReopenWindow;
            }
            else
                EditorUtility.DisplayDialog("Missing Package File", $"{AssetPkgId} not found in the package.", "OK");
        }

        [MenuItem(ImportElementsContextMenuPath, true)]
        private static bool ValidateImportElements()
        {
            _isElementsImported = AssetDatabase.IsValidFolder(ElementsAssetPath);
            return !_isElementsImported;
        }

        [MenuItem(ForceImportElementsContextMenuPath, false, priority = 2)]
        private static void ForceImportElements()
        {
            ImportElements();
        }

        [MenuItem(ForceImportElementsContextMenuPath, true)]
        private static bool ValidateForceImportElements()
        {
            return _isElementsImported;
        }

        [MenuItem(AddSceneLoaderPrefabToScenePath, false, 3)]
        private static void AddSceneLoaderPrefabToScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneLoaderPrefabPath);

            if (prefab)
            {
                var go = Instantiate(prefab);
                go.name = prefab.name;
                Undo.RegisterCreatedObjectUndo(GameObject.Find(go.name), "Add SceneLoader prefab instance to Scene");
                AddTransitionManagerPrefabToScene();
            }
            else
                EditorUtility.DisplayDialog("Missing Prefab",
                    "SceneLoader prefab not found in the package.\n\nImport this package's elements and try again",
                    "OK");
        }

        [MenuItem(AddSceneLoaderPrefabToScenePath, true)]
        private static bool ValidateAddSceneLoaderPrefabToScene()
        {
            // Validate that the prefab exists and is not already in the scene
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneLoaderPrefabPath);
            return prefab && !GameObject.Find(prefab.name);
        }

        [MenuItem(AddTransitionManagerPrefabToScenePath, false, 4)]
        private static void AddTransitionManagerPrefabToScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TransitionManagerPrefabPath);

            if (prefab)
            {
                var go = Instantiate(prefab);
                go.name = prefab.name;
                Undo.RegisterCreatedObjectUndo(GameObject.Find(go.name),
                    "Add TransitionManager prefab instance to Scene");
            }
            else
                EditorUtility.DisplayDialog("Missing Prefab",
                    "TransitionManager prefab not found in the package.\n\nImport this package's elements and try again",
                    "OK");
        }

        [MenuItem(AddTransitionManagerPrefabToScenePath, true)]
        private static bool ValidateAddTransitionManagerPrefabToScene()
        {
            // Validate that the prefab exists and is not already in the scene
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TransitionManagerPrefabPath);
            return prefab && !GameObject.Find(prefab.name);
        }

        [MenuItem(ContextMenuPath + "Remove Package(recommended)", priority = 10)]
        private static void RemovePackage()
        {
            _removeRequest = Client.Remove(PkgId);
            EditorApplication.update += RemoveProgress;
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
                    Debug.LogError($"Failed to remove package: '{PkgId}'");
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

        private static void DeleteEmptyMetaFiles(string directory)
        {
            if (Directory.Exists(directory)) return;

            var metaFile = directory + ".meta";

            if (File.Exists(metaFile))
                File.Delete(metaFile);
        }
    }

    internal static class Styles
    {
        public static readonly GUIContent DuplicateBtn =
            new("Clone",
                "Creates a copy of the existing transition with the same settings, allowing you to modify it without affecting the original.");

        public static readonly GUIContent ImportElementsBtn =
            new("Import Elements", "Imports elements required for creating custom transitions.");
    }
}
#endif