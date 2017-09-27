using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public int indexInGraph;
    public Vector3 worldPos;

    public float gCost;
    public float hCost;
    public Node comesFrom;

	public Node(int indexInGraph, Vector3 pos)
    {
        this.indexInGraph = indexInGraph;
        worldPos = pos;
    }

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
