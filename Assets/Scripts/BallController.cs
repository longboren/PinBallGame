using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Physics")]
    public float mass = 1.0f;
    public float drag = 0.0f;
    public float angularDrag = 0.05f;
    public float launchForce = 20f;
    
    [Header("Movement Controls")]
    public float moveForce = 15f;
    public float maxSpeed = 10f;
    public bool useArrowKeys = true;
    public bool useWASD = true;
    
    private Rigidbody rb;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        
        // Apply inspector settings
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    void Update()
    {
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        Vector3 movement = Vector3.zero;
        
        // WASD controls
        if (useWASD)
        {
            if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
            if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
            if (Input.GetKey(KeyCode.D)) movement += Vector3.right;
        }
        
        // Arrow key controls
        if (useArrowKeys)
        {
            if (Input.GetKey(KeyCode.UpArrow)) movement += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow)) movement += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow)) movement += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow)) movement += Vector3.right;
        }
        
        // Apply movement force
        if (movement.magnitude > 0)
        {
            movement.Normalize();
            
            // Only apply force if below max speed
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(movement * moveForce, ForceMode.Force);
            }
        }
    }

    public void Launch(float chargeAmount)
    {
        rb.AddForce(Vector3.forward * launchForce * chargeAmount, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drain"))
        {
            gameManager.BallDrained(gameObject);
        }
    }
}