using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    public int scoreValue = 1;
    public float respawnTime = 3f;
    private bool hit = false;
    private ParticleSystem particles;
    private Renderer rend;
    private Collider col;

    void Start()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        
        // Create circular-disc particle system (soft round sprites)
        GameObject particleObj = new GameObject("RingParticles");
        particleObj.transform.parent = transform;
        particleObj.transform.localPosition = Vector3.zero;
        
        particles = particleObj.AddComponent<ParticleSystem>();
        
        // Renderer settings - use billboard quads with a generated circular texture
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        
        // Create a soft circular texture to make each particle a round disc
        Texture2D circleTex = CreateCircleTexture(64);
        if (renderer.material.HasProperty("_MainTex"))
            renderer.material.SetTexture("_MainTex", circleTex);
        else if (renderer.material.HasProperty("_BaseMap"))
            renderer.material.SetTexture("_BaseMap", circleTex);
        // Tint color (blue-ish as in your reference)
        Color tint = new Color(0.45f, 0.75f, 0.95f);
        if (renderer.material.HasProperty("_TintColor"))
            renderer.material.SetColor("_TintColor", tint);
        else if (renderer.material.HasProperty("_Color"))
            renderer.material.SetColor("_Color", tint);

        renderer.enableGPUInstancing = true;
        renderer.sortMode = ParticleSystemSortMode.OldestInFront;

        var main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.3f);    // visible a bit longer
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);       // outward speed
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);      // small discs
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.35f, 0.65f, 0.95f), new Color(0.6f, 0.9f, 1f)
        );
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;
        main.gravityModifier = 0.05f; // almost no fall, slight drop

        // Emission - single burst for explosion
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            // Burst at time 0, min 40 max 80 particles
            new ParticleSystem.Burst(0f, 10, 20)
        });

        // Shape - emit from a circle so particles form a ring
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.55f; // ring radius - change to tighten/loosen ring
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random; // randomized around circle
        shape.randomDirectionAmount = 0f; // direct outward from the shape

        // Velocity over lifetime - push particles radially outward a bit more and add some upward variance
        var vel = particles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        // radial velocity will push them away from the center; main.startSpeed combined with this
#if UNITY_2018_1_OR_NEWER
        // Use radial when available
        vel.radial = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
#endif
        // add a slight upward component so ring has depth
        vel.y = new ParticleSystem.MinMaxCurve(0.0f, 0.4f);

        // Size over lifetime - slightly shrink
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.35f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime - fade alpha out towards the end
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.45f, 0.75f, 0.95f), 0.0f),
                new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0.6f),
                new GradientColorKey(new Color(0.8f, 0.95f, 1f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.9f, 0.6f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Slight random rotation so discs don't all look identical
        var rot = particles.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-45f, 45f);

        // Stop until triggered
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit) return;
        if (other.CompareTag("Ball"))
        {
            hit = true;
            GameManager.Instance?.AddScore(scoreValue);
            
            // Play particle effect
            if (particles != null)
            {
                particles.transform.position = transform.position; // ensure center on target
                particles.Play();
            }
            
            // Disable renderer and collider
            if (rend) rend.enabled = false;
            if (col) col.enabled = false;
            
            // Start respawn coroutine
            StartCoroutine(RespawnAfterDelay());
        }
    }
    
    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);
        
        // Re-enable target
        if (rend) rend.enabled = true;
        if (col) col.enabled = true;
        hit = false;
    }

    // Procedurally create a soft circular texture (alpha gradient) for round discs
    Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] cols = new Color32[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int idx = x + y * size;
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / radius);
                // Create smooth falloff so edges are soft
                float alpha = Mathf.SmoothStep(1f, 0f, t);
                // small rim highlight by boosting center slightly
                float brightness = Mathf.Lerp(1.0f, 0.9f, t * 0.6f);
                byte a = (byte)(alpha * 255f);
                byte b = (byte)(brightness * 255f);
                cols[idx] = new Color32(b, b, b, a);
            }
        }
        tex.SetPixels32(cols);
        tex.Apply();
        return tex;
    }
}