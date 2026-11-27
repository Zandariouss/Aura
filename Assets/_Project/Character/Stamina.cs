using System;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 15f;
    [SerializeField] private float regenDelay = 1f;

    private float current;
    private float timeSinceUse;

    public event Action<float, float> StaminaChanged;

    public float Current => current;
    public float Max => maxStamina;

    private void Awake()
    {
        current = maxStamina;
    }

    private void Update()
    {
        timeSinceUse += Time.deltaTime;

        if (timeSinceUse >= regenDelay && current < maxStamina)
        {
            Change(regenPerSecond * Time.deltaTime);
        }
    }

    public void Use(float amount)
    {
        timeSinceUse = 0f;
        Change(-amount);
    }

    private void Change(float delta)
    {
        current = Mathf.Clamp(current + delta, 0f, maxStamina);
        StaminaChanged?.Invoke(current, maxStamina);
    }
}