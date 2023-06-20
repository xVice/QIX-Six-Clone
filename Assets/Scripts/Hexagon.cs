using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hexagon : MonoBehaviour
{
    public Vector2Int GridCoordinates;

    bool IsCaptured = false;
    SpriteRenderer sp;
    HexagonGridObject grid;
    GameManager gameManager;
    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public Color GetColor()
    {
        return sp.color;
    }

    public bool GetCaptured()
    {
        return IsCaptured;
    }

    public void SetColor(Color clr)
    {
        sp.color = clr;
    }

    /// <summary>
    /// Returns a list of neighboring hexagons
    /// </summary>
    /// <returns></returns>
    public List<Hexagon> GetNeighbors()
    {
        List<Hexagon> neighbors = new List<Hexagon>();

        // Define the offsets for the neighboring hexagons based on the grid layout
        int[] dx = { 1, 1, 0, -1, 0, -1 };
        int[] dy = { 0, 1, 1, 0, -1, -1 };


        // Iterate through the offsets and calculate the coordinates of the neighboring hexagons
        for (int i = 0; i < dx.Length; i++)
        {
            int nx = GridCoordinates.x + dx[i];
            int ny = GridCoordinates.y + dy[i];

            // Retrieve the neighboring hexagon based on the calculated coordinates
            Hexagon neighbor = grid.GetHexagon(new Vector2Int(nx, ny));

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public void SetCaptured(bool captured)
    {
        IsCaptured = captured;
    }

    public void SetGrid(HexagonGridObject grid)
    {
        this.grid = grid;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(CollisionCoroutine(collision));
        }
        else if(collision.CompareTag("AI"))
        {
            StartCoroutine(CollisionCoroutineAI(collision));
        }
    }



    /// <summary>
    /// Ai trailing/hex collision logic.
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    IEnumerator CollisionCoroutineAI(Collider2D collision)
    {
        var ai = collision.gameObject.GetComponent<AIPlayer>();
        var color = ai.GetComponent<PlayerData>().TeamColor;
        var list = grid.GetTrail(color);

        if (grid.IsHexagonInAnyTrail(this))
        {
            Destroy(ai.gameObject);
            var area = grid.GetArea(color);
            foreach (var hex in area)
            {
                hex.SetCaptured(false);
                hex.SetColor(Color.white);
            }
            yield break;
        }
        else if(IsCaptured && list.Count != 0 && GetColor() == color)
        {
            grid.FillTrailArea(color);
            yield break;
        }

        if (GetColor() != color)
        {
            grid.AddTrailObject(color, this);
            SetColor(color);
            IsCaptured = true;
        }
        gameManager.UpdateScoreBoard();
        yield break;
    }

    //↑
    //Should somehow be merged into one Coroutine
    //↓

    /// <summary>
    /// Player trailing/hex collision logic.
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    IEnumerator CollisionCoroutine(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        var color = player.GetPreferences().TeamColor;
        var list = grid.GetTrail(color);

        if (collision.gameObject.CompareTag("Player"))
        {

            if (IsCaptured && list.Count != 0 && GetColor() == color)
            {

                if (grid.IsHexagonInAnyTrail(this))
                {
                    SceneManager.LoadScene(0, LoadSceneMode.Single);
                    yield break;
                }

            }

            if (GetColor() != color && !IsCaptured)
            {
                grid.AddTrailObject(color, this);
                SetColor(color);
                IsCaptured = true;
            }
            else if(grid.GetTrail(color).Count > 0)
            {
                grid.FillTrailArea(color);
                gameManager.UpdateScoreBoard();
                yield break;
            }
            gameManager.UpdateScoreBoard();
            yield break;
        }
    }

    //Old maybe slower logic, keeping it here for refrence for now.
    /*
    IEnumerator CollisionCoroutine(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        var color = player.GetPreferences().TeamColor;
        var list = grid.GetTrail(color);

        if (collision.gameObject.CompareTag("Player"))
        {
            if (IsCaptured && list.Count != 0 && GetColor() == color)
            {
                if (!list.Contains(this))
                {
                    Debug.Log("IsCaptured");
                    grid.FillTrailArea(color);
                    yield break;
                }
                Debug.Log("killing");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield break;
            }

            yield return new WaitForSeconds(0.05f);


            grid.AddTrailObject(color, this);
            SetColor(color);

            yield return new WaitForSeconds(1f);
            IsCaptured = true;
            
            yield break;
        }
    }
    */
}
