using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    public float spinTorque = 50f;
    public float reactionForce = 6f;
    public int scorePerHit = 10;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // keep spinning slowly
        rb.AddTorque(Vector3.up * spinTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            GameManager.Instance?.AddScore(scorePerHit);
            Rigidbody ballRb = collision.collider.attachedRigidbody;
            if (ballRb != null)
            {
                Vector3 dir = collision.contacts[0].point - transform.position;
                ballRb.AddForce(dir.normalized * reactionForce, ForceMode.Impulse);
            }
        }
    }
}