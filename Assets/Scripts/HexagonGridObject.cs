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

    public List<Hexagon> GridObjects = new List<Hexagon>();

    public Dictionary<Color, List<Hexagon>> Trails = new Dictionary<Color, List<Hexagon>>();


    private void Awake()
    {

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

    /// <summary>
    /// Untested: Gets a hexagon in the direction from the area of color with the given distance.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Hexagon GetHexagonInDirectionWithDistance(Color color, Vector2 direction, float distance)
    {
        var area = GetArea(color);
        var centerHex = GetCenterHexagon(area);

        if (area == null || centerHex == null)
        {
            // Handle cases where area or centerHex are null
            return null;
        }

        Vector2 targetPosition = centerHex.GridCoordinates + (direction * distance);

        Hexagon closestHexagon = null;
        float closestDistance = float.MaxValue;

        foreach (var hexagon in area)
        {
            float distanceToTarget = Vector2.Distance(hexagon.GridCoordinates, targetPosition);

            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                closestHexagon = hexagon;
            }
        }

        return closestHexagon;
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

    //Extrem langsam, mögliche fixes die ich getestet habe:
    /// <summary>
    /// Fills the trailed of area.
    /// </summary>
    /// <param name="trailColor"></param>
    public void FillTrailArea(Color trailColor)
    {
        var trailArea = GetTrailSquareArea(trailColor);

        var subdividedarea = SubdivideTrailedArea(trailArea, 5);
        foreach(var area in subdividedarea)
        {
            var centerHex = GetCenterHexagon(area);
            var floodFilledArea = FloodFill(centerHex, trailColor, trailArea.Count());
            if(floodFilledArea != null)
            {
                foreach(var hex in floodFilledArea)
                {
                    hex.SetColor(trailColor);
                    hex.SetCaptured(true);
                }
                ClearTrail(trailColor);
                return;
            }
            ClearTrail(trailColor);
        }
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
    /// "Slices" a area into a list of areas, each area will be sized according to cubeSize 
    /// </summary>
    /// <param name="area"></param>
    /// <param name="cubeSize"></param>
    /// <returns></returns>
    public List<List<Hexagon>> SubdivideTrailedArea(List<Hexagon> area, int cubeSize)
    {
        List<List<Hexagon>> subdividedAreas = new List<List<Hexagon>>();

        int areaSize = area.Count;
        int numSubdivisions = Mathf.CeilToInt((float)areaSize / cubeSize);

        for (int i = 0; i < numSubdivisions; i++)
        {
            int startIndex = i * cubeSize;
            int endIndex = Mathf.Min(startIndex + cubeSize, areaSize);

            List<Hexagon> subdividedArea = area.GetRange(startIndex, endIndex - startIndex);
            subdividedAreas.Add(subdividedArea);
        }

        return subdividedAreas;
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

    /// <summary>
    /// A flood fill algorithm that exits when more then maxHexes get filled.
    /// </summary>
    /// <param name="startHex"></param>
    /// <param name="outlineColor"></param>
    /// <param name="maxHexes"></param>
    /// <returns></returns>
    public List<Hexagon> FloodFill(Hexagon startHex, Color outlineColor, int maxHexes)
    {
        List<Hexagon> filledHexagons = new List<Hexagon>();

        // Check if the startHex is already filled or has a different color
        if (startHex == null)
            return filledHexagons;

        // Create a queue to store the hexagons to be processed
        Queue<Hexagon> hexagonQueue = new Queue<Hexagon>();
        hexagonQueue.Enqueue(startHex);

        // Perform flood fill algorithm
        while (hexagonQueue.Count > 0)
        {
            Hexagon currentHex = hexagonQueue.Dequeue();



            // Check if the number of filled hexagons exceeds maxHexes
            if (filledHexagons.Count >= maxHexes)
                return null;

            filledHexagons.Add(currentHex);

            // Get the neighboring hexagons
            List<Hexagon> neighbors = currentHex.GetNeighbors();

            foreach (Hexagon neighbor in neighbors)
            {
                if (neighbor.GetColor() != outlineColor && !filledHexagons.Contains(neighbor) && !hexagonQueue.Contains(neighbor))
                {
                    hexagonQueue.Enqueue(neighbor);
                }
            }
        }

        return filledHexagons;
    }


}
