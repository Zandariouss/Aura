using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Stamina))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float jogSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float timeToJog = 0.5f;

    [Header("Sprint")]
    [SerializeField] private float sprintCostPerSecond = 20f;
    [SerializeField] private float minStaminaToSprint = 15f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 720f;

    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private Stamina stamina;
    private Transform cameraTransform;
    private Animator animator;
    private float verticalVelocity;
    private float heldTime;
    private bool exhausted;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = GetComponent<Stamina>();
        cameraTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float z = 0f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed) x -= 1f;

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 direction = (camForward * z + camRight * x).normalized;
        bool hasInput = direction.sqrMagnitude > 0f;

        if (hasInput)
        {
            heldTime += Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            heldTime = 0f;
        }

        if (stamina.Current <= 0f) exhausted = true;
        else if (stamina.Current >= minStaminaToSprint) exhausted = false;

        bool sprinting = keyboard.leftShiftKey.isPressed && hasInput && !exhausted;

        float speed;
        if (sprinting)
        {
            speed = sprintSpeed;
            stamina.Use(sprintCostPerSecond * Time.deltaTime);
        }
        else
        {
            speed = Mathf.Lerp(walkSpeed, jogSpeed, heldTime / timeToJog);
        }

        Vector3 move = direction * speed;

        if (animator != null)
        {
            float animSpeed = hasInput ? speed : 0f;
            animator.SetFloat("Speed", animSpeed, 0.05f, Time.deltaTime);
            animator.SetFloat("WalkMultiplier", Mathf.Clamp(speed / 2f, 1f, 2f));
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}