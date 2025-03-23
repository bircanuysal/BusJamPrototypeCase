using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class StickmanAPathfinding : MonoBehaviour
{
    AStarPathfinding pathfinding;
    private Dictionary<Vector2Int, GameObject> gridObjects;
    private Vector2Int currentGridPos;
    private Outline outline;
    private Coroutine moveOnTheCoroutine;

    private void Start()
    {
        pathfinding = new AStarPathfinding(LevelDesigner.Instance.GetGridManager().GetGridObjects());
        gridObjects = LevelDesigner.Instance.GetGridManager().GetGridObjects();
        currentGridPos = GetGridPosition(transform.position);
        outline = GetComponent<Outline>();
        ControlOutline();
    }
    private void OnEnable()
    {
        EventManager.StickmanEvents.ControlOutline += ControlOutline;
    }
    private void OnDisable()
    {
        DOTween.Kill(gameObject);
        EventManager.StickmanEvents.ControlOutline -= ControlOutline;
    }
    private void OnMouseDown()
    {
        if (GameManager.Instance.GetCanClickOnStickman())
        {
            Move();
        }
        
    }

    public void Move()
    {
        if (IsOnLastRow())
        {
            SetGridBlocked(currentGridPos, false);
            EventManager.StickmanEvents.TargetReached(GetComponent<Unit>().UnitID);
            return;
        }

        Vector2Int targetGridPos = GetLowestCostGridOnLastRow();


        if (targetGridPos == currentGridPos || targetGridPos == Vector2Int.zero)
        {
            return;
        }

        bool targetBlocked = IsBlocked(targetGridPos);

        if (targetBlocked)
        {
            return;
        }

        List<Vector2Int> path = pathfinding.FindPath(currentGridPos, targetGridPos);

        if (path.Count > 0)
        {
            moveOnTheCoroutine = StartCoroutine(MoveOnPath(transform, path));
        }
    }
    private IEnumerator MoveOnPath(Transform stickman, List<Vector2Int> path)
    {
        EventManager.StickmanEvents.Moving(GetComponent<Unit>().UnitID);
        EventManager.StickmanEvents.StickmanControlOutline();
        SetGridBlocked(currentGridPos, false);


        foreach (var gridPos in path)
        {
            Vector3 targetPos = gridObjects[gridPos].transform.position;
            Vector3 dir = (targetPos - transform.position).normalized;
            if (transform != null)
            {
                transform.DORotateQuaternion(Quaternion.LookRotation(dir), .25f);
            }            
            while (Vector3.Distance(stickman.position, targetPos) > 0.1f)
            {
                stickman.position = Vector3.MoveTowards(stickman.position, targetPos, Time.deltaTime * 9f);

                //stickman.LookAt(targetPos);
                yield return null;
            }
            currentGridPos = gridPos;
        }
        EventManager.StickmanEvents.TargetReached(GetComponent<Unit>().UnitID);

        //SetGridBlocked(currentGridPos, true);
    }
    public Coroutine GetMoveCoroutine()
    {
        return moveOnTheCoroutine;
    }
    public void StopMoveCourutine()
    {
        if (moveOnTheCoroutine != null)
        {
            StopCoroutine(moveOnTheCoroutine);
        }
    }
    private Vector2Int GetGridPosition(Vector3 worldPos)
    {
        foreach (var kvp in gridObjects)
        {
            if (Vector3.Distance(kvp.Value.transform.position, worldPos) < 0.5f)
            {
                return kvp.Key;
            }
        }
        return Vector2Int.zero;
    }
    private bool IsOnLastRow()
    {
        int maxZ = gridObjects.Keys.Max(pos => pos.y);
        return currentGridPos.y == maxZ;
    }
    private Vector2Int GetLowestCostGridOnLastRow()
    {
        int maxZ = gridObjects.Keys.Max(pos => pos.y);

        List<Vector2Int> availableGrids = gridObjects.Keys
            .Where(pos => pos.y == maxZ && !IsBlocked(pos))
            .ToList();

        if (availableGrids.Count == 0)
        {
            Debug.LogError("Son satýrda uygun grid bulunamadý!");
            return Vector2Int.zero;
        }

        Vector2Int bestTarget = Vector2Int.zero;
        int minPathCost = int.MaxValue;

        foreach (var target in availableGrids)
        {
            List<Vector2Int> path = pathfinding.FindPath(currentGridPos, target);
            int pathCost = path.Count;

            if (pathCost > 0 && pathCost < minPathCost)
            {
                minPathCost = pathCost;
                bestTarget = target;
            }
        }

        if (bestTarget == Vector2Int.zero)
        {
            Debug.LogError("Hiçbir uygun maliyetli grid bulunamadý!");
        }
        return bestTarget;
    }

    private bool IsBlocked(Vector2Int gridPos)
    {
        if (gridObjects.ContainsKey(gridPos))
        {
            Grid grid = gridObjects[gridPos].GetComponent<Grid>();
            if (grid != null)
            {
                return grid.GridCell.isBlocked;
            }
        }
        return true;
    }
    private void ControlOutline()
    {
        //outline kontrolü yap yetisirse
        //if (CanMove())
        //{
        //    outline.enabled = true;
        //}
    }

    private void SetGridBlocked(Vector2Int gridPos, bool isBlocked)
    {
        if (gridObjects.ContainsKey(gridPos))
        {
            Grid grid = gridObjects[gridPos].GetComponent<Grid>();
            if (grid != null)
            {
                grid.GridCell.isBlocked = isBlocked;
                if (!isBlocked)
                {
                    grid.GridCell.color = Colors.White;
                }
            }
        }
    }
}
