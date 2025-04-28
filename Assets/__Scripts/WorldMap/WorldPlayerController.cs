using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldPlayerController : MonoBehaviour
{
    // Movement variables
    public float acceleration = 5f;
    public float maxSpeed = 5f;
    public float linearDamping = 0.5f;
    public float turnSpeed = 200f;
    
    // Components and actions
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private InputAction MoveAction;
    private Vector2 _movement;

    private void Awake()
    {
        transform.position = GameSessionManager.Instance.worldPlayerPosition;
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        MoveAction.Enable();
        _rigidbody2D.linearDamping = linearDamping;
    }

    // Called once per frame
    void Update()
    {
        _movement = MoveAction.ReadValue<Vector2>();
    }

    // FixedTimestep version of Update() that's in sync w/ physics engine
    void FixedUpdate()
    {
        ApplyMovement();
        RotateTowardsMovement();
    }

    // Apply a smooth movement to the player
    void ApplyMovement()
    {
        // Allow player to control "thrust" with forward/backward inputs
        if (_movement.magnitude != 0f)
        {
            // Apply a force in direction of player
            Vector2 force = _movement.normalized * acceleration;
            _rigidbody2D.AddForce(force); // Default force mode is ForceMode2D.Force
            
            // Clamp speed
            if (_rigidbody2D.linearVelocity.magnitude > maxSpeed)
            {
                _rigidbody2D.linearVelocity = _rigidbody2D.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    // Rotate player towards the direction of movement
    void RotateTowardsMovement()
    {
        if (_rigidbody2D.linearVelocity.magnitude > 0.1f) // Only rotate if moving
        {
            float angle = Mathf.Atan2(_rigidbody2D.linearVelocity.y, _rigidbody2D.linearVelocity.x) * Mathf.Rad2Deg + 90;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
