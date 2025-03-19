using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStarPathfinding
{
    private Dictionary<Vector2Int, GameObject> gridObjects;

    public AStarPathfinding(Dictionary<Vector2Int, GameObject> gridObjects)
    {
        this.gridObjects = gridObjects;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        if (!gridObjects.ContainsKey(target) || IsBlocked(target))
        {
            return new List<Vector2Int>();
        }

        var openSet = new SortedSet<Node>(new NodeComparer());
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        foreach (var grid in gridObjects.Keys)
        {
            gScore[grid] = float.MaxValue;
            fScore[grid] = float.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);
        openSet.Add(new Node(start, fScore[start]));

        while (openSet.Count > 0)
        {
            var current = openSet.First().position;
            openSet.Remove(openSet.First());

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                float tentativeGScore = gScore[current] + 1; // Her adým maliyeti 1
                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);

                    if (!openSet.Any(n => n.position == neighbor))
                        openSet.Add(new Node(neighbor, fScore[neighbor]));
                }
            }
        }
        return new List<Vector2Int>(); // Yol bulunamazsa boþ liste döndür
    }



    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        var possibleNeighbors = new List<Vector2Int>
    {
        new Vector2Int(cell.x + 1, cell.y),
        new Vector2Int(cell.x - 1, cell.y),
        new Vector2Int(cell.x, cell.y + 1),
        new Vector2Int(cell.x, cell.y - 1)
    };

        List<Vector2Int> validNeighbors = possibleNeighbors
            .Where(n => gridObjects.ContainsKey(n) && !IsBlocked(n))
            .ToList();
        return validNeighbors;
    }


    private bool IsBlocked(Vector2Int gridPos)
    {
        if (!gridObjects.ContainsKey(gridPos))
        {
            return true;
        }

        Grid grid = gridObjects[gridPos].GetComponent<Grid>();
        if (grid != null)
        {
            return grid.GridCell.isBlocked;
        }

        return true;
    }


    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private class Node
    {
        public Vector2Int position;
        public float fScore;
        public Node(Vector2Int pos, float score)
        {
            position = pos;
            fScore = score;
        }
    }

    private class NodeComparer : IComparer<Node>
    {
        public int Compare(Node a, Node b)
        {
            return a.fScore.CompareTo(b.fScore);
        }
    }
}
