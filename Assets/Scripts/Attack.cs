﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to be derived for PlayerAttack & EnemyAttack
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(BoxCollider2D))]
public class Attack : MonoBehaviour
{
    protected BoxCollider2D[] m_hitAreas;
    protected List<BoxCollider2D> m_objectsToHit;
    protected Animator m_animator;
    protected int m_activeColliderID = 0;
    protected AudioSource m_audioSource;

    #region Unity methods
    /// <summary>
    /// Creates lists of BoxCollider & gets reference of animator
    /// </summary>
    protected virtual void Awake()
    {
        m_hitAreas = GetComponents<BoxCollider2D>();
        m_objectsToHit = new List<BoxCollider2D>();
        m_animator = transform.parent.GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        // Set Colliders as trigger & randomize
        foreach (var collider in m_hitAreas)
        {
            collider.isTrigger = true;
            RandomizeActiveCollider();
        }
        // Setup Audiosource
        m_audioSource.playOnAwake = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BoxCollider2D>() && !HasNonHitableCollider(collision))
        {
            m_objectsToHit.Add(collision.GetComponent<BoxCollider2D>());
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (!HasNonHitableCollider(collision))
        {
            if (m_objectsToHit.Contains(collision.GetComponent<BoxCollider2D>()))
            {
                m_objectsToHit.Remove(collision.GetComponent<BoxCollider2D>());
            }
        }
    }
    #endregion

    protected void RandomizeActiveCollider()
    {
        m_activeColliderID = Random.Range(0, m_hitAreas.Length);
        m_objectsToHit.Clear();
        foreach (var collider in m_hitAreas)
        {
            collider.enabled = false;
        }
        m_hitAreas[m_activeColliderID].enabled = true;
    }

    /// <summary>
    /// Checks of Collider are hitable
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    bool HasNonHitableCollider(Collider2D collision)
    {
        if (collision.GetComponent<Attack>() || 
            collision.GetComponent<EnemyDetectionArea>() || 
            collision.GetComponent<PlayerGroundSensor>())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual void PlaySound()
    {
        if (m_audioSource.clip)
        {
            m_audioSource.PlayOneShot(m_audioSource.clip);
        }
        else
        {
            Debug.LogError("No clip set in: " + this.gameObject);
        }
    }
}
