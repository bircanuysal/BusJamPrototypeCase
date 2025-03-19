using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private GridCell gridCell;

    public GridCell GridCell{
        get{return gridCell;}
        set{gridCell = value;
            if (gridCell.color == Colors.Gray)
            {
                MakeObstacle();
            }
        }
    }
    private void MakeObstacle()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
