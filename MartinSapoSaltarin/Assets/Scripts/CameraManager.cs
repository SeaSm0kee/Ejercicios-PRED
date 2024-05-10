using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] float aheadDistance = 2;
    [SerializeField] float aboveDistance = 4;
    [SerializeField] float smoothTime = .3f;
    IGameManager gm;
    Camera cam;
    Vector3 velocity;


    // Start is called before the first frame update
    void Awake()
    {
        var gmOb = GameObject.Find("GameManager");
        if (gmOb == null) gmOb = GameObject.Find("NetGameManager");
        gm = gmOb.GetComponent<IGameManager>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        var player = gm.GetCurrentPlayer();
        if (player == null) return;
        var cameraPos = cam.transform.position;
        /*cameraPos.x = Mathf.Max(player.transform.position.x + aheadDistance, 1.5f);
        cameraPos.x = Mathf.Min(cameraPos.x, 38f);*/
        cameraPos.x = Mathf.Clamp(player.transform.position.x + aheadDistance, 1.5f, 38f);
        cameraPos.y = Mathf.Max(0, player.transform.position.y - aboveDistance);
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, cameraPos, ref velocity, smoothTime);
    }
}
