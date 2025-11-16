using UnityEngine;

// Simple helper to detect drain events and inform GameManager
public class DrainDetector : MonoBehaviour
{
    public GameManager gameManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            gameManager?.BallDrained(other.gameObject);
        }
    }
}
