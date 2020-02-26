using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to start animations on objects, NOT ENEMYS, which has been hit by the Player
/// </summary>
public class HitableObject : MonoBehaviour, IHitable
{

    public void OnHit(int damage)
    {
        Debug.LogError("Method not implemented");
    }

    void Awake()
    {

    }
}
