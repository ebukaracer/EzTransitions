using System;
using System.Collections;
using System.Collections.Generic;
using Racer.EzTransitions.Utilities;
using UnityEngine;

namespace Racer.EzTransitions.Core
{
    /// <summary>
    /// Enum representing the progress states of a transition.
    /// </summary>
    public enum TransitionStateProgress
    {
        InStarted,
        OutStarted,
        Done
    }

    /// <summary>
    /// Manages transitions within the application.
    /// <remarks>
    /// Ensure <see cref="TransitionManager"/> prefab or a gameobject containing this script is present in the scene before usage.
    /// </remarks>
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TransitionManager : SingletonPattern.Singleton<TransitionManager>
    {
        private float _activeOutTransitionDelay;
        private Dictionary<Transition, TransitionSettings> _transitionRefs = new();

        /// <summary>
        /// Event triggered when the transition progress changes.
        /// </summary>
        public event Action<TransitionStateProgress> OnTransitionProgress;

        /// <summary>
        /// Gets the currently active transition.
        /// </summary>
        public Transition ActiveTransition { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a transition is currently in progress.
        /// </summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>
        /// Initiates a transition.
        /// </summary>
        /// <param name="transition">The transition to be performed.</param>
        /// <param name="outTransitionDelay">The delay before the out transition starts.</param>
        public void Transit(Transition transition, float outTransitionDelay = .5f)
        {
            if (IsTransitioning) return;

            ActiveTransition = transition;
            _activeOutTransitionDelay = outTransitionDelay;

            StartCoroutine(OnTransit);
        }

        private IEnumerator OnTransit
        {
            get
            {
                if (ActiveTransition.TransitionSettings)
                {
                    var ts = Init(ActiveTransition);

                    IsTransitioning = true;

                    ts.TransitIn();
                    OnTransitionProgress?.Invoke(TransitionStateProgress.InStarted);
                    yield return Utility.GetWaitForSeconds(ActiveTransition.TransitionTime + _activeOutTransitionDelay);

                    ts.TransitOut(false);
                    OnTransitionProgress?.Invoke(TransitionStateProgress.OutStarted);

                    yield return Utility.GetWaitForSeconds(ActiveTransition.TransitionTime);
                    OnTransitionProgress?.Invoke(TransitionStateProgress.Done);

                    IsTransitioning = false;
                }
                else
                {
                    Debug.LogError($"'{nameof(TransitionSettings)}' field was not assigned in the active transition.",
                        ActiveTransition);
                }
            }
        }

        private TransitionSettings Init(Transition transition)
        {
            if (_transitionRefs.TryGetValue(transition, out var ts))
                return ts;
            if (!_transitionRefs.ContainsKey(transition))
                _transitionRefs.Add(transition, CreateTransitionSettings(transition));

            return _transitionRefs[transition];
        }

        private TransitionSettings CreateTransitionSettings(Transition transition)
        {
            var ts = Instantiate(transition.TransitionSettings.gameObject, transform)
                .GetComponent<TransitionSettings>();

            ts.transition = transition;
            return ts;
        }
    }
}