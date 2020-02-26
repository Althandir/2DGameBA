using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D), typeof(Animator))]
public class Enemy : MonoBehaviour , IHitable
{
    [SerializeField] protected int m_health = 100;
    [SerializeField] protected int m_damage = 10;
    [SerializeField] protected int m_speed = 4;

    Animator m_animator;
    Rigidbody2D m_rb2D;
    EnemyDetectionArea m_detectionArea;

    protected Vector3 m_targetPosition;

    IEnumerator m_mainRoutine;
    IEnumerator m_moveRoutine;

    void Awake()
    {
        m_rb2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_detectionArea = transform.GetChild(0).GetComponent<EnemyDetectionArea>();

        m_mainRoutine = MainRoutine();
        m_moveRoutine = MoveRoutine();
    }

    void Start()
    {
        // Freeze Rotation
        m_rb2D.freezeRotation = true;
        // Start Enemyroutine
        StartCoroutine(m_mainRoutine);
        StartCoroutine(m_moveRoutine);
    }

    /// <summary>
    /// MovementRoutine for Enemys
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveRoutine()
    {
        while (true)
        {
            Vector2 thisPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(m_targetPosition.x, transform.position.y);
            float distance = Vector2.Distance(thisPos, targetPos);

            if (distance > 0)
            {
                transform.position = Vector2.MoveTowards(thisPos, targetPos, 0.01f * m_speed);
                RotateToTarget();
                m_animator.SetBool("isMoving", true);
            }
            else
            {
                m_animator.SetBool("isMoving", false);
            }

            yield return null;
        }

    }

    /// <summary>
    /// Function to rotate the sprite to face the target position. Can be overwritten.
    /// </summary>
    protected virtual void RotateToTarget()
    {
        // Debug.Log(m_targetPosition.x - transform.position.x); Gibt pos Zahl wenn rechts, neg wenn links
        if (m_targetPosition.x - transform.position.x > 0)
        {
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (m_targetPosition.x - transform.position.x < 0)
        {
            if (transform.localScale == new Vector3(-1, 1, 1))
            {
                transform.localScale = Vector3.one; 
            }
        }
    }

    /// <summary>
    /// Idle Behavior for an enemy. Can be overwritten.
    /// </summary>
    protected virtual void IdleBehavior()
    {
        // TODO: Add IdleBehavoir
        m_targetPosition = transform.position;
    }

    /// <summary>
    /// Behavior if the Player has entered the trigger once. Can be overwritten.
    /// </summary>
    protected virtual void AlertedBehavior()
    {
        // TODO: Add AlertedBehavoir
        m_targetPosition = Player.GetInstance.transform.position;
    }

    /// <summary>
    /// Main Coroutine of the Enemy
    /// </summary>
    IEnumerator MainRoutine()
    {
        while (m_health > 0)
        {
            if (!m_detectionArea.IsAlerted)
            {
                IdleBehavior();
                yield return new WaitForSeconds(1);
            }
            else 
            {
                if (m_animator.GetBool("isAlerted"))
                {
                    AlertedBehavior();
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    m_animator.SetBool("isAlerted", true);
                    yield return new WaitForSeconds(1);
                }
                yield return new WaitForSeconds(1);
            }


        }
    }



    public virtual void OnHit(int damage)
    {
        throw new System.NotImplementedException();
    }



}
