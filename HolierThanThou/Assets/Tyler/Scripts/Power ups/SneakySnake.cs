﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakySnake : PowerUp
{
    public SneakySnake(bool _hasDuration, float _duration, float _radius) : base(_hasDuration, _duration, _radius)
    {

    }

    public override void ActivatePowerUp()
    {
        base.ActivatePowerUp();
        Debug.Log("Sneaky Snake Power Up Used!");

    }

    
}
