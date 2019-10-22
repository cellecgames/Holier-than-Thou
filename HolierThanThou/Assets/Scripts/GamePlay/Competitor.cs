﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Competitor : MonoBehaviour
{
    //Name and scoring Variables
    public string Name;
    public int Score;
    private bool scoredGoal;

    //Powerup constructor intakes
    public Transform origin;
    public bool navMeshOff;

    //Variables for power up effects
    public float blastedDuration;
    public bool untouchable;
    public bool inivisible;
    public bool ballOfSteel;
    public Material startMaterial;


    private void Awake()
    {
        this.transform.localScale = new Vector3(.025f, .025f, .025f);
        origin = this.transform;
        navMeshOff = false;
        untouchable = false;
        inivisible = false;
    }

    public void Start()
    {
        if (transform.childCount > 1)
        {
            Transform t = transform.GetChild(1);
            if (t.childCount > 0)
            {
                t = t.GetChild(0);
                startMaterial = t.GetComponent<MeshRenderer>().material;
                return;
            }
        }
        startMaterial = transform.GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {

    }

    public bool ScoredGoal
    {
        get { return scoredGoal; }

        set { scoredGoal = value; }
    }

    public void BallOfSteel(Transform origin, float duration)
    {
        StartCoroutine(Unbouncable(origin, duration));
    }

    public void BeenBlasted()
    {
        StartCoroutine(TurnNavMeshBackOn(blastedDuration));
    }

    public void BeenChilled(Competitor competitor, float duration)
    {
        StartCoroutine(TurnMovementControlBackOn(competitor, duration));
    }

    public void CantTouchMe(float duration)
    {
        StartCoroutine(Untouchable(duration));
    }

    public void CantFindMe(float duration)
    {
        StartCoroutine(Invis(duration));
    }

    public void WentFast(Transform origin, float duration, float speedMultiplier)
    {
        StartCoroutine(ResetSpeed(origin, duration, speedMultiplier));
    }

    public void BeenSlowed(Competitor competitor, float duration, float speedMultiplier)
    {
        StartCoroutine(ReverseMovementSpeed(competitor, duration, speedMultiplier));
    }

    IEnumerator Invis(float duration)
    {
        inivisible = true;
        //GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(duration);
        inivisible = false;
        //GetComponent<MeshRenderer>().enabled = true;
    }

    private IEnumerator Untouchable(float duration)
    {
        untouchable = true;
        yield return new WaitForSeconds(duration);
        untouchable = false;

    }

    private IEnumerator Unbouncable(Transform origin, float duration)
    {
        ballOfSteel = true;
        yield return new WaitForSeconds(duration);
        ballOfSteel = false;
        origin.GetComponent<BounceFunction>().enabled = true;
        origin.GetComponent<MeshRenderer>().sharedMaterial = startMaterial;
    }

    private IEnumerator TurnNavMeshBackOn(float duration)
    {
        yield return new WaitForSeconds(duration);
        navMeshOff = false;

    }

    private IEnumerator TurnMovementControlBackOn(Competitor competitor, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (competitor.GetComponent<RigidBodyControl>())
        {
            competitor.GetComponent<Rigidbody>().freezeRotation = false;
            competitor.GetComponent<RigidBodyControl>().enabled = true;
        }
        else
        {
            competitor.GetComponent<Rigidbody>().freezeRotation = false;
            competitor.GetComponent<AIBehavior>().enabled = true;
        }

    }

    private IEnumerator ResetSpeed(Transform origin, float duration, float speedMultiplier)
    {
        yield return new WaitForSeconds(duration);

        if (origin.GetComponent<RigidBodyControl>())
        {
            origin.GetComponent<RigidBodyControl>().speed /= speedMultiplier;
        }
        else
        {
            origin.GetComponent<AIBehavior>().velocity /= speedMultiplier;
        }
    }

    private IEnumerator ReverseMovementSpeed(Competitor competitor, float duration, float speedMultiplier)
    {
        yield return new WaitForSeconds(duration);

        if (competitor.GetComponent<RigidBodyControl>())
        {
            competitor.GetComponent<RigidBodyControl>().speed /= speedMultiplier;
        }
        else
        {
            competitor.GetComponent<AIBehavior>().velocity /= speedMultiplier;
        }
    }
}
