using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0; // baþlangýç düðümünden uzaklýk
    public int hCost = 0;// bitis düðümünden uzaklýk
    public bool isObstacle = false;
    public int movementPenalty;
    public Node parentNode;


    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        parentNode = null;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        //compare will be <0 if this instance Fcost is less than nodeToCompare.Fcost
        //compare will be >0 if this instance Fcost is greater than nodeToCompare.Fcost

        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return compare;
    }
}
