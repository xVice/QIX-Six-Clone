using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    GameManager gameManager;
    HexagonGridObject grid;
    Rigidbody2D rb; // Reference to the AI player's Rigidbody2D

    public float MoveSpeed = 5f;
    public float GizmoSize = 0.2f;
    public Color GizmoColor = Color.red;

    private List<Vector2> trailPoints;
    private Vector2 initialPosition;
    private Vector2 direction;
    private int turnCount = 0;
    private float currentTick = 0f;
    private float targetTicks = 25f;

    private bool shouldTurnRight; // Flag to determine whether to turn right or left

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        grid = gameManager.GetGrid();
        rb = GetComponent<Rigidbody2D>(); // Get the reference to the Rigidbody2D component
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        trailPoints = new List<Vector2>();
        GenerateRandomDirection();
        MoveInDirection();
    }

    // Update is called once per frame
    void Update()
    {
        currentTick += Time.deltaTime;
        if (currentTick >= targetTicks)
        {
            currentTick = 0;
            GenerateRandomDirection();
            MoveInDirection();
        }
    }

    private void MoveInDirection()
    {
        rb.velocity = direction * MoveSpeed;
    }

    private void GenerateRandomDirection()
    {
        float turnAngle = 45f;
        if (turnCount >= 3)
        {
            ReturnToInitialPosition();
            return;
        }

        shouldTurnRight = Random.value < 0.5f; // Randomly choose whether to turn right or left

        if (shouldTurnRight)
            direction = Quaternion.Euler(0f, 0f, turnAngle) * direction;
        else
            direction = Quaternion.Euler(0f, 0f, -turnAngle) * direction;

        turnCount++;
    }

    private void ReturnToInitialPosition()
    {
        rb.velocity = Vector2.zero;
        transform.position = initialPosition;
        trailPoints.Clear();
        turnCount = 0;
    }

    private bool IsOutOfBounds()
    {
        return !grid.ContainsPoint(transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(direction, GizmoSize);
    }
}
