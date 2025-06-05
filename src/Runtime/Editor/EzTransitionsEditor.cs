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

        private const string ImportElementsContextMenuPath = ContextMenuPath + "Import Elements";
        private const string ForceImportElementsContextMenuPath = ContextMenuPath + "Import Elements(Force)";


        [MenuItem(ContextMenuPath + "Transition Creator", priority = 0)]
        private static void DisplayWindow()
        {
            var window = GetWindow<EzTransitionsEditor>("Transition Creator");
            window.maxSize = window.minSize = new Vector2(Dimension, Dimension / 2.5f);

            AutoAssignExistingDefaultTransition();
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

        [MenuItem(ForceImportElementsContextMenuPath, false, priority = 1)]
        private static void ForceImportElements()
        {
            ImportElements();
        }

        [MenuItem(ImportElementsContextMenuPath, true, priority = 1)]
        private static bool ValidateImportElements()
        {
            _isElementsImported = AssetDatabase.IsValidFolder(ElementsAssetPath);
            return !_isElementsImported;
        }

        [MenuItem(ForceImportElementsContextMenuPath, true, priority = 1)]
        private static bool ValidateForceImportElements()
        {
            return _isElementsImported;
        }

        [MenuItem(ContextMenuPath + "Remove Package(recommended)", priority = 2)]
        private static void RemovePackage()
        {
            _removeRequest = Client.Remove(PkgId);
            EditorApplication.update += RemoveProgress;
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
            new("Create from Existing?", "Creates a new transition based upon an existing one.");

        public static readonly GUIContent ImportElementsBtn =
            new("Import Elements", "Imports elements required for creating custom transitions.");
    }
}
#endif