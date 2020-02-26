﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMinotaur : Enemy
{
    [SerializeField] int m_defenseModifier = 2;

    public override void OnHit(int damage)
    {
        Debug.Log("You hit an Minotaur.");
        m_health -= damage / m_defenseModifier;
    }

    protected override void RotateToTarget()
    {
        // face right
        if (m_targetPosition.x - transform.position.x > 0)
        {
            if (transform.localScale == new Vector3(-1, 1, 1))
            {
                transform.localScale = Vector3.one;
            }
        }
        // face left
        else if (m_targetPosition.x - transform.position.x < 0)
        {
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}
