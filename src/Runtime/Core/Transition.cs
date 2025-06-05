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
        
        [field: SerializeField, Tooltip("Whether or not to use the original transition's UI color.")]
        public bool PreserveColor { get; private set; } = true;

        [SerializeField, HideInInspector,
         Tooltip("New color for the transition UI.")]
        private Color overrideColor = Color.black;
        // @formatter:on

        public float TransitionTime => ClipLength / TransitionSpeed;
        public float DestroyTime => TransitionTime + .15f;
        public Color OverrideColor => overrideColor;
    }
}