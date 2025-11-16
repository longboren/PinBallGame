using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinTarget : MonoBehaviour
{
    [Header("Spin")]
    public float spinSpeed = 90f; // degrees per second

    [Header("Hit Feedback")]
    public float hitScale = 1.3f;
    public float hitDuration = 0.18f;

    Vector3 baseScale;
    bool isAnimating = false;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Rotate around local Z so a flat coin spins visually when lying flat
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !isAnimating)
        {
            StartCoroutine(HitPulse());
        }
    }

    IEnumerator HitPulse()
    {
        isAnimating = true;
        float t = 0f;
        Vector3 targetScale = baseScale * hitScale;

        // scale up quickly then back
        while (t < hitDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Sin((t / hitDuration) * Mathf.PI); // smooth in-out
            transform.localScale = Vector3.Lerp(baseScale, targetScale, lerp);
            yield return null;
        }

        transform.localScale = baseScale;
        isAnimating = false;
    }
}
