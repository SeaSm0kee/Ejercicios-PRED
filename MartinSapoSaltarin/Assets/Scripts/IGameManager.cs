using UnityEngine;

public interface IGameManager
{
    public void OnPlayerDeath(ulong clientId = 0);

    public GameObject GetCurrentPlayer();

    public void SetClientPlayer(GameObject player, ulong clientId) { }
}