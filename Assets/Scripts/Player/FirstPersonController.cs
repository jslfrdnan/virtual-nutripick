using UnityEngine;

namespace NutriPick.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public CharacterController controller;
        public Transform cameraPivot;
        public GamepadInput input;
        public float moveSpeed = 2.0f;
        public float lookSpeed = 120f;
        public float gravity = -9.81f;

        private float pitch;
        private float verticalVelocity;
        private bool controlEnabled = true;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();
            if (input == null)
                input = GetComponent<GamepadInput>();
            if (cameraPivot == null)
            {
                var found = transform.Find("CameraPivot");
                if (found != null)
                    cameraPivot = found;
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!controlEnabled || input == null || controller == null)
                return;

            Vector3 move = transform.right * input.Move.x + transform.forward * input.Move.y;
            controller.Move(move * moveSpeed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

            float yaw = input.Look.x * lookSpeed * Time.deltaTime;
            float pitchDelta = -input.Look.y * lookSpeed * Time.deltaTime;
            transform.Rotate(0f, yaw, 0f);

            pitch = Mathf.Clamp(pitch + pitchDelta, -80f, 80f);
            if (cameraPivot != null)
                cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        public void SetControlEnabled(bool enabled)
        {
            controlEnabled = enabled;
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }
    }
}
