using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System to check if the Player is grounded. 
/// Base Idea from: Unity Asset Store Bandits - Pixel Art from Sven Thole
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerGroundSensor : MonoBehaviour
{
    BoxCollider2D m_col2D;
    bool m_isGrounded = false;
    int m_colCounter = 0;

    private void Awake()
    {
        // Get required Collider
        m_col2D = GetComponent<BoxCollider2D>();
        // Setup Collider 
        if (!m_col2D.isTrigger)
        {
            m_col2D.isTrigger = true;
        }
    }

    public bool IsGrounded()
    {
        if (m_colCounter > 0)
        {
            m_isGrounded = true;
        }
        else 
        {
            m_isGrounded = false;
        }

        return m_isGrounded;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Checking if Object has a renderer and is not an Enemy
        if (collision.GetComponent<Renderer>()) 
        {
            Renderer collisionRenderer = collision.GetComponent<Renderer>();
            // Checking if Collider is on the same layer as sensor (Unity does not have a SortingLayer.Default ref)
            if (collisionRenderer.sortingLayerName.Equals("Default"))
            {
                m_colCounter += 1;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Checking if Object has a renderer
        if (collision.GetComponent<Renderer>())
        {
            Renderer collisionRenderer = collision.GetComponent<Renderer>();
            // Checking if Collider is on the same layer as sensor (Unity does not have a SortingLayer.Default ref)
            if (collisionRenderer.sortingLayerName.Equals("Default"))
            {
                m_colCounter -= 1;
            }
        }
    }
}
