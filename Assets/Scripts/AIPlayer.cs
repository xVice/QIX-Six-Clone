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

    private Vector2 initialPosition;
    private Vector2 direction;
    private float currentTick = 0f;
    private float targetTicks = 25f;

    private AIData aiData;

    private Hexagon targetHex;

    private Hexagon CurrentHexagon;


    private void Awake()
    {
        aiData = GetComponent<AIData>();
        gameManager = FindObjectOfType<GameManager>();
        grid = gameManager.GetGrid();
        rb = GetComponent<Rigidbody2D>(); // Get the reference to the Rigidbody2D component
    }

    public void SetCurrentHexagon(Hexagon hex)
    {
        CurrentHexagon = hex;
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;

        GenerateRandomTargetPoint();
        MoveInDirection();
    }

    // Update is called once per frame
    void Update()
    {
        currentTick += Time.deltaTime;
        if(CurrentHexagon == targetHex)
        {
            ReturnToInitialPosition();
        }
    }

    private void MoveInDirection()
    {
        rb.velocity = direction * MoveSpeed;
    }

    private void GenerateRandomTargetPoint()
    {
        Vector2Int randomDirection;
        Vector2Int randomTargetPosition = grid.GenerateRandomDirection(aiData.TeamColor, out randomDirection, 1, 5);
        targetHex = grid.GetHexagon(randomTargetPosition);
        targetHex.SetColor(Color.cyan);

        // Set the direction based on the generated random direction
        direction = randomDirection;
    }


    private void ReturnToInitialPosition()
    {
        rb.velocity = Vector2.zero;
        direction = grid.GetClosestHexagonOfColor(targetHex, aiData.TeamColor).GridCoordinates;
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
