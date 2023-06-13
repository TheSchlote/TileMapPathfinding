using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public GameObject selectedGameObject;
    public TileType[] tileTypes;

    int[,] tileGrid;
    Node[,] nodeGrid;
    public List<Node> currentPathList;
    private readonly int mapWidth = 10;
    private readonly int mapLength = 10;
    private void Start()
    {
        Unit selectedUnit = selectedGameObject.GetComponent<Unit>();
        selectedUnit.currentTileX = (int)selectedGameObject.transform.position.x;
        selectedUnit.currentTileY = (int)selectedGameObject.transform.position.y;
        selectedUnit.associatedMap = this;
        GenerateTileGridData();
        GenerateNodeGridPathfinding();
        GenerateVisualRepresentationOfMap();
    }
    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        TileType targetTileType = tileTypes[tileGrid[targetX, targetY]];

        float movementCost = targetTileType.movementCost;

        if (sourceX != targetX && sourceY != targetY)
        {
            // We are moving diagonally, calculate cost for diagonal movement
            movementCost += 0.001f;
        }

        return movementCost;
    }

    private void GenerateTileGridData()
    {
        tileGrid = new int[mapWidth, mapLength];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                tileGrid[x, y] = 0;
            }
        }
    }

    void GenerateNodeGridPathfinding()
    {
        nodeGrid = new Node[mapWidth, mapLength];
        //Initialize a Node for each spot in the array
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                nodeGrid[x, y] = new Node
                {
                    x = x,
                    y = y
                };
            }
        }
        //Now that all the nodes exist, calculate their neighbours
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                //For Square map
                if (x > 0)
                {
                    nodeGrid[x, y].neighbours.Add(nodeGrid[x - 1, y]);
                }
                if (x < mapWidth - 1)
                {
                    nodeGrid[x, y].neighbours.Add(nodeGrid[x + 1, y]);
                }
                if (y > 0)
                {
                    nodeGrid[x, y].neighbours.Add(nodeGrid[x, y - 1]);
                }
                if (y < mapLength - 1)
                {
                    nodeGrid[x, y].neighbours.Add(nodeGrid[x, y + 1]);
                }
            }
        }
    }

    private void GenerateVisualRepresentationOfMap()
    {
        //Now that we have our map data, let's spawn the visual prefabs
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                TileType currentTileType = tileTypes[tileGrid[x, y]];
                GameObject newTileObject = Instantiate(currentTileType.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile clickableTileComponent = newTileObject.GetComponent<ClickableTile>();
                clickableTileComponent.tileX = x;
                clickableTileComponent.tileY = y;
                clickableTileComponent.map = this;
            }
        }
    }
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, y, 0);
    }
    public void GeneratePathTo(int destinationX, int destinationY)
    {
        //Clear out our unit's old path.
        Unit selectedUnit = selectedGameObject.GetComponent<Unit>();
        selectedUnit.currentPathList = null;

        Dictionary<Node, float> nodeDistanceMap = new();
        Node sourceNode = nodeGrid[selectedUnit.currentTileX, selectedUnit.currentTileY];
        nodeDistanceMap[sourceNode] = 0;
        Dictionary<Node, Node> previousNodeMap = new()
        {
            [sourceNode] = null
        };

        List<Node> unvisitedNodes = new();
        foreach (Node currentNode in nodeGrid)
        {
            if (currentNode != sourceNode)
            {
                nodeDistanceMap[currentNode] = Mathf.Infinity;
                previousNodeMap[currentNode] = null;
            }

            unvisitedNodes.Add(currentNode);
        }
        Node targetNode = nodeGrid[destinationX, destinationY];
        while (unvisitedNodes.Count > 0)
        {
            Node shortestDistanceNode = null;
            foreach (Node possibleNode in unvisitedNodes)
            {
                if (shortestDistanceNode == null || nodeDistanceMap[possibleNode] < nodeDistanceMap[shortestDistanceNode])
                {
                    shortestDistanceNode = possibleNode;
                }
            }

            if (shortestDistanceNode == targetNode)
            {
                break;
            }

            unvisitedNodes.Remove(shortestDistanceNode);

            foreach (Node neighbourNode in shortestDistanceNode.neighbours)
            {
                float alternatePathDistance = nodeDistanceMap[shortestDistanceNode];
                if (alternatePathDistance < nodeDistanceMap[neighbourNode])
                {
                    nodeDistanceMap[neighbourNode] = alternatePathDistance;
                    previousNodeMap[neighbourNode] = shortestDistanceNode;
                }
            }
        }
        if (previousNodeMap[targetNode] == null)
        {
            //No route between target and source
            return;
        }
        List<Node> currentPath = new();
        Node curr = targetNode;

        while (curr != null)
        {
            currentPath.Add(curr);
            curr = previousNodeMap[curr];
        }
        currentPath.Reverse();

        selectedUnit.currentPathList = currentPath;
    }
}
