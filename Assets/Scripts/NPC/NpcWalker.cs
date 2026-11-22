using UnityEngine;

namespace NutriPick.NPC
{
    [DisallowMultipleComponent]
    public class NpcWalker : MonoBehaviour
    {
        public Vector3[] waypoints;
        public float speed = 1.1f;
        public float pauseSeconds = 1.4f;

        public NpcAnimator animator;

        private int index;
        private float pauseLeft;
        private bool waiting;

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<NpcAnimator>();

            if (waypoints != null && waypoints.Length > 0)
            {
                index = 0;
                waiting = false;
            }
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            if (waiting)
            {
                pauseLeft -= Time.deltaTime;
                if (pauseLeft <= 0f)
                {
                    waiting = false;
                    index = (index + 1) % waypoints.Length;
                }

                return;
            }

            var target = waypoints[index];
            var position = transform.position;
            var flat = new Vector3(target.x - position.x, 0f, target.z - position.z);
            if (flat.sqrMagnitude < 0.04f)
            {
                waiting = true;
                pauseLeft = pauseSeconds;
                animator?.Play("Idle");
                return;
            }

            animator?.Play("Walk");
            var direction = flat.normalized;
            transform.position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * Time.deltaTime);
        }
    }
}
