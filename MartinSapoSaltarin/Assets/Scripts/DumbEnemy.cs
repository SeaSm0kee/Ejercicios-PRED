using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbEnemy : MonoBehaviour
{
    [SerializeField] bool moveEnemy = false;
    [SerializeField] float speed = 2f;
    
    Rigidbody2D rb;
    bool facingRight = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    // Start is called before the first frame update
    void Start()
    {
        if(moveEnemy) rb.velocity = new Vector2(-speed, 0f);
    }

    void FixedUpdate()
    {
        if(moveEnemy)
        {
            if(Math.Abs(rb.velocity.x) < 0.1f)
            {
                rb.velocity = new Vector2(facingRight?-speed:speed,0f);
                transform.localScale = new Vector3(-transform.localScale.x, 1f,1f);
                facingRight = !facingRight;
            }
        }      
    } 
    
}
