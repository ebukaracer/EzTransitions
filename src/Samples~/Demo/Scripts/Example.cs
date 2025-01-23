using System;
using System.Linq;
using Racer.EzTransitions.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Racer.EzTransitions.Samples
{
    internal class Example : MonoBehaviour
    {
        private TransitionManager _transitionManager;
        private SceneLoader _sceneLoader;
        private int _index = -1;

        [SerializeField] private Transition[] transitions;
        [SerializeField] private float transitionDelay = .5f;
        [SerializeField] private Text transitionTxt;
        [SerializeField] private Text loadProgressTxt;

        [Space(10),
         SerializeField,
         Tooltip("Refreshes the transitions' list on the next script compilation.")]
        private bool refreshTransitions;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!refreshTransitions) return;

            // Find all transitions from project window
            transitions = UnityEditor.AssetDatabase.FindAssets("t:Transition")
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<Transition>)
                .ToArray();

            refreshTransitions = false;
            Debug.Log("Refreshed transition list.");
        }
#endif

        private void Start()
        {
            _transitionManager = TransitionManager.Instance;
            _sceneLoader = SceneLoader.Instance;
            transitionTxt.text = transitions[0].name;

            _transitionManager.OnTransitionProgress += progress =>
            {
                if (progress == TransitionStateProgress.Done)
                    transitionTxt.text = transitions[(_index + 1) % transitions.Length].name;
            };

            _sceneLoader.OnSceneLoadProgress += SceneLoader_OnSceneLoadProgress;
        }

        private void SceneLoader_OnSceneLoadProgress(SceneLoadProgress progress)
        {
            switch (progress)
            {
                case SceneLoadProgress.Ready:
                    loadProgressTxt.text = "Scene Loading ready";
                    break;
                case SceneLoadProgress.Started:
                    loadProgressTxt.text = "Scene Loading started";
                    break;
                case SceneLoadProgress.Done:
                    loadProgressTxt.text = "Scene Loading done";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(progress), progress, null);
            }
        }

        public void NextTransition()
        {
            if (_transitionManager.IsTransitioning) return;

            _index = (_index + 1) % transitions.Length;
            _transitionManager.Transit(transitions[_index], transitionDelay);
        }

        public void LoadScene(bool useTransition)
        {
            _sceneLoader.UseTransition = useTransition;
            _sceneLoader.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            _sceneLoader.OnSceneLoadProgress -= SceneLoader_OnSceneLoadProgress;
        }
    }
}