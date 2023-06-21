using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HexagonGridObject : MonoBehaviour
{
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float hexagonSize = 1f;
    public float horizontalSpacing = 1.732f;
    public float verticalSpacing = 1.5f;
    public GameObject hexagonPrefab;
    public GameObject PlayerHolder;
    private GameManager gameManager;

    public List<Hexagon> GridObjects = new List<Hexagon>();

    public Dictionary<Color, List<Hexagon>> Trails = new Dictionary<Color, List<Hexagon>>();


    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>
    /// Returns the trails with the given color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public List<Hexagon> GetTrail(Color color)
    {
        if (!Trails.ContainsKey(color))
        {
            Trails[color] = new List<Hexagon>();
        }
        return Trails[color];
    }

    /// <summary>
    /// Adds a hexagon to a trail of color.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="hexagon"></param>
    public void AddTrailObject(Color color, Hexagon hexagon)
    {
        Trails[color].Add(hexagon);
    }

    /// <summary>
    /// Clears the trail of a specific color.
    /// </summary>
    /// <param name="color"></param>
    public void ClearTrail(Color color)
    {
        Trails[color].Clear();
    }

    private void Start()
    {
        GenerateHexagonGrid();
    }

    /// <summary>
    /// Gets a hexagon at a specfice locations.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hexagon GetHexagon(Vector2Int position)
    {
        return GridObjects.FirstOrDefault(hex => hex.GridCoordinates == position);
    }

    /// <summary>
    /// Returns a dictionary of color and list of hexagons representing all areas
    /// </summary>
    /// <returns></returns>
    public Dictionary<Color, List<Hexagon>> GetAreas()
    {
        Dictionary<Color, List<Hexagon>> areas = GridObjects
            .Where(hexagon => hexagon.GetColor() != Color.white)
            .GroupBy(hexagon => hexagon.GetColor())
            .ToDictionary(group => group.Key, group => group.ToList());

        return areas;
    }

    /// <summary>
    /// Spawns a GameObject on the grid and captures starting area.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="prefab"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public GameObject InitilzePrefabOnHexagonWithStartingArea(Vector2Int position, GameObject prefab, Color color)
    {
        var hex = GetHexagon(position);
        hex.SetColor(color);
        hex.SetCaptured(true);
        Vector2 hexposition = hex.transform.position;

        var neighbors = hex.GetNeighbors();
        foreach(var hexes in neighbors)
        {
            hexes.SetColor(color);
            hexes.SetCaptured(true);
        }

        return Instantiate(prefab, hexposition, Quaternion.identity);
    }

    /// <summary>
    /// Generates the hexagon grid
    /// </summary>
    private void GenerateHexagonGrid()
    {
        GridObjects.Clear();
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float xPos = x * horizontalSpacing;
                if (y % 2 == 1)
                    xPos += horizontalSpacing / 2f;
                float yPos = y * verticalSpacing;

                GameObject hexagon = Instantiate(hexagonPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
                hexagon.transform.localScale = new Vector3(hexagonSize, hexagonSize, hexagonSize);
                hexagon.transform.parent = transform;

                Hexagon hexagonComponent = hexagon.GetComponent<Hexagon>();
                if (hexagonComponent != null)
                {
                    // You can add additional logic or properties to the Hexagon component here
                    // For example, you can assign grid coordinates to each hexagon
                    hexagonComponent.SetGrid(this);
                    GridObjects.Add(hexagonComponent);
                    hexagonComponent.GridCoordinates = new Vector2Int(x, y);
                }
            }
        }
    }
    public Vector2Int GenerateRandomDirection(Color color, out Vector2Int direction, int minDistance, int maxDistance)
    {
        List<Hexagon> area = GetArea(color);

        if (area.Count == 0)
        {
            direction = Vector2Int.zero;
            return Vector2Int.zero;
        }

        // Choose a random hexagon from the area
        Hexagon randomHexagon = area[Random.Range(0, area.Count)];

        // Generate a random direction
        Vector2Int randomDirection = RandomDirection();

        // Generate a random distance within the given range
        int randomDistance = Random.Range(minDistance, maxDistance);

        // Calculate the target position based on the random direction and distance
        Vector2Int targetPosition = randomHexagon.GridCoordinates + randomDirection * randomDistance;

        // Get the closest hexagon to the target position
        Hexagon closestHexagon = GetRandomHexagonInDirection(color, randomDirection, randomDistance);

        if (closestHexagon != null)
        {
            direction = randomDirection;
            return closestHexagon.GridCoordinates;
        }
        else
        {
            direction = Vector2Int.zero;
            return Vector2Int.zero;
        }
    }

    private Vector2Int RandomDirection()
    {
        int randomIndex = Random.Range(0, 4);

        switch (randomIndex)
        {
            case 0:
                return new Vector2Int(1, 0);  // Right
            case 1:
                return new Vector2Int(-1, 0); // Left
            case 2:
                return new Vector2Int(0, 1);  // Up
            case 3:
                return new Vector2Int(0, -1); // Down
            default:
                return Vector2Int.zero;
        }
    }
    /// <summary>
    /// Untested: Gets a hexagon in the direction from the area of color with the given distance.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Hexagon GetHexagonInDirection(Color areaColor, Vector2Int direction, int distance)
    {
        List<Hexagon> area = GetArea(areaColor);
        Hexagon targetHexagon = null;

        switch (direction)
        {
            case Vector2Int right when right.x > 0: // Right direction
                targetHexagon = area.FirstOrDefault(hex => hex.GridCoordinates.x > distance);
                break;
            case Vector2Int left when left.x < 0: // Left direction
                targetHexagon = area.FirstOrDefault(hex => hex.GridCoordinates.x < -distance);
                break;
            case Vector2Int up when up.y > 0: // Up direction
                targetHexagon = area.FirstOrDefault(hex => hex.GridCoordinates.y > distance);
                break;
            case Vector2Int down when down.y < 0: // Down direction
                targetHexagon = area.FirstOrDefault(hex => hex.GridCoordinates.y < -distance);
                break;
        }

        return targetHexagon;
    }

    //remake to use struct, for more indepth scoreboard display.
    /// <summary>
    /// Returns a scoreboard string
    /// </summary>
    /// <returns></returns>
    public string GetScoreboardString()
    {
        var sb = new StringBuilder();
        var areas = GetAreas();
        var sortedAreas = areas.OrderByDescending(trail => trail.Value.Count);

        foreach (KeyValuePair<Color, List<Hexagon>> area in sortedAreas)
        {
            string colorName = ColorNameConverter.GetColorName(area.Key); // Convert Color to HTML color name
            sb.AppendLine($"[{colorName}]: {area.Value.Count}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns true when the point is inside the grid.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool ContainsPoint(Vector3 point)
    {
        foreach(var hex in GridObjects)
        {
            Collider2D collider = hex.GetComponent<Collider2D>();
            if (collider != null)
            {
                return collider.bounds.Contains(point);
            }
           
        }
        return false;
    }


    /// <summary>
    /// Returns true when a given hexagon is in any trail.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool IsHexagonInAnyTrail(Hexagon hex)
    {
        return Trails.Values.Any(trail => trail.Contains(hex));
    }


    public Hexagon GetHexagonInTrailedArea(List<Hexagon> trail)
    {
        if (trail.Count < 3)
        {
            return null; // Not enough trail points to form an area
        }

        Vector2Int targetPosition = trail[trail.Count - 1].GridCoordinates;

        for (int i = 0; i < trail.Count - 1; i++)
        {
            Hexagon pointA = trail[i];
            Hexagon pointB = trail[i + 1];
            Hexagon pointC = trail[0];

            // Skip the last segment if it is the target position
            if (i == trail.Count - 2 && pointB.GridCoordinates == targetPosition)
            {
                continue;
            }

            Vector2Int vectorA = pointA.GridCoordinates - targetPosition;
            Vector2Int vectorB = pointB.GridCoordinates - targetPosition;
            Vector2Int vectorC = pointC.GridCoordinates - targetPosition;

            float angleSum = GetSignedAngle(vectorA, vectorB) + GetSignedAngle(vectorB, vectorC);

            if (Mathf.Approximately(angleSum, 360f))
            {
                return GetHexagon(targetPosition);
            }
        }

        return null; // Target position is outside the trailed area
    }

    private float GetSignedAngle(Vector2Int vectorA, Vector2Int vectorB)
    {
        float angle = Vector2.SignedAngle(vectorA, vectorB);
        return angle < 0 ? 360f + angle : angle;
    }

    public List<Hexagon> GetArea(Color color)
    {
        return GridObjects.Where(x => x.GetColor() == color).ToList();
    }


    /// <summary>
    /// Fills the trailed of area.
    /// </summary>
    /// <param name="trailColor"></param>
    public void FillTrailArea(Color trailColor)
    {
        var area = GetArea(trailColor);
        var areaFilled = GetFilledAreaFromArea(area, trailColor);

        foreach (var hex in areaFilled)
        {
            hex.SetCaptured(true);
            hex.SetColor(trailColor);

        }

        ClearTrail(trailColor);
    }

    private void CenterCam(Color trailColor)
    {
        var area = GetArea(trailColor);
        var centerHex = GetCenterHexagon(area);
        var playerPos = gameManager.GetPlayer().transform.position;

        var targetPosition = (centerHex.transform.position + playerPos) / 2f;

        gameManager.ReSetupCam(targetPosition, area.Count);
    }


    private List<Hexagon> GetFilledAreaFromArea(List<Hexagon> area, Color trailColor)
    {
        List<Hexagon> hexes = new List<Hexagon>();

        // Group the hexagons in the area by their y-coordinate
        var groupedHexagons = area.GroupBy(hexagon => hexagon.GridCoordinates.y);

        foreach (var group in groupedHexagons)
        {
            List<Hexagon> line = new List<Hexagon>();

            // Sort the hexagons in the group based on their x-coordinate
            var sortedHexagons = group.OrderBy(hexagon => hexagon.GridCoordinates.x);

            // Find the leftmost and rightmost hexagons with the trail color
            Hexagon leftmostHexagon = null;
            Hexagon rightmostHexagon = null;

            foreach (var hexagon in sortedHexagons)
            {
                if (hexagon.GetColor() == trailColor)
                {
                    if (leftmostHexagon == null || hexagon.GridCoordinates.x < leftmostHexagon.GridCoordinates.x)
                    {
                        leftmostHexagon = hexagon;
                    }

                    if (rightmostHexagon == null || hexagon.GridCoordinates.x > rightmostHexagon.GridCoordinates.x)
                    {
                        rightmostHexagon = hexagon;
                    }
                    for (int x = leftmostHexagon.GridCoordinates.x + 1; x < rightmostHexagon.GridCoordinates.x; x++)
                    {
                        Vector2Int coordinates = new Vector2Int(x, leftmostHexagon.GridCoordinates.y);
                        Hexagon hex = GetHexagon(coordinates);
                        hexes.Add(hex);
                    }
                }
            }

            hexes.Add(leftmostHexagon);
            hexes.Add(rightmostHexagon);
            
        }

        return hexes;
    }

    public Hexagon GetClosestHexagonOfColor(Hexagon start, Color targetColor)
    {
        Hexagon closestHexagon = null;
        float closestDistance = float.MaxValue;

        foreach (Hexagon hexagon in GridObjects)
        {
            if (hexagon.GetColor() == targetColor)
            {
                float distance = Vector2.Distance(start.transform.position, hexagon.transform.position);
                if (distance < closestDistance)
                {
                    closestHexagon = hexagon;
                    closestDistance = distance;
                }
            }
        }

        return closestHexagon;
    }


    /// <summary>
    /// Gets the center hexagon from a List of hexagons.
    /// </summary>
    /// <param name="area"></param>
    /// <returns></returns>
    public Hexagon GetCenterHexagon(List<Hexagon> area)
    {
        if (area.Count == 0)
        {
            return null; // Return null if the area is empty
        }

        // Calculate the average grid coordinates of all hexagons in the area
        int sumX = 0;
        int sumY = 0;

        foreach (Hexagon hexagon in area)
        {
            sumX += hexagon.GridCoordinates.x;
            sumY += hexagon.GridCoordinates.y;
        }

        int avgX = Mathf.RoundToInt((float)sumX / area.Count);
        int avgY = Mathf.RoundToInt((float)sumY / area.Count);

        // Find the hexagon in the area with the closest grid coordinates to the average
        Hexagon centerHexagon = area.OrderBy(hex => Mathf.Abs(hex.GridCoordinates.x - avgX) + Mathf.Abs(hex.GridCoordinates.y - avgY)).First();

        return centerHexagon;
    }
  
    /// <summary>
    /// Gets a square area around a trail, used for breaking the floodfill early
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public List<Hexagon> GetTrailSquareArea(Color color)
    {
        List<Hexagon> trail = GetTrail(color);

        if (trail.Count == 0)
        {
            Debug.LogWarning("No hexagons found in the trail.");
            return new List<Hexagon>();
        }

        int minX = trail.Min(hex => hex.GridCoordinates.x);
        int maxX = trail.Max(hex => hex.GridCoordinates.x);
        int minY = trail.Min(hex => hex.GridCoordinates.y);
        int maxY = trail.Max(hex => hex.GridCoordinates.y);

        List<Hexagon> squareArea = new List<Hexagon>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Hexagon hexagon = GetHexagon(new Vector2Int(x, y));
                if (hexagon != null)
                {
                    squareArea.Add(hexagon);
                }
            }
        }

        return squareArea;
    }

    public List<Hexagon> GetSquareArea(List<Hexagon> area)
    {
        List<Hexagon> trail = area;

        if (trail.Count == 0)
        {
            Debug.LogWarning("No hexagons found in the trail.");
            return new List<Hexagon>();
        }

        int minX = trail.Min(hex => hex.GridCoordinates.x);
        int maxX = trail.Max(hex => hex.GridCoordinates.x);
        int minY = trail.Min(hex => hex.GridCoordinates.y);
        int maxY = trail.Max(hex => hex.GridCoordinates.y);

        List<Hexagon> squareArea = new List<Hexagon>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Hexagon hexagon = GetHexagon(new Vector2Int(x, y));
                if (hexagon != null)
                {
                    squareArea.Add(hexagon);
                }
            }
        }

        return squareArea;
    }

    private void LateUpdate()
    {
        //CenterCam(gameManager.GetPlayer().GetPreferences().TeamColor);
    }

}
