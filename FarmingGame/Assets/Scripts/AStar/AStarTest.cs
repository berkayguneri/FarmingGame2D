using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(AStar))]
public class AStarTest : MonoBehaviour
{
    [SerializeField] private NPCPath npcPath = null;
    [SerializeField] private bool moveNPC = false;
    [SerializeField] private Vector2Int finishPosition;
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip evenAnimationClip = null;
    private NPCMovement npcMovement;



    private void Start()
    {
        npcMovement = npcPath.GetComponent<NPCMovement>();
        npcMovement.npcFacingDirectionAtDestination = Direction.none;
        npcMovement.npcTargetAnimationClip = idleDownAnimationClip;
    }

    private void Update()
    {
        if (moveNPC)
        {
            moveNPC = false;

            NPCScheduleEvent npsScheduleEvent = new NPCScheduleEvent(0, 0, 0, 0, Weather.none, Season.none, SceneName.Scene1_Farm, new GridCoordinate(finishPosition.x, finishPosition.y), evenAnimationClip);

            npcPath.BuildPath(npsScheduleEvent); 
        }
    }
    
}
