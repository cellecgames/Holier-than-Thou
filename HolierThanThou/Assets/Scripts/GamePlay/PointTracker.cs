﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointTracker : MonoBehaviour

{
    private int basePoints = 1;
    private int multPoints = 1;
    private int totalPoints = 1;
    private float mag;

    // Start is called before the first frame update
    void Start()
    {
        mag = 0;
    }

    // Update is called once per frame
    void Update()
    {
        totalPoints = basePoints * multPoints;

    }

    private void OnCollisionEnter(Collision other)
    {
        var vel = this.GetComponentInParent<Rigidbody>().velocity;
        var _competitor = other.gameObject.GetComponent<Competitor>();

        if (_competitor && vel.magnitude > 8)
        {
            multPoints++;
            Debug.Log("mutltiplier increased!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var crownBox = GameObject.FindGameObjectWithTag("CrownBox");
        other = crownBox.GetComponentInParent<Collider>();
        if (crownBox)
        {
            basePoints++;
            Debug.Log("You picked up a crown!");
            Object.Destroy(crownBox);
        }
    }
    public int PointVal()
    {
        return totalPoints;
    }

    public int MultVal()
    {
        return multPoints;
    }

    public void ResetMult()
    {
        multPoints = 1;
    }

    public void ResetBasePoints()
    {
        basePoints = 1;
    }

    public int baseVal()
    {
        return basePoints;
    }
}
