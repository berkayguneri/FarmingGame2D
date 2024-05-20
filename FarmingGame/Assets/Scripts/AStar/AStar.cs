using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("Tiles & Tilemap References")]
    [Header("Options")]
    [SerializeField] private bool observeMovementPenalties = true;

    [Range(0, 20)]
    [SerializeField] private int pathMovementPenalty = 0;
    [Range(0, 20)]
    [SerializeField] private int defaultMovementPenalty = 0;

    private GridNodes gridNodes;
    private Node startNode;
    private Node targetNode;

    private int gridWidth;
    private int gridHeight;
    private int originX;
    private int originY;

    private List<Node> openNodeList;
    private HashSet<Node> closedNodeList;

    private bool pathFound = false;

    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition, Stack<NPCMovementStep> npcMovementSteps)
    {

        pathFound = false;

        if (PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, endGridPosition))
        {
            if (FindShortesPath())
            {
                UpdatePathOnNPCMovementStepStack(sceneName, npcMovementSteps);
                return true;
            }

        }

        return false;
    }

    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
    {
        Node nextNode = targetNode;

        while (nextNode != null)
        {
            NPCMovementStep npcMovementStep = new NPCMovementStep();

            npcMovementStep.sceneName = sceneName;
            npcMovementStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);

            npcMovementStepStack.Push(npcMovementStep);
            nextNode = nextNode.parentNode;
        }
    }

    private bool FindShortesPath()
    {
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();

            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            closedNodeList.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            EvaluateCurrentNodeNeighbours(currentNode);
        }

            if (pathFound)
            {
                return true;
            }
            else
            {
                return false;
            }
        
        
    }
    
    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighourNode;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                validNeighourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);

                if (validNeighourNode != null)
                {
                    int newCostToNeighbour;

                    if (observeMovementPenalties)
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighourNode) + validNeighourNode.movementPenalty;
                    }
                    else
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighourNode);
                    }

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighourNode);

                    if (newCostToNeighbour < validNeighourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighourNode.gCost = newCostToNeighbour;
                        validNeighourNode.hCost = GetDistance(validNeighourNode, targetNode);

                        validNeighourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighourNode);
                        }
                    }
                }
            }

        }
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private Node GetValidNodeNeighbour(int neighbourNodeXposition, int neighbourNodeYposition)
    {
        if (neighbourNodeXposition >= gridWidth || neighbourNodeXposition < 0 || neighbourNodeYposition >= gridHeight || neighbourNodeYposition < 0)
        {
            return null;
        }

        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXposition, neighbourNodeYposition);

        if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }

    private bool PopulateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition)
    {
        SceneSave sceneSave;

        if (GridPropertiesManager.Instance.GameobjectSave.sceneData.TryGetValue(sceneName.ToString(), out sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {

                if (GridPropertiesManager.Instance.GetGridDimension(sceneName, out Vector2Int gridDimension, out Vector2Int gridOrigin))
                {
                    gridNodes = new GridNodes(gridDimension.x, gridDimension.y);
                    gridWidth = gridDimension.x;
                    gridHeight = gridDimension.y;
                    originX = gridOrigin.x;
                    originY = gridOrigin.y;

                    openNodeList = new List<Node>();

                    closedNodeList = new HashSet<Node>();

                }
                else
                {
                    return false;
                }

                startNode = gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x, startGridPosition.y - gridOrigin.y);

                targetNode = gridNodes.GetGridNode(endGridPosition.x - gridOrigin.x, endGridPosition.y - gridOrigin.y);

                for (int x = 0; x < gridDimension.x; x++)
                {
                    for (int y = 0; y < gridDimension.y; y++)
                    {
                        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(x + gridOrigin.x, y + gridOrigin.y,sceneSave.gridPropertyDetailsDictionary);

                        if (gridPropertyDetails != null)
                        {
                            if (gridPropertyDetails.isNPCObstacle == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.isObstacle = true;
                            }
                            else if (gridPropertyDetails.isPath == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = pathMovementPenalty;
                            }
                            else
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = defaultMovementPenalty;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }
        return true;

    }

    
}
