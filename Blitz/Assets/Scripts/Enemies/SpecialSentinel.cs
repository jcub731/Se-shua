﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialSentinel : Sentinel
{
    public bool standingBy = true;

    new private void Start()
    {
        targetTag = "Ally";
        gunAudioHit = GetComponents<AudioSource>()[0];
        gunAudioMiss = GetComponents<AudioSource>()[1];
        weaponDamage = 25;
    }

    protected void Update()
    {
        if (patrolState)
        {
            DetectAlliesPatrol();
            Patrol();
        }
        else if (seekingCoverState)
        {
            if (!seekingCover)
            {
                seekCover();
            }
            if (!this.moving)
            {
                this.seekingCover = false;
                float z = gameObject.transform.position.z;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0.5f);
                SetToCombatState();
            }
        }
        else if (combatState)
        {
            shootAllies();
            DetectAlliesCombat();
        }
    }

    protected void DetectAlliesPatrol()
    {
        closestAlly = GetClosestObject("Ally");
        if (closestAlly != null)
        {
            if (Vector3.Distance(transform.position, closestAlly.transform.position) <= detectionRange)
            {
                SetToSeekingCoverState();
                direction = DetermineDirection(closestAlly);
            }
        }
    }

    protected void DetectAlliesCombat()
    {
        closestAlly = GetClosestObject("Ally");
        if (closestAlly != null)
        {
            if (Vector3.Distance(transform.position, closestAlly.transform.position) > detectionRange)
            {
                float z = gameObject.transform.position.z;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                SetToPatrolState();
            }
        }
        else
        {
            float z = gameObject.transform.position.z;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            SetToPatrolState();
        }
    }


    protected void seekCover()
    {
        this.seekingCover = true;
        direction = DetermineDirection(closestAlly);
        GoToClosetCoverWithDirection(direction);
    }

    protected string DetermineDirection(GameObject ally)
    {
        string direction = null;
        Vector2 vector = ally.transform.position - transform.position;
        float angle = Vector2.Angle(Vector2.up, vector);
        if (vector.x > 0 && 45 < angle && angle < 135)
        {
            direction = "right";
        }
        else if (angle >= 135)
        {
            direction = "down";
        }
        else if (vector.x <= 0 && 45 < angle && angle < 135)
        {
            direction = "left";
        }
        else if (angle <= 45)
        {
            direction = "up";
        }
        return direction;
    }

    protected void GoToClosetCoverWithDirection(string direction)
    {
        //UWAGA, nie wiem czemu gdy szuka osłony LEFT jest błąd w znajdywaniu ścieżki, target node wydaje się legitny
        //WYGLĄDA NA TO, ZE JUZ DZIALA - na wszelki wypadek tu zostawie - poza tym to do usuniecia
        Node targetNode = Grid.GetClosestNodeWithCover(transform.position, direction);
        pathfindingTargetVector.x = targetNode.position[0];
        pathfindingTargetVector.y = targetNode.position[1];
        this.moving = true;
        PathfindingManager.RequestPath(transform.position, pathfindingTargetVector, OnPathFound);
    }

    protected void SetToPatrolState()
    {
        patrolState = true;
        seekingCoverState = false;
        combatState = false;
    }

    protected void SetToCombatState()
    {
        patrolState = false;
        seekingCoverState = false;
        combatState = true;
    }

    protected void SetToSeekingCoverState()
    {
        patrolState = false;
        seekingCoverState = true;
        combatState = false;
    }


    public void moveToPosition(Vector2 repositionTargetVector, Vector2 repositionPatrolTargetVector)
    {
        PathfindingManager.RequestPath(transform.position, repositionTargetVector, OnPathFound);
        this.startingPosition = repositionTargetVector;
        this.patrolTargetVector = repositionPatrolTargetVector;
        SetToPatrolState();
    }

}