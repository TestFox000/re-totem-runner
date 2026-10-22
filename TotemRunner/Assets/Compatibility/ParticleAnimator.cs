using UnityEngine;

// Compatibility for the legacy ParticleAnimator, paired with ParticleEmitter.
[DefaultExecutionOrder(-70)]
public sealed class ParticleAnimator : MonoBehaviour
{
    public bool animateColor = true;
    [SerializeField] private Color[] m_ColorAnimation = { Color.white, Color.white, Color.white, Color.white, Color.white };
    public Vector3 worldRotationAxis;
    public Vector3 localRotationAxis;
    public float sizeGrow;
    public Vector3 rndForce;
    public Vector3 force;
    public float damping;
    public bool stopSimulation;
    public bool autodestruct;

    private ParticleEmitter emitter;
    private ParticleSystem.Particle[] buffer;
    private bool hadEmitEnabled;

    public Color[] colorAnimation
    {
        get { return m_ColorAnimation; }
        set
        {
            m_ColorAnimation = value;
            if (emitter == null)
                emitter = GetComponent<ParticleEmitter>();
            if (emitter != null)
                ApplyGradient(emitter.EnsureSystem());
        }
    }

    private void OnEnable()
    {
        emitter = GetComponent<ParticleEmitter>();
        if (emitter != null)
            ApplyGradient(emitter.EnsureSystem());
    }

    internal void ApplyGradient(ParticleSystem particles)
    {
        ParticleSystem.ColorOverLifetimeModule colors = particles.colorOverLifetime;
        colors.enabled = animateColor && m_ColorAnimation != null && m_ColorAnimation.Length != 0;
        if (!colors.enabled)
            return;
        int length = Mathf.Min(m_ColorAnimation.Length, 8);
        GradientColorKey[] rgb = new GradientColorKey[length];
        GradientAlphaKey[] alpha = new GradientAlphaKey[length];
        for (int i = 0; i < length; i++)
        {
            float time = length > 1 ? (float)i / (length - 1) : 0f;
            rgb[i] = new GradientColorKey(m_ColorAnimation[i], time);
            alpha[i] = new GradientAlphaKey(m_ColorAnimation[i].a, time);
        }
        Gradient gradient = new Gradient();
        gradient.SetKeys(rgb, alpha);
        colors.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private void LateUpdate()
    {
        if (emitter == null)
            emitter = GetComponent<ParticleEmitter>();
        // ParticleEmitter.enabled=false was a pause in Unity 3.5, including
        // particle animation. Model visibility can reactivate this GameObject
        // while its emitter remains disabled; that must not destroy the effect.
        if (emitter == null || !emitter.enabled || stopSimulation)
            return;
        ParticleSystem particles = emitter.EnsureSystem();
        int capacity = particles.main.maxParticles;
        if (buffer == null || buffer.Length < capacity)
            buffer = new ParticleSystem.Particle[capacity];
        int count = particles.GetParticles(buffer);
        float delta = Time.deltaTime;
        // Legacy damping is a per-second velocity multiplier: 1 preserves speed,
        // 0 stops particles, 2 doubles their speed in a second (Unity manual).
        float drag = Mathf.Pow(Mathf.Max(0f, damping), delta);
        for (int i = 0; i < count; i++)
        {
            Vector3 acceleration = force + new Vector3(RandomSigned(rndForce.x), RandomSigned(rndForce.y), RandomSigned(rndForce.z));
            if (!emitter.simulateInWorldspace)
                acceleration = transform.InverseTransformDirection(acceleration);
            buffer[i].velocity = (buffer[i].velocity + acceleration * delta) * drag;
            buffer[i].startSize = Mathf.Max(0f, buffer[i].startSize + sizeGrow * delta);
        }
        if (count != 0)
            particles.SetParticles(buffer, count);
        bool emissionStopped = hadEmitEnabled && !emitter.emit;
        hadEmitEnabled |= emitter.emit;
        // Legacy autodestruct is armed by actual emission, or an Emit-on to
        // Emit-off transition, never by an empty, disabled emitter.
        if (autodestruct && count == 0 && (emitter.HasEmittedParticles || emissionStopped))
            Destroy(gameObject);
    }

    private static float RandomSigned(float value)
    {
        return Random.Range(-Mathf.Abs(value), Mathf.Abs(value));
    }
}
