﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateMachine : MonoBehaviour {
    public enum EAIState {
        FINDING_OBJECTIVE,
        USING_POWERUP,
        MOVING_TO_GOAL,
        SCORING_GOAL,
        GRABBING_POWERUP,
        ATTACKING_PLAYER,
        GETTING_UNSTUCK
    }

    // TODO introduce the state stack...
    private Stack<EAIState> m_stateStack = new Stack<EAIState>();

    private EAIState m_currentState;

    // Cached Components
    private Competitor m_competitor;
    private Rigidbody m_rigidbody;

    // AI Pathfinding
    private readonly float m_distanceToCommitToGoal = 10.0f;
    private readonly float m_stoppingDistance = 5.0f;
    // private readonly float m_jumpingDistance = 1.0f;
    // private float m_jumpingForce;

    // Timer
    private float m_minimumTimeToCommitToANewState = 2f;
    private float m_timeOnCurrentState = 0;

    private float m_baseVelocity = 10f;
    private float velocity = 10f;
    public float Velocity {
        get {
            return velocity;
        }
        set {
            if(float.IsNaN(value) || float.IsInfinity(value)) {
                velocity = m_baseVelocity;
            } else {
                velocity = value;
            }
        }
    }

    private NavMeshPath m_navMeshPath;
    private Queue<Vector3> m_cornersQueue = new Queue<Vector3>();
    private Vector3 m_currentGoal;
    [SerializeField] private Transform target;

    // AI Blackboard
    private float m_distanceToCheckForPowerUps = 10f;
    private float m_distanceToCheckForCompetitors = 10f;
    private Transform m_goalTransform;

    public PowerUp slot1;
    public PowerUp slot2;

    private float m_usePowerUpStart = 10f;
    private float m_attackCooldown = 5f;
    private bool m_isBeingKnockedback;
    private bool m_isBully = false;
    private bool m_isItemHog = false;
    private bool m_canActivatePowerUp1 = true;
    private bool m_canActivatePowerUp2 = true;

    // getting unstuck
    private float m_timeWithoutMovingToBeConsideredStuck = 2.0f;
    private float m_timeWithoutMoving = 0f;
    private Vector3 m_positionToGoToGetUnstuck;
    private float m_maximumTimeToGetUnstuck = 2f;
    private float m_timeElapsedTryingToGetUnstuck = 0f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_distanceToCheckForPowerUps);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_distanceToCheckForCompetitors);

        if (m_cornersQueue.Count > 0) {
            foreach (Vector3 corner in m_cornersQueue) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(corner, 1.0f);
            }
        }
    }

    private void Start() {
        slot1 = null;
        slot2 = null;

        m_competitor = GetComponent<Competitor>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_goalTransform = GameObject.FindGameObjectWithTag("Goal").transform;
        target = m_goalTransform;
        ChangeState(EAIState.FINDING_OBJECTIVE);
    }

    public void MakeBully() {
        m_distanceToCheckForCompetitors = 50f;
        m_attackCooldown = 2f;
        m_distanceToCheckForPowerUps = 5f;
        m_isBully = true;
    }

    public void MakeItemHog() {
        m_distanceToCheckForCompetitors = 5f;
        m_distanceToCheckForPowerUps = 50f;
        m_isItemHog = true;
    }

    private void Update() {
        m_timeOnCurrentState += Time.deltaTime;

        switch(m_currentState) {
            case EAIState.MOVING_TO_GOAL:
                MoveToGoalState();
                break;
            case EAIState.SCORING_GOAL:
                ScoreGoalState();
                break;
            case EAIState.FINDING_OBJECTIVE:
                FindObjectiveState();
                break;
            case EAIState.GRABBING_POWERUP:
                GrabbingPowerUpState();
                break;
            case EAIState.ATTACKING_PLAYER:
                AttackingPlayerState();
                break;
            case EAIState.USING_POWERUP:
                ChangeState(EAIState.FINDING_OBJECTIVE);
                break;
            case EAIState.GETTING_UNSTUCK:
                GettingUnstuckState();
                break;
        }

        if (Vector3.Distance(transform.position, m_currentGoal) < m_stoppingDistance) {
            RecalculatePath();
        }

        // Getting Unstuck
        if (m_rigidbody.velocity.magnitude < 3.0f) {
            m_timeWithoutMoving += Time.deltaTime;

            if(m_timeWithoutMoving > m_timeWithoutMovingToBeConsideredStuck) {
                m_timeWithoutMoving = 0;
                m_timeElapsedTryingToGetUnstuck = 0f;
                Vector3 randomInCircle = UnityEngine.Random.insideUnitCircle;
                m_positionToGoToGetUnstuck = transform.position + new Vector3(randomInCircle.x, 0f, randomInCircle.y) * velocity;
                ClearPath();
                ChangeState(EAIState.GETTING_UNSTUCK);
            }
        } else {
            m_timeWithoutMoving = 0f;
        }
    }

    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // Handling AI States

    #region Finding Objective
    private void FindObjectiveState() {
        target = null;
        Transform targetToFollow;

        if(UseEnhacementPowerUp()) {
            return;
        } else if(CanGetPowerUp(out targetToFollow)) {
            Debug.Log($"Getting Power Up!");
            target = targetToFollow;
            ChangeState(EAIState.GRABBING_POWERUP);
        } else if(CanAttackOtherCompetitor(out targetToFollow)) {
            target = targetToFollow;
            ChangeState(EAIState.ATTACKING_PLAYER);
            return;
        }

        RunPathCalculation();
    }

    private bool CanGetPowerUp(out Transform targetToFollow) {
        if(slot1 != null && slot2 != null) {
            targetToFollow = null;
            return false;
        }

        PowerUpBox[] powerUpBoxes = FindObjectsOfType<PowerUpBox>();
        List<PowerUpBox> powerUpBoxesWithinDistance = new List<PowerUpBox>();

        foreach(PowerUpBox box in powerUpBoxes) {
            if(Vector3.Distance(transform.position, box.transform.position) < m_distanceToCheckForPowerUps && !box.IsDisabled) {
                powerUpBoxesWithinDistance.Add(box);
            }
        }

        if(powerUpBoxesWithinDistance.Count == 0) {
            targetToFollow = null;
            return false;
        }

        PowerUpBox closestBox = powerUpBoxesWithinDistance[0];
        for(int i = 1; i < powerUpBoxesWithinDistance.Count; i++) {
            if(Vector3.Distance(transform.position, closestBox.transform.position) > Vector3.Distance(transform.position, powerUpBoxesWithinDistance[i].transform.position)) {
                closestBox = powerUpBoxesWithinDistance[i];
            }
        }

        targetToFollow = closestBox.transform;
        return true;
    }

    private bool CanAttackOtherCompetitor(out Transform competitorToFollow) {
        Competitor[] allCompetitors = FindObjectsOfType<Competitor>();
        List<Competitor> allCompetitorsWithinDistance = new List<Competitor>();

        foreach(Competitor competitor in allCompetitors) {
            if(competitor == this.m_competitor) {
                continue;
            }

            // check if it is within distance...
            if(Vector3.Distance(transform.position, competitor.transform.position) < m_distanceToCheckForCompetitors &&
                Vector3.Distance(transform.position, competitor.transform.position) > m_distanceToCheckForCompetitors / 2.0f) {
                allCompetitorsWithinDistance.Add(competitor);
            }
        }

        if(allCompetitorsWithinDistance.Count == 0) {
            competitorToFollow = null;
            return false;
        }

        // set target to closest competitor
        Competitor closestCompetitor = allCompetitorsWithinDistance[0];
        for(int i = 1; i < allCompetitorsWithinDistance.Count; i++) {
            if(Vector3.Distance(transform.position, closestCompetitor.transform.position) > Vector3.Distance(transform.position, allCompetitorsWithinDistance[i].transform.position)) {
                closestCompetitor = allCompetitors[i];
            }
        }

        competitorToFollow = closestCompetitor.transform;
        return true;
    }
    #endregion

    #region Using Power Ups
    private bool UseEnhacementPowerUp() {
        Debug.Log($"AI - UseEnhancementPowerUp: {slot1 != null} | {slot2 != null}");
        if(slot1 != null && slot1.isEnhancement) {
            if(m_canActivatePowerUp1) {
                UsePowerUp(true);
            }

            ChangeState(EAIState.USING_POWERUP);
            return true;
        } else if(slot2 != null && slot2.isEnhancement) {
            if(m_canActivatePowerUp2) {
                UsePowerUp(false);
            }

            ChangeState(EAIState.USING_POWERUP);
            return true;
        }

        return false;
    }

    private void UsePowerUp(bool _isSlot1) {
        StartCoroutine(UsePowerUpRoutine(_isSlot1));
    }

    private IEnumerator UsePowerUpRoutine(bool _isSlot1) {
        if(_isSlot1) {
            m_canActivatePowerUp1 = false;
            slot1.ActivatePowerUp(m_competitor.Name, m_competitor.origin);
            yield return new WaitForSeconds(slot1.duration);
            slot1.ResetEffects(m_competitor.Name);
            slot1 = null;
            m_canActivatePowerUp1 = true;
        } else {
            m_canActivatePowerUp2 = false;
            slot2.ActivatePowerUp(m_competitor.Name, m_competitor.origin);
            yield return new WaitForSeconds(slot2.duration);
            slot2.ResetEffects(m_competitor.Name);
            slot2 = null;
            m_canActivatePowerUp2 = true;
        }
    }
    #endregion

    #region Grabbing Power Up State
    private void GrabbingPowerUpState() {
        Debug.Log($"Grabbing Power Up State");
        PowerUpBox boxBeingGrabbed = target.GetComponent<PowerUpBox>();

        Transform newTarget;
        if(m_isBully && CanAttackOtherCompetitor(out newTarget) && HasSpentEnoughTimeOnCurrentState()) {
            target = newTarget;
            ChangeState(EAIState.ATTACKING_PLAYER);
            return;
        }

        // Performing this check because the box can be disabled
        // maybe someone else grabbed the box while I was on my way to it :(
        // maybe ME got the box :)
        if(boxBeingGrabbed == null || boxBeingGrabbed.IsDisabled || (slot1 != null && slot2 != null)) {
            target = null;
            ChangeState(EAIState.FINDING_OBJECTIVE);
            return;
        }

        MoveTowardsCorner();
    }
    #endregion

    #region Attacking Player State
    private void AttackingPlayerState() {
        if(Vector3.Distance(target.position, transform.position) > m_distanceToCheckForCompetitors) {
            ChangeState(EAIState.FINDING_OBJECTIVE);
            return;
        }

        // If we are too slow it is not interesting to attack other balls because we will not knockback them and won't get any multiplier...
        if(m_rigidbody.velocity.magnitude < 6.0f &&
            Vector3.Distance(transform.position, target.position) < m_distanceToCheckForCompetitors / 2.0f) {
            target = null;
            Transform newTarget;
            if(CanGetPowerUp(out newTarget)) {
                target = newTarget;
                ChangeState(EAIState.GRABBING_POWERUP);
            }

            RunPathCalculation();
            return;
        }

        // Debug.Log($"Attacking Player State");
        HardFollowTarget();

    }
    #endregion

    #region Move To Goal State
    private void MoveToGoalState() {
        Transform powerUpToGet;
        Transform playerToAttack;

        if(CanGetPowerUp(out powerUpToGet)) {
            if (Vector3.Distance(transform.position, powerUpToGet.position) < Vector3.Distance(transform.position, target.position)) {
                target = powerUpToGet;
                ChangeState(EAIState.GRABBING_POWERUP);
                RunPathCalculation();
                return;
            }
        } else if(CanAttackOtherCompetitor(out playerToAttack) && HasSpentEnoughTimeOnCurrentState()) {
            if(Vector3.Distance(transform.position, playerToAttack.position) < Vector3.Distance(transform.position, target.position)) {
                target = playerToAttack;
                ChangeState(EAIState.ATTACKING_PLAYER);
                return;
            }
        }

        // We are too close to the goal so now we commit to getting into it!!
        if(Vector3.Distance(transform.position, target.position) < m_distanceToCommitToGoal) {
            ChangeState(EAIState.SCORING_GOAL);
            return;
        }

        MoveTowardsCorner();
    }
    #endregion

    #region Score Goal State
    private void ScoreGoalState() {
        // Debug.Log($"IMMA SCORE!!");
        if(Vector3.Distance(transform.position, target.position) >= m_distanceToCommitToGoal) {
            // something happened and we are far from the goal, guess I will just do something else (shrug)
            ChangeState(EAIState.FINDING_OBJECTIVE);
            return;
        }

        HardFollowTarget();
    }
    #endregion

    #region Getting Unstuck State
    private void GettingUnstuckState() {
        m_timeElapsedTryingToGetUnstuck += Time.deltaTime;

        if(m_timeElapsedTryingToGetUnstuck > m_maximumTimeToGetUnstuck) {
            ChangeState(EAIState.FINDING_OBJECTIVE);
            return;
        }

        Debug.DrawRay(transform.position, (m_positionToGoToGetUnstuck - transform.position), Color.red, Time.deltaTime);
        HardGoToPosition(m_positionToGoToGetUnstuck);
    }
    #endregion

    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------

    #region Changing States
    private void ChangeState(EAIState _newState) {
        m_timeOnCurrentState = 0;
        m_currentState = _newState;
    }

    private bool HasSpentEnoughTimeOnCurrentState() {
        return m_timeOnCurrentState >= m_minimumTimeToCommitToANewState;
    }
    #endregion


    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------

    #region AI Pathfinding
    private void MoveTowardsCorner() {
        Debug.DrawRay(transform.position, (m_currentGoal - transform.position), Color.red, Time.deltaTime);
        m_rigidbody.AddForce((m_currentGoal - transform.position).normalized * velocity, ForceMode.Force);
    }

    private void HardFollowTarget() {
        Debug.DrawRay(transform.position, (target.position - transform.position), Color.green, Time.deltaTime);
        m_rigidbody.AddForce((target.position - transform.position).normalized * velocity, ForceMode.Force);
    }

    private void HardGoToPosition(Vector3 _position) {
        Debug.DrawRay(transform.position, (_position - transform.position), Color.blue, Time.deltaTime);
        m_rigidbody.AddForce((_position - transform.position).normalized * velocity, ForceMode.Force);
    }

    public void RunPathCalculation() {
        m_cornersQueue = new Queue<Vector3>();
        m_navMeshPath = new NavMeshPath();

        if(target == null) {
            target = m_goalTransform;
            ChangeState(EAIState.MOVING_TO_GOAL);
        }

        if(NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, m_navMeshPath)) {
            foreach (Vector3 cornerPosition in m_navMeshPath.corners) {
                m_cornersQueue.Enqueue(cornerPosition);
            }
        }

        RecalculatePath();
    }

    public void RecalculatePath() {
        if(m_cornersQueue.Count == 0) {
            // target = null;
            // ChangeState(EAIState.FINDING_OBJECTIVE);
        } else {
            m_currentGoal = m_cornersQueue.Dequeue();
        }
    }

    public void ClearPath() {
        target = null;
        m_cornersQueue.Clear();
    }
    #endregion
}
