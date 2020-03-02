using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General Movement System for the Player
/// Base Idea from: Unity Asset Store Bandits - Pixel Art from Sven Thole
/// </summary>
[RequireComponent(typeof(Rigidbody2D),typeof(Animator), typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour, IPlayerControlls
{
    [SerializeField] float m_speed = 1.0f;
    [SerializeField] float m_jumpForce = 1.0f;
    [SerializeField] PlayerGroundSensor m_GroundSensor = null;

    Rigidbody2D m_rb2D;
    Animator m_animator;
    AudioSource m_audioSource;

    float m_inputHorizontal = 0.0f;
    bool m_jumpPressed = false;
    Vector3 m_usedScale;

    void Awake()
    {
        // Get all required Components
        m_rb2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_audioSource = transform.GetChild(2).GetComponent<AudioSource>();
        // save usedScale for rotation
        m_usedScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void Start()
    {
        // Add Listener on PlayerDeadEvent
        Player.GetInstance.OnPlayerDeadEvent.AddListener(PlayerDeathHandler);
        // Freezing Rotation 
        m_rb2D.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Get Input
        m_inputHorizontal = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            m_jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        // Handle horizontal movement
        m_rb2D.velocity = new Vector2(m_inputHorizontal * m_speed, m_rb2D.velocity.y);
        // Rotate Player depending of direction
        // Moving right
        if (m_inputHorizontal > 0)
        {
            transform.localScale = new Vector3(m_usedScale.x * (-1), m_usedScale.y, m_usedScale.z);
        } 
        // Moving left
        else if (m_inputHorizontal < 0)
        {
            transform.localScale = m_usedScale;
        }
        // Activate Moving Animator
        if (Mathf.Abs(m_rb2D.velocity.x) > 0)
        {
            m_animator.SetBool("isMoving", true);
        }
        else
        {
            m_animator.SetBool("isMoving", false);
        }
        // Activate Sound when moving
        if (m_animator.GetBool("isMoving") && m_GroundSensor.IsGrounded())
        {
            if (!m_audioSource.clip)
            {
                Debug.LogError("Missing Moving Audioclip in: " + gameObject.name);
            }
            else if (!m_audioSource.isPlaying)
            {
                m_audioSource.Play();
            }
        }
        else
        {
            m_audioSource.Stop();
        }

        // Handle jump
        if (m_jumpPressed && m_GroundSensor.IsGrounded())
        {
            m_rb2D.velocity = new Vector2(m_rb2D.velocity.x, m_jumpForce);
            m_jumpPressed = false;
        }
        else
        {
            m_jumpPressed = false;
        }

        StartCoroutine(AnimationJumpCheck());

    }

    /// <summary>
    /// Checks if the player is in air and activates Jump Animation if nessesary.
    /// If not in air the player jump animation will be disabled.
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimationJumpCheck()
    {
        yield return new WaitForSeconds(0.5f);
        if (m_GroundSensor.IsGrounded())
        {
            m_animator.SetBool("isGrounded", true);
        }
        else if (Mathf.Abs(m_rb2D.velocity.y) > 0)
        {
            if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Jump"))
            {
                m_animator.SetBool("isGrounded", false);
                m_animator.SetTrigger("Jumping");
            }
        }
    }

    /// <summary>
    /// Method to disable this script & starting the dying animation in case the player dies
    /// </summary>
    public void PlayerDeathHandler()
    {
        m_animator.SetBool("isDead", true);
        this.enabled = false;
    }
}
