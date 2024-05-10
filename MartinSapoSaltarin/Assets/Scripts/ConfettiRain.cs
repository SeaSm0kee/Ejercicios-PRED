using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfettiRain : MonoBehaviour
{
    [SerializeField] GameObject confettiPrefab;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnConfetti", 0f, 0.01f);
        Invoke("EndLevel", 3.0f);

    }

    void SpawnConfetti()
    {
        var confetti = Instantiate(confettiPrefab, transform.position + Vector3.right*Random.Range(-2f,2f), Quaternion.identity);
        confetti.transform.SetParent(gameObject.transform);
    }

    // Update is called once per frame
    void EndLevel()
    {
        CancelInvoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
