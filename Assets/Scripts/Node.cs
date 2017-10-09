using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Node {

    public int indexInGraph;
    public Vector3 worldPos;

    public float gCost;
    public float hCost;
    public Node comesFrom;

    //Djisktra
    public float value;


    public Node(int indexInGraph, Vector3 pos)
    {
        this.indexInGraph = indexInGraph;
        worldPos = pos;
    }

    public Node(int indexInGraph, Vector3 pos, float value)
    {
        this.indexInGraph = indexInGraph;
        worldPos = pos;
        this.value = value;
    }

    public Node()
    {
        this.indexInGraph = -1;
    }


    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}


