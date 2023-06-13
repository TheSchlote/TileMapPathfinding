using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int currentTileX;
    public int currentTileY;
    public TileMap associatedMap;

    public List<Node> currentPathList = null;
    public float unitMovementSpeed;

    private void Update()
    {
        if (currentPathList != null)
        {
            int currentNodeIndex = 0;
            while (currentNodeIndex < currentPathList.Count - 1)
            {
                Vector3 start = associatedMap.TileCoordToWorldCoord(currentPathList[currentNodeIndex].x, currentPathList[currentNodeIndex].y) + new Vector3(0, 0, -1f);
                Vector3 end = associatedMap.TileCoordToWorldCoord(currentPathList[currentNodeIndex + 1].x, currentPathList[currentNodeIndex + 1].y) + new Vector3(0, 0, -1f);
                Debug.DrawLine(start, end, Color.red);
                currentNodeIndex++;
            }
        }
    }

    public void StartMove()
    {
        Debug.Log("StartMove called");
        // Ensure there is a path and movement speed.
        if (currentPathList == null || unitMovementSpeed <= 0)
        {
            return;
        }

        // Stop any existing move coroutine in case StartMove is called multiple times.
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveOverTime());
    }

    IEnumerator MoveOverTime()
    {
        float remainingMovement = unitMovementSpeed;

        while (remainingMovement > 0 && currentPathList != null)
        {
            // Get cost from current tile to next tile
            float cost = associatedMap.CostToEnterTile(currentPathList[0].x, currentPathList[0].y, currentPathList[1].x, currentPathList[1].y);

            // If the cost exceeds the remaining movement, wait until the next turn.
            if (cost > remainingMovement)
            {
                yield break;
            }

            remainingMovement -= cost;

            // Move to the next tile in the sequence
            currentTileX = currentPathList[1].x;
            currentTileY = currentPathList[1].y;
            transform.position = associatedMap.TileCoordToWorldCoord(currentTileX, currentTileY);

            // Remove the old "current" tile
            currentPathList.RemoveAt(0);

            if (currentPathList.Count == 1)
            {
                // We only have one tile left in the path, and that tile MUST be our ultimate destination.
                // So let's just clear our pathfinding info
                Debug.Log("MoveOverTime ended because path is complete");

                currentPathList = null;
            }

            // Wait for a specified amount of time before moving to the next tile.
            yield return new WaitForSeconds(1); // Wait for 1 second. Change this to control the time between moves.
        }
    }

    // Keep track of the running coroutine so we can stop it if we need to.
    private Coroutine moveCoroutine;

}
