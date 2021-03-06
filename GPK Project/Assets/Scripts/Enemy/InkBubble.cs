﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkBubble : MonoBehaviour
{
    [SerializeField] private Transform bubbleDirection = null;
    public float bubbleSpeed;
    public float bubbleSize;
    public AnimationClip apparitionAnim;
    public GameObject blackExplosionPrefab;
    public GameObject colorExplosionPrefab;
    public AudioClip explosionSound;
    [HideInInspector] public Boss boss;

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool convertable;
    private bool isConverted;
    private Animator animator;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        direction = bubbleDirection.position - transform.position;
        direction.Normalize();
        yield return new WaitForSeconds(apparitionAnim.length);
        convertable = true;
        animator.SetBool("Flying", true);
        rb.velocity = direction * bubbleSpeed;
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction));
    }

    void FixedUpdate()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        if(Physics2D.OverlapCircle(transform.position, bubbleSize, LayerMask.GetMask("Obstacle")))
        {
            Explode();
        }
    }

    public void Explode()
    {
        Instantiate(isConverted ? colorExplosionPrefab : blackExplosionPrefab, transform.position, Quaternion.identity);
        if (isConverted)
            boss.AddRecoloration();
        else
            boss.LoseRecoloration();
        source.PlayOneShot(explosionSound);
        Destroy(gameObject);
    }

    public void Convert()
    {
        if (convertable)
        {
            isConverted = true;
            animator.SetBool("Converted", true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, bubbleDirection.position);
    }
}
