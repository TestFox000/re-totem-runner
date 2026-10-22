using UnityEngine;
using UnityEngine.Rendering;

// Compatibility for Unity 3.5 EllipsoidParticleEmitter scene components.
// Its MonoBehaviour keeps the old scene fileID, including PlayerController references.
[DefaultExecutionOrder(-80)]
public sealed class ParticleEmitter : MonoBehaviour
{
    public bool m_Emit = true;
    public float minSize = 1f;
    public float maxSize = 1f;
    public float minEnergy = 1f;
    public float maxEnergy = 1f;
    public float minEmission = 1f;
    public float maxEmission = 1f;
    public Vector3 worldVelocity;
    public Vector3 localVelocity;
    public Vector3 rndVelocity;
    public float emitterVelocityScale;
    public Vector3 tangentVelocity;
    public float angularVelocity;
    public float rndAngularVelocity;
    public bool rndRotation;
    public bool simulateInWorldspace;
    public bool m_OneShot;
    public Vector3 m_Ellipsoid = Vector3.one;
    public float m_MinEmitterRange;

    // Copied from the removed legacy ParticleRenderer on this same GameObject.
    public Material[] legacyMaterials;
    public bool legacyRendererEnabled = true;
    public bool legacyCastShadows;
    public bool legacyReceiveShadows;
    public int legacySortingLayerID;
    public int legacySortingOrder;
    public float legacyCameraVelocityScale;
    public int legacyStretchParticles;
    public float legacyLengthScale = 2f;
    public float legacyVelocityScale;
    public float legacyMaxParticleSize = 0.25f;
    public int legacyUVXTiles = 1;
    public int legacyUVYTiles = 1;
    public float legacyUVCycles = 1f;

    private ParticleSystem runtimeParticles;
    private float emissionRemainder;
    private Vector3 previousPosition;
    private Vector3 emitterVelocity;
    private bool oneShotEmitted;
    private bool hasEmittedParticles;
    private ParticleSystem.Particle[] pausedParticles;
    private int pausedParticleCount;

    internal bool HasEmittedParticles
    {
        get { return hasEmittedParticles; }
    }

    public bool emit
    {
        get { return m_Emit; }
        set { m_Emit = value; }
    }

    public int particleCount
    {
        get { return runtimeParticles != null ? runtimeParticles.particleCount : 0; }
    }

    internal ParticleSystem EnsureSystem()
    {
        if (runtimeParticles != null)
            return runtimeParticles;

        runtimeParticles = GetComponent<ParticleSystem>();
        if (runtimeParticles == null)
            runtimeParticles = gameObject.AddComponent<ParticleSystem>();
        runtimeParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = runtimeParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = Mathf.Max(1f, maxEnergy);
        main.startLifetime = new ParticleSystem.MinMaxCurve(Mathf.Max(0.01f, minEnergy), Mathf.Max(0.01f, maxEnergy));
        main.startSize = new ParticleSystem.MinMaxCurve(Mathf.Max(0f, minSize), Mathf.Max(0f, maxSize));
        main.startSpeed = 0f;
        main.startColor = Color.white;
        main.gravityModifier = 0f;
        main.simulationSpace = simulateInWorldspace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(maxEmission, 1f) * Mathf.Max(maxEnergy, 1f) * 4f) + 32, 256, 10000);

        ParticleSystem.EmissionModule emission = runtimeParticles.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = runtimeParticles.shape;
        shape.enabled = false;

        ParticleSystemRenderer renderer = runtimeParticles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            if (legacyMaterials != null && legacyMaterials.Length != 0)
                renderer.sharedMaterials = legacyMaterials;
            renderer.enabled = legacyRendererEnabled;
            renderer.shadowCastingMode = legacyCastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = legacyReceiveShadows;
            renderer.sortingLayerID = legacySortingLayerID;
            renderer.sortingOrder = legacySortingOrder;
            renderer.renderMode = legacyStretchParticles == 0 ? ParticleSystemRenderMode.Billboard : ParticleSystemRenderMode.Stretch;
            renderer.cameraVelocityScale = legacyCameraVelocityScale;
            renderer.lengthScale = legacyLengthScale;
            renderer.velocityScale = legacyVelocityScale;
            renderer.maxParticleSize = Mathf.Max(0.001f, legacyMaxParticleSize);
        }

        if (legacyUVXTiles > 1 || legacyUVYTiles > 1)
        {
            ParticleSystem.TextureSheetAnimationModule sheet = runtimeParticles.textureSheetAnimation;
            sheet.enabled = true;
            sheet.numTilesX = Mathf.Max(1, legacyUVXTiles);
            sheet.numTilesY = Mathf.Max(1, legacyUVYTiles);
            sheet.cycleCount = Mathf.Max(1, Mathf.RoundToInt(legacyUVCycles));
        }

        ParticleAnimator animator = GetComponent<ParticleAnimator>();
        if (animator != null)
            animator.ApplyGradient(runtimeParticles);
        return runtimeParticles;
    }

    private void Awake()
    {
        EnsureSystem();
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        emitterVelocity = Vector3.zero;
        emissionRemainder = 0f;
        oneShotEmitted = false;
        ParticleSystem particles = EnsureSystem();
        // Modern ParticleSystem clears itself when its GameObject is hidden.
        // Legacy disabling paused the existing particles, including model hides.
        if (pausedParticleCount > 0 && particles.particleCount == 0)
            particles.SetParticles(pausedParticles, pausedParticleCount);
        pausedParticleCount = 0;
        particles.Play(false);
    }

    private void OnDisable()
    {
        if (runtimeParticles != null)
        {
            // Legacy enabled=false pauses both emission and existing particles.
            // Stopping instead lets a reusable player effect drain and die.
            int count = runtimeParticles.particleCount;
            if (pausedParticles == null || pausedParticles.Length < count)
                pausedParticles = new ParticleSystem.Particle[count];
            pausedParticleCount = count > 0 ? runtimeParticles.GetParticles(pausedParticles) : 0;
            runtimeParticles.Pause(false);
        }
    }

    private void Update()
    {
        ParticleSystem particles = EnsureSystem();
        float delta = Time.deltaTime;
        emitterVelocity = delta > 0f ? (transform.position - previousPosition) / delta : Vector3.zero;
        previousPosition = transform.position;
        ParticleAnimator animator = GetComponent<ParticleAnimator>();
        if (animator != null && animator.stopSimulation)
        {
            particles.Pause(false);
            return;
        }
        if (particles.isPaused)
            particles.Play(false);
        if (!m_Emit || delta <= 0f)
            return;
        if (m_OneShot)
        {
            if (!oneShotEmitted)
            {
                Emit(Mathf.RoundToInt(Random.Range(minEmission, maxEmission)));
                oneShotEmitted = true;
            }
            return;
        }

        emissionRemainder += Mathf.Max(0f, Random.Range(minEmission, maxEmission)) * delta;
        int count = Mathf.Min(Mathf.FloorToInt(emissionRemainder), 1024);
        emissionRemainder -= count;
        if (count > 0)
            Emit(count);
    }

    public void Emit()
    {
        Emit(Mathf.RoundToInt(Random.Range(minEmission, maxEmission)));
    }

    public void Emit(int count)
    {
        ParticleSystem particles = EnsureSystem();
        if (!particles.isPlaying)
            particles.Play(false);
        int emittedCount = Mathf.Clamp(count, 0, 1024);
        if (emittedCount > 0)
            hasEmittedParticles = true;
        for (int i = 0; i < emittedCount; i++)
        {
            Vector3 point = Random.insideUnitSphere;
            float inner = Mathf.Clamp01(m_MinEmitterRange);
            if (point.sqrMagnitude < inner * inner)
                point = (point.sqrMagnitude > 0.00001f ? point.normalized : Vector3.up) * inner;
            point = Vector3.Scale(point, m_Ellipsoid);
            Vector3 randomVelocity = new Vector3(RandomSigned(rndVelocity.x), RandomSigned(rndVelocity.y), RandomSigned(rndVelocity.z));
            Vector3 velocity = worldVelocity + transform.TransformDirection(localVelocity + randomVelocity) + emitterVelocity * emitterVelocityScale;
            ParticleSystem.EmitParams spawn = new ParticleSystem.EmitParams();
            spawn.position = simulateInWorldspace ? transform.TransformPoint(point) : point;
            spawn.velocity = simulateInWorldspace ? velocity : transform.InverseTransformDirection(velocity);
            spawn.startLifetime = Mathf.Max(0.01f, Random.Range(minEnergy, maxEnergy));
            spawn.startSize = Mathf.Max(0f, Random.Range(minSize, maxSize));
            spawn.startColor = Color.white;
            spawn.rotation = rndRotation ? Random.Range(0f, 360f) : 0f;
            spawn.angularVelocity = angularVelocity + RandomSigned(rndAngularVelocity);
            spawn.applyShapeToPosition = false;
            particles.Emit(spawn, 1);
        }
    }

    public void Clear()
    {
        EnsureSystem().Clear(false);
    }

    private static float RandomSigned(float value)
    {
        return Random.Range(-Mathf.Abs(value), Mathf.Abs(value));
    }
}
