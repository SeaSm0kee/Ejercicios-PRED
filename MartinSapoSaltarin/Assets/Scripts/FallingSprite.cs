using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSprite : MonoBehaviour
{
    [SerializeField] float fallingSpeed = 3f;
    [SerializeField] float destructionHeight = -3f;
    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        pos.y -= fallingSpeed*Time.deltaTime;

        if(pos.y < destructionHeight) Destroy(gameObject);
        else transform.position = pos;
    }


}
