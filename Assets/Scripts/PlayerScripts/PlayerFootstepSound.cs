using System;
using UnityEngine;

public class PlayerFootstepSound : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AudioSource audioSource;

    [Header("Pasos por estado")]
    [SerializeField] private SoundVariation walkSteps;
    [SerializeField] private SoundVariation runSteps;
    [SerializeField] private SoundVariation crouchSteps;

    [Header("Intervalo entre pasos (segundos)")]
    [SerializeField] private float walkInterval = 0.5f;
    [SerializeField] private float runInterval = 0.3f;
    [SerializeField] private float crouchInterval = 0.8f;

    [Header("Volumen")]
    [SerializeField] private float walkVolume = 0.6f;
    [SerializeField] private float runVolume = 1f;
    [SerializeField] private float crouchVolume = 0.25f;

    [Header("Radio del ruido")]
    [SerializeField] private float walkNoiseRadius = 6f;
    [SerializeField] private float runNoiseRadius = 12f;
    [SerializeField] private float crouchNoiseRadius = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawNoiseGizmo = true;
    private float lastNoiseRadius;
    private float lastNoiseTime = -10f;


    private float stepTimer;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!player.IsMoving || !player.Grounded)
        {
            stepTimer = 0f;
            return;
        }

        float interval = walkInterval;
        float volume = walkVolume;
        float noiseRadius = walkNoiseRadius;
        SoundVariation steps = walkSteps;

        if (player.IsCrouching)
        {
            interval = crouchInterval;
            volume = crouchVolume;
            noiseRadius = crouchNoiseRadius;
            steps = crouchSteps;
        }
        else if (player.IsRunning)
        {
            interval = runInterval;
            volume = runVolume;
            noiseRadius = runNoiseRadius;
            steps = runSteps;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            steps.Play(audioSource, volume);
            EmitNoise(noiseRadius);
            stepTimer = interval;
        }
    }

    private void EmitNoise(float radius)
    {
        lastNoiseRadius = radius;
        lastNoiseTime = Time.time;
        Noise.Emit(transform.position, radius);
        Debug.Log($"Ruido emitido, radio {radius}");
    }

    private void OnDrawGizmos()
    {
        if (!drawNoiseGizmo) return;
        if (Time.time - lastNoiseTime < 1.5f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, lastNoiseRadius);
        }
    }

}