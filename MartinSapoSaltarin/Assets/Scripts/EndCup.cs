using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCup : MonoBehaviour
{
    [SerializeField] GameObject ConfettiPrefab;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Instantiate(ConfettiPrefab, transform.position + Vector3.up* 3f, Quaternion.identity);
            audioSource.Play();
        }
    }
}
