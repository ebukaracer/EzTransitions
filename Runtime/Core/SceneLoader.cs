using System;
using System.Collections;
using Racer.EzTransitions.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.EzTransitions.Core
{
    /// <summary>
    /// Singleton class that manages asynchronous scene loading with optional transition effects.
    /// <remarks>
    /// Ensure <see cref="SceneLoader"/> prefab or a gameobject containing this script is present in the scene before usage.
    /// </remarks>
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SceneLoader : SingletonPattern.SingletonPersistent<SceneLoader>
    {
        // Prevents loading more than once at a time.
        private bool _isLoading;

        private TransitionSettings _transitionSettingsRef;

        public event Action<SceneLoadProgress> OnSceneLoadProgress;
        public event Action<TransitionStateProgress> OnTransitionStateProgress;

        // @formatter:off
        [Header("LOADING")] 
        [Tooltip("Time to spend while loading.")] 
        [SerializeField] private float loadTime = 1f;

        [Tooltip("Delay before and after loading.")] 
        [SerializeField] private float loadDelay = .5f;

        [Tooltip("Whether or not to start loading immediately.")]
        [SerializeField] private float startupDelay = 1f;

        [Tooltip("Whether or not to start loading as soon as startup is complete.\nNB: Depends on loadDelay")]
        [SerializeField] private bool delayBeforeLoad = true;

        [Tooltip("Whether or not to activate scene immediately loading is complete.\nNB: Depends on loadDelay")]
        [SerializeField] private bool delayAfterLoad = true;

        [Space(5)]
        [SerializeField]
        [Header("TRANSITION")]
        [Tooltip("Whether or not to use transition for scene loading.")]
        private bool useTransition;
        // @formatter:on

        public bool UseTransition
        {
            get => useTransition;
            set
            {
                if (!_isLoading)
                    useTransition = value;
            }
        }

        [SerializeField] private Transition transition;


        private void Start()
        {
            if (useTransition && !transition)
            {
                Debug.LogError(
                    $"'Use Transition' was set to true, but '{nameof(Transition)}' field is null.", this);
            }
        }

        /// <summary>
        /// Loads into next scene asynchronously by name.
        /// </summary>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode = default)
        {
            LoadSceneAsync(SceneManager.GetSceneByName(sceneName).buildIndex, mode);
        }

        /// <summary>
        /// Loads into next scene asynchronously by build index.
        /// </summary>
        public void LoadSceneAsync(int buildIndex, LoadSceneMode mode = default)
        {
            if (_isLoading)
                return;

            StartCoroutine(LoadScene(buildIndex, mode));
        }

        private IEnumerator LoadScene(int buildIndex, LoadSceneMode loadSceneMode = default)
        {
            if (buildIndex == -1)
            {
                Debug.LogError("Scene with such name or build index was not found.", this);
                yield break;
            }

            _isLoading = true;

            OnSceneLoadProgress?.Invoke(SceneLoadProgress.Ready);

            // Initialization delay.
            yield return Utility.GetWaitForSeconds(useTransition ? 0 : startupDelay);

            // Transition
            if (useTransition && transition.TransitionSettings)
            {
                _transitionSettingsRef = Instantiate(transition.TransitionSettings.gameObject, transform)
                    .GetComponent<TransitionSettings>();

                _transitionSettingsRef.transition = transition;

                _transitionSettingsRef.TransitIn();
                OnTransitionStateProgress?.Invoke(TransitionStateProgress.InStarted);

                yield return Utility.GetWaitForSeconds(transition.TransitionTime);
            }
            else if (useTransition)
            {
                Debug.LogError($"'{nameof(TransitionSettings)}' field was not assigned in the active transition.",
                    transition);
                _isLoading = false;
                yield break;
            }

            if (delayBeforeLoad)
                yield return Utility.GetWaitForSeconds(useTransition ? 0 : loadDelay);

            var scene = SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);
            scene.allowSceneActivation = false;

            OnSceneLoadProgress?.Invoke(SceneLoadProgress.Started);

            do
                yield return Utility.GetWaitForSeconds(loadTime);
            while
                (scene.progress < .9f);

            if (!(scene.progress >= .9f)) yield break;

            OnSceneLoadProgress?.Invoke(SceneLoadProgress.Done);

            if (delayAfterLoad)
                yield return Utility.GetWaitForSeconds(useTransition ? 0 : loadDelay);

            scene.allowSceneActivation = true;

            if (_transitionSettingsRef)
            {
                _transitionSettingsRef.TransitOut();
                OnTransitionStateProgress?.Invoke(TransitionStateProgress.OutStarted);

                yield return Utility.GetWaitForSeconds(transition.TransitionTime);

                OnTransitionStateProgress?.Invoke(TransitionStateProgress.Done);
            }

            _isLoading = false;
        }
    }
}