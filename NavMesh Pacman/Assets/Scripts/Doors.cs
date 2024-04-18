using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    [SerializeField] private GameObject nextDoor;
    private float positionXChild;
    void Start()
    {
        positionXChild = nextDoor.transform.GetChild(0).transform.position.x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.position = new Vector3(positionXChild, other.gameObject.transform.position.y, other.gameObject.transform.position.z);
            if (gameObject.CompareTag("LeftDoor"))
                other.gameObject.transform.Rotate(0, -90, 0);
            else if (gameObject.CompareTag("RightDoor"))
                other.gameObject.transform.Rotate(0, 90, 0);
        }
    }
}
