using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeHead : MonoBehaviour
{
    [SerializeField] float movingRange = 10f;
    [SerializeField] float speed = 4f;

    Rigidbody2D rb;
    float initialPos;
    bool goingUp = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();   
        initialPos = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(goingUp && rb.position.y >= initialPos + movingRange/2)
        {
            goingUp = false;
            rb.velocity = new Vector2(0, -speed);
        }
        else if (!goingUp && rb.position.y <= initialPos - movingRange/2)
        {
            goingUp = true;
            rb.velocity = new Vector2(0, speed);
        }
        else if(Mathf.Abs(rb.velocity.y) <= 1f) //Ha chocado con algo
        {
            rb.velocity = new Vector2(0, goingUp?-speed:speed);
            goingUp = !goingUp;            
        }
    }
}
