using UnityEngine;

public class DustParticleEffect : MonoBehaviour
{
    [Header("Particle System Settings")]
    public ParticleSystem dustParticles;
    public float emissionRate = 50f;
    public float particleLifetime = 0.5f;
    public float particleSpeed = 2f;
    public float particleSize = 0.1f;
    
    [Header("Visual Settings")]
    public Color dustColor = new Color(0.8f, 0.6f, 0.4f, 0.7f);
    public Gradient colorOverLifetime;
    
    [Header("Movement Settings")]
    public float spreadAngle = 30f;
    public float velocityMultiplier = 1f;
    
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.ColorOverLifetimeModule colorModule;
    private ParticleSystem.SizeOverLifetimeModule sizeModule;
    
    void Awake()
    {
        SetupParticleSystem();
    }
    
    void SetupParticleSystem()
    {
        if (dustParticles == null)
        {
            // Create particle system if not assigned
            GameObject particleObj = new GameObject("DustParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            dustParticles = particleObj.AddComponent<ParticleSystem>();
        }
        
        // Configure main module
        mainModule = dustParticles.main;
        mainModule.startLifetime = particleLifetime;
        mainModule.startSpeed = particleSpeed;
        mainModule.startSize = particleSize;
        mainModule.startColor = dustColor;
        mainModule.maxParticles = 100;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
        mainModule.playOnAwake = false;
        
        // Configure emission module
        emissionModule = dustParticles.emission;
        emissionModule.rateOverTime = emissionRate;
        emissionModule.enabled = true;
        
        // Configure velocity over lifetime
        velocityModule = dustParticles.velocityOverLifetime;
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.Local;
        velocityModule.radial = new ParticleSystem.MinMaxCurve(0.5f);
        velocityModule.radialMultiplier = velocityMultiplier;
        
        // Configure color over lifetime
        colorModule = dustParticles.colorOverLifetime;
        colorModule.enabled = true;
        if (colorOverLifetime.colorKeys.Length == 0)
        {
            // Create default gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(dustColor, 0.0f), 
                    new GradientColorKey(new Color(dustColor.r, dustColor.g, dustColor.b, 0f), 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.7f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorModule.color = gradient;
        }
        else
        {
            colorModule.color = colorOverLifetime;
        }
        
        // Configure size over lifetime
        sizeModule = dustParticles.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure shape module for better dust effect
        var shapeModule = dustParticles.shape;
        shapeModule.enabled = true;
        shapeModule.shapeType = ParticleSystemShapeType.Circle;
        shapeModule.radius = 0.1f;
        shapeModule.angle = spreadAngle;
        
        // Configure noise module for more realistic dust
        var noiseModule = dustParticles.noise;
        noiseModule.enabled = true;
        noiseModule.strength = 0.5f;
        noiseModule.frequency = 1f;
        noiseModule.scrollSpeed = 0.5f;
    }
    
    public void StartDustEffect()
    {
        if (dustParticles != null && !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }
    }
    
    public void StopDustEffect()
    {
        if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }
    
    public void SetEmissionRate(float rate)
    {
        emissionRate = rate;
        if (emissionModule.enabled)
        {
            emissionModule.rateOverTime = rate;
        }
    }
    
    public void SetParticleColor(Color color)
    {
        dustColor = color;
        if (mainModule.isInitialized)
        {
            mainModule.startColor = color;
        }
    }
    
    public void SetParticleSize(float size)
    {
        particleSize = size;
        if (mainModule.isInitialized)
        {
            mainModule.startSize = size;
        }
    }
    
    public void SetParticleSpeed(float speed)
    {
        particleSpeed = speed;
        if (mainModule.isInitialized)
        {
            mainModule.startSpeed = speed;
        }
    }
    
    // Method to update particle direction based on movement
    public void UpdateParticleDirection(Vector2 movementDirection)
    {
        if (movementDirection != Vector2.zero)
        {
            // Update shape module rotation to match movement direction
            var shapeModule = dustParticles.shape;
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            shapeModule.rotation = new Vector3(0, 0, angle);
            
            // Update velocity module to add movement-based velocity
            velocityModule.linear = new ParticleSystem.MinMaxCurve(movementDirection * velocityMultiplier);
        }
    }
    
    void OnValidate()
    {
        if (dustParticles != null && Application.isPlaying)
        {
            SetupParticleSystem();
        }
    }
}