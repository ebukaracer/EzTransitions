using UnityEngine;

namespace Racer.EzTransitions.Core
{
    /// <summary>
    /// Represents a transition instance.
    /// </summary>
    [CreateAssetMenu(fileName = "Transition", menuName = "EzTransitions/New Transition", order = 0)]
    public class Transition : ScriptableObject
    {
        public float ClipLength { get; set; } = 1;

        // @formatter:off
        [field: Header("CORE")]
        [field: SerializeField, Tooltip("Available at: /Prefabs")]
        internal TransitionSettings TransitionSettings { get; private set; }
        
        [field: SerializeField, Tooltip("The gameobject(prefab) containing the actual transition/animator component.")]
        public GameObject TransitionGo { get; internal set; }

        [field: Space(10),
         Header("SETTINGS"),
        Tooltip("The reference resolution for the transition.\nMatch it with your game's resolution.")]
        [field: SerializeField] public Vector2 ReferenceResolution { get; private set; } = new(1920, 1080);
        
        [field: SerializeField, Range(0.5f, 2f)] 
        public float TransitionSpeed { get; private set; } = 1;
        
        [field: Space(5)] 
        
        [field: SerializeField, Tooltip("Whether to use this transition's original image color or 'OverrideColor'.")]
        public bool PreserveColor { get; private set; } = true;

        [field: SerializeField, Tooltip("The color to override the transition with.\nHas no effect if 'PreserveColor' is checked.")]
        public Color OverrideColor { get; private set; } = Color.black;
        // @formatter:on

        public float TransitionTime => ClipLength / TransitionSpeed;
        public float DestroyTime => TransitionTime + .15f;
    }
}