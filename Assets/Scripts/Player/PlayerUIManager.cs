using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Network JOIN")]
    [SerializeField] bool startGameAsClient;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);   
    }
    private void Update()
    {
        if (startGameAsClient)
        {
            startGameAsClient = false;
            //first shut down so we can start as client
            NetworkManager.Singleton.Shutdown();
            //start newtwork as client
            NetworkManager.Singleton.StartClient();
        }
    }
}
