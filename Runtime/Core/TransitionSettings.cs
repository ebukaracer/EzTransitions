using System;
using UnityEngine;
using UnityEngine.UI;

namespace Racer.EzTransitions.Core
{
    internal enum TransitionState
    {
        In,
        Out
    }

    internal class TransitionSettings : MonoBehaviour
    {
        private static readonly int AnimID = Animator.StringToHash("Go");

        private Animator _transitionAnimatorRef;
        private GameObject _transitionGoRef;

        [HideInInspector] public Transition transition;
        [SerializeField] private Transform transitionPanel;
        [SerializeField] private CanvasScaler transitionCanvas;


        public void TransitIn()
        {
            TriggerTransition(TransitionState.In);
        }

        public void TransitOut(bool autoDestroy = true)
        {
            TriggerTransition(TransitionState.Out);
            AutoDestroy(autoDestroy);
        }

        private void Init()
        {
            if (_transitionGoRef) return;

            _transitionGoRef = Instantiate(transition.TransitionGo, transitionPanel);
            transitionCanvas.referenceResolution = transition.ReferenceResolution;
            _transitionAnimatorRef = _transitionGoRef.GetComponent<Animator>();

            if (!transition.PreserveColor)
                foreach (var image in _transitionGoRef.GetComponentsInChildren<Image>())
                    image.color = transition.OverrideColor;

            if (!Mathf.Approximately(transition.TransitionSpeed, 1))
                _transitionAnimatorRef.speed = Mathf.Sign(_transitionAnimatorRef.speed) * transition.TransitionSpeed;
        }

        private void TriggerTransition(TransitionState state)
        {
            Init();

            gameObject.SetActive(true);

            switch (state)
            {
                case TransitionState.In:
                    transition.ClipLength = _transitionAnimatorRef.runtimeAnimatorController.animationClips[0].length;
                    _transitionAnimatorRef.SetBool(AnimID, true);
                    break;
                case TransitionState.Out:
                    transition.ClipLength = _transitionAnimatorRef.runtimeAnimatorController.animationClips[1].length;
                    _transitionAnimatorRef.SetBool(AnimID, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void AutoDestroy(bool canDestroy)
        {
            if (canDestroy)
                Destroy(gameObject, transition.DestroyTime);
            else
                Invoke(nameof(Disable), transition.DestroyTime);
        }

        private void Disable() => gameObject.SetActive(false);
    }
}