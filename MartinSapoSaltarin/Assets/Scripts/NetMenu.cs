using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NetMenu : MonoBehaviour
{
    [SerializeField] private GameObject buttonServer;
    [SerializeField] private GameObject buttonClient;
    [SerializeField] private GameObject enterCode;
    private TMP_InputField inputField;
    // Start is called before the first frame update
    private void Awake()
    {
        enterCode.SetActive(false);
        inputField = enterCode.transform.GetChild(1).GetComponent<TMP_InputField>();
    }
    public void OnStartServer()
    {
        //NetworkManager.Singleton.StartServer();
        NetData.Instance.IsServer = true;
        SceneManager.LoadScene(1);
    }

    public void OnJoinAsAClient()
    {
        buttonServer.SetActive(false);
        buttonClient.SetActive(false);
        enterCode.SetActive(true);
        //NetworkManager.Singleton.StartClient(); Esto ya estaba comentado de antes
        //NetData.Instance.IsServer = false;
        //SceneManager.LoadScene(1);
    }

    public void OnJoinLobby()
    {
        NetData.Instance.IsLobby = true;
        SceneManager.LoadScene(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NetData.Instance.JoinCode = inputField.text.Substring(0, 6);
            NetData.Instance.IsServer = false;
            SceneManager.LoadScene(1);
        }
    }
}
