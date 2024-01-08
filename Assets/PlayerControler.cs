using UnityEngine;

public class PlayerController : MonoBehaviour
    {
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
        {
        // Get the Rigidbody component for physics-based movement
        rb = GetComponent<Rigidbody>();

        // If there's no Rigidbody attached to the GameObject, add one
        if (rb == null)
            {
            rb = gameObject.AddComponent<Rigidbody>();
            }
        }

    void Update()
        {
        // Get input from keyboard
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Create movement vector
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Apply the movement to the Rigidbody
        rb.velocity = movement * moveSpeed;
        }
    }
