using UnityEngine;

namespace NutriPick.NPC
{
    [DisallowMultipleComponent]
    public class NpcAnimator : MonoBehaviour
    {
        public Animator animator;
        public string defaultState = "Idle";
        [Range(0f, 1f)] public float phaseOffset;

        private string current;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            Play(defaultState);
        }

        public void Play(string state)
        {
            if (animator == null || string.IsNullOrWhiteSpace(state))
                return;

            if (current == state)
                return;

            current = state;
            animator.Play(state, 0, Mathf.Repeat(phaseOffset, 1f));
        }
    }
}
