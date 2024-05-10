using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    [SerializeField]
    GameObject playerPrefab;
    GameObject player;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Instantiate(playerPrefab);
    }

    public void OnPlayerDeath(ulong clientId = 0)
    {
        audioSource.Play();
        player = Instantiate(playerPrefab);
    }

    public GameObject GetCurrentPlayer()
    {
        return player;
    }
}
