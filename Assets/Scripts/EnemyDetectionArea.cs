using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDetectionArea : MonoBehaviour
{
    UnityEvent m_playerDetectedEvent;
    bool m_isAlerted = false;

    public UnityEvent PlayerDetectedEvent { get => m_playerDetectedEvent;}
    public bool IsAlerted { get => m_isAlerted; }

    private void Awake()
    {
        m_playerDetectedEvent = new UnityEvent();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() && !m_isAlerted)
        {
            m_playerDetectedEvent.Invoke();
            m_isAlerted = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
