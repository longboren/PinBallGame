using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bumper : MonoBehaviour
{
    public float bounceForce = 12f;
    public int scoreValue = 100;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            Rigidbody ballRb = collision.collider.attachedRigidbody;
            if (ballRb != null)
            {
                // push ball away from bumper's center
                Vector3 dir = (collision.collider.transform.position - transform.position).normalized;
                ballRb.AddForce(dir * bounceForce + Vector3.up * (bounceForce * 0.2f), ForceMode.Impulse);
            }

            GameManager.Instance?.AddScore(scoreValue);
            // optional visual/audio feedback
        }
    }
}