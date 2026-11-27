using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 1.5f;
    public float sensitivity = 0.1f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float arrowSpeed = 90f; // degrees per second for arrow keys

    float yaw;
    float pitch = 15f;

    void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 d = mouse.delta.ReadValue();
            yaw += d.x * sensitivity;
            pitch -= d.y * sensitivity;
        }

        var kb = Keyboard.current;
        if (kb != null)
        {
            float h = 0f, v = 0f;
            if (kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.rightArrowKey.isPressed) h += 1f;
            if (kb.upArrowKey.isPressed) v += 1f;
            if (kb.downArrowKey.isPressed) v -= 1f;
            yaw += h * arrowSpeed * Time.deltaTime;
            pitch -= v * arrowSpeed * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * height;
        transform.position = focus - rot * Vector3.forward * distance;
        transform.LookAt(focus);
    }
}