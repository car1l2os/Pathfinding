using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    [Header("Elements")]
    public GameObject terrain;
    public GameObject marcador;
    public GameObject line;

    [Header("Materials")]
    public Material startMaterial;
    public Material endMaterial;
    public Material neutralMaterial;
    public Material visitedMaterial;

    private Mesh mesh;
    private int[] triangles;
    private Vector3[] vertices;

    private List<Vector3> posiciones_marcadores;
    private List<GameObject> marcadores;
    private List<List<int>> graph;

    private int startNode;
    private int endNode;


	// Use this for initialization
	void Start () {
        mesh = terrain.GetComponentInChildren<MeshFilter>().mesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
        posiciones_marcadores = new List<Vector3>();
        marcadores = new List<GameObject>();
        graph = new List<List<int>>();

        startNode = -1;
        endNode = -1;

        CreateGraph();
	}

    private void CreateGraph()
    {
        for(int i=0; i<triangles.Length;i+=3)
        {
            
            Vector3 baricentro = new Vector3(
                                 (vertices[triangles[i]].x + vertices[triangles[i+1]].x + vertices[triangles[i + 2]].x)/3,
                                 (vertices[triangles[i]].y + vertices[triangles[i + 1]].y + vertices[triangles[i + 2]].y) / 3,
                                 (vertices[triangles[i]].z + vertices[triangles[i + 1]].z + vertices[triangles[i + 2]].z)/ 3);

            posiciones_marcadores.Add(baricentro);
            graph.Add(new List<int>());

            marcadores.Add(Instantiate(marcador, baricentro, Quaternion.identity,GameObject.Find("Graph_Visual_Representation").transform));

            for (int j= i+3; j< triangles.Length;j+=3)
            {
                int[] otherTriangle = { triangles[j], triangles[j + 1], triangles[j + 2] };

                int[] pair_1_to_check = { triangles[i], triangles[i + 1] };
                int[] pair_2_to_check = { triangles[i], triangles[i + 2] };
                int[] pair_3_to_check = { triangles[i+1], triangles[i + 2] };

                int[][] pairs_to_check = { pair_1_to_check, pair_2_to_check, pair_3_to_check };

                for(int l=0;l<pairs_to_check.Length;l++)
                {
                    if (!pairs_to_check[l].Except(otherTriangle).Any())
                    {
                        //Debug.Log("Pareja");
                        int[] triangle = { triangles[i], triangles[i + 1], triangles[i + 2] };
                        DrawLine(triangle, otherTriangle);
                        graph[i/3].Add(j / 3);
                    }
                }
            }

        }

        for (int i= 0;i < graph.Count;i++)
        {
            List<int> actualList = graph[i];
            for(int j=0;j<actualList.Count;j++)
            {
                if(!graph[actualList[j]].Contains(i))
                {
                    graph[actualList[j]].Add(i);
                }
            }
        }
        //Debug.Log(graph);

    }

    // Update is called once per frame
    void Update () {
        if(Input.GetMouseButtonDown(0))
            CheckForClick(0);

        if (Input.GetMouseButtonDown(1))
            CheckForClick(1);

        if (Input.GetKeyDown(KeyCode.Space))
            StarA();
    }

    private void CheckForClick(int button)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.tag == "Marcador")
            {
                if (button == 0)
                {
                    if(startNode != -1)
                        marcadores[startNode].GetComponent<Renderer>().material = neutralMaterial;

                    startNode = GetMarcadorIndexByPos(hit.transform.position);
                    marcadores[startNode].GetComponent<Renderer>().material = startMaterial;
                }  
                else if (button == 1)
                {
                    if (endNode != -1)
                        marcadores[endNode].GetComponent<Renderer>().material = neutralMaterial;

                    endNode = GetMarcadorIndexByPos(hit.transform.position);
                    marcadores[endNode].GetComponent<Renderer>().material = endMaterial;
                }
                    
            }
        }
    }

    private void StarA()
    {
        List<Node> nodes = new List<Node>();
        for(int i=0; i< posiciones_marcadores.Count;i++)
        {
            nodes.Add(new Node(i, posiciones_marcadores[i]));
        }

        Node start = nodes[startNode];
        Node end = nodes[endNode];

        List<Node> posibles = new List<Node>();
        HashSet<Node> visitados = new HashSet<Node>();

        posibles.Add(start);

        while(posibles.Count > 0)
        {
            Node actual = posibles[0];
            for(int i=1;i<posibles.Count;i++) //Cola prioridad 
            {
                if(posibles[i].fCost < actual.fCost || posibles[i].fCost == actual.fCost && posibles[i].hCost < actual.hCost)
                {
                    actual = posibles[i];
                }
            }

            posibles.Remove(actual);
            visitados.Add(actual);

            if (actual == end)
            {
                Debug.Log("Camino encontrado");
                ConsigueCamino(start, end);
                PaintHashOfNodes(visitados);
                break;
            }


            foreach(int neighIndex in graph[actual.indexInGraph])
            {
                if(visitados.Contains(nodes[neighIndex]))
                {
                    continue;
                }

                float cost = actual.gCost + Vector3.Distance(actual.worldPos, nodes[neighIndex].worldPos);
                if(cost < nodes[neighIndex].gCost || !posibles.Contains(nodes[neighIndex]))
                {
                    nodes[neighIndex].gCost = cost;
                    nodes[neighIndex].hCost = Vector3.Distance(nodes[neighIndex].worldPos, end.worldPos);
                    nodes[neighIndex].comesFrom = actual;

                    if(!posibles.Contains(nodes[neighIndex]))
                    {
                        posibles.Add(nodes[neighIndex]);
                    }
                }

            }


        }

    }
    private void Dijkstra()
    {
        int numberNodes = graph.Count;
        //int count = 0;
        List<Node> nodes = new List<Node>();
        HashSet<Node> visitados = new HashSet<Node>();
        for (int i = 0; i < posiciones_marcadores.Count; i++)
        {
            nodes.Add(new Node(i, posiciones_marcadores[i], float.PositiveInfinity));
        }

        Node start = nodes[startNode];
        start.value = 0;

        while (visitados.Count < numberNodes)
        {
            /*count++;
            if(count >= 10000000)
            {
                Debug.Log("Pasos maximos");
                break;
            }*/

            Node actual = start;
            foreach (Node node in nodes)
            {
                if (!visitados.Contains(node))
                {
                    actual = node;
                    break;
                }      
            }

            for (int i = 1; i < nodes.Count; i++) //Cola prioridad 
            {
                if (nodes[i].value < actual.value && !visitados.Contains(nodes[i]))
                {
                    actual = nodes[i];
                }
            }

            visitados.Add(actual);

            if (actual == nodes[endNode])
            {
                ConsigueCamino(start, nodes[endNode]);
                PaintHashOfNodes(visitados);
                break;
            }

            for (int i = 0; i < graph[actual.indexInGraph].Count; i++)
            {
                float n_value = actual.value + Vector3.Distance(actual.worldPos, posiciones_marcadores[graph[actual.indexInGraph][i]]);
                if (n_value < nodes[graph[actual.indexInGraph][i]].value)                               
                {
                    nodes[graph[actual.indexInGraph][i]].value = n_value;
                    nodes[graph[actual.indexInGraph][i]].comesFrom = actual;
                }

            }
        }
    }
    private void DFS()
    {
        List<Node> nodes = new List<Node>();
        HashSet<Node> visitados = new HashSet<Node>();
        for (int i = 0; i < posiciones_marcadores.Count; i++)
        {
            nodes.Add(new Node(i, posiciones_marcadores[i], float.PositiveInfinity));
        }
        Node start = nodes[startNode];
        Node end = nodes[endNode];

        DFS_recursive(start, nodes, visitados,end,start);
    }

    void DFS_recursive(Node actual, List<Node> nodes, HashSet<Node> visitados, Node end,Node start)
    {

        if(actual == end)
        {
            ConsigueCamino(start, nodes[endNode]);
            PaintHashOfNodes(visitados);
        }

        visitados.Add(actual);

        for (int i = 0; i < graph[actual.indexInGraph].Count; i++)
        {
            if (!visitados.Contains(nodes[graph[actual.indexInGraph][i]]))
            {
                nodes[graph[actual.indexInGraph][i]].comesFrom = actual;
                DFS_recursive(nodes[graph[actual.indexInGraph][i]], nodes, visitados,end,start);
            }

        }


    }
    

    void ConsigueCamino(Node start, Node end)
    {
        List<Node> cam = new List<Node>();
        Node act = end; 

        while(act != start)
        {
            cam.Add(act);
            act = act.comesFrom;
        }
        cam.Add(start);
        cam.Reverse();

        PaintPath(cam);

    }

    private void PaintPath(List<Node> cam)
    {
        foreach(GameObject delete in GameObject.FindGameObjectsWithTag("Line"))
        {
            Destroy(delete);
        }

        for(int i=0; i<cam.Count-1;i++)
        {
            DrawLine(cam[i].worldPos, cam[i + 1].worldPos);
        }
    }
    private void PaintListOfNodes(List<Node> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            marcadores[list[i].indexInGraph].GetComponent<Renderer>().material = visitedMaterial;
        }
    }
    private void PaintHashOfNodes(HashSet<Node> list)
    {
        foreach(GameObject marcador in marcadores)
        {
            marcador.GetComponent<Renderer>().material =neutralMaterial;
        }

        foreach(Node node in list)
        {
            marcadores[node.indexInGraph].GetComponent<Renderer>().material = visitedMaterial;
        }
    }
    int GetMarcadorIndexByPos(Vector3 position)
    {
        for(int i=0; i<posiciones_marcadores.Count;i++)
        {
            if (posiciones_marcadores[i] == position)
                return i;
        }
        return -1;
    }
    void DrawLine(int[] one, int[] other)
    {
        Vector3 vertexOne = new Vector3(
                                 (vertices[one[0]].x + vertices[one[1]].x + vertices[one[2]].x) / 3,
                                 (vertices[one[0]].y + vertices[one[1]].y + vertices[one[2]].y) / 3,
                                 (vertices[one[0]].z + vertices[one[1]].z + vertices[one[2]].z) / 3);

        Vector3 vertexOther = new Vector3(
                                 (vertices[other[0]].x + vertices[other[1]].x + vertices[other[2]].x) / 3,
                                 (vertices[other[0]].y + vertices[other[1]].y + vertices[other[2]].y) / 3,
                                 (vertices[other[0]].z + vertices[other[1]].z + vertices[other[2]].z) / 3);


        GameObject n_line = Instantiate(line, Vector3.zero, Quaternion.identity, GameObject.Find("Graph_Visual_Representation").transform);
        n_line.GetComponent<LineRenderer>().SetPosition(0, vertexOne);
        n_line.GetComponent<LineRenderer>().SetPosition(1, vertexOther);

    }
    void DrawLine(Vector3 one, Vector3 other)
    {
        GameObject n_line = Instantiate(line, Vector3.zero, Quaternion.identity, GameObject.Find("Graph_Visual_Representation").transform);
        n_line.GetComponent<LineRenderer>().SetPosition(0, one);
        n_line.GetComponent<LineRenderer>().SetPosition(1, other);

    }
}
