using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class Server : MonoBehaviour {
    
    public bool init;
    public ServerThread serverThread;
    public GameObject canvas;
    public Text info;

    void Start() {
        Application.runInBackground = true;
        init = false;
        canvas = GameObject.Find("Canvas");
        info = canvas.transform.GetChild(0).gameObject.GetComponent<Text>();
        serverThread = GetComponent<ServerThread>();
    }

	void Update() {
        if (serverThread.connected) {
            if (!init) {
                serverThread.StartConnect();
                init = true;
            }
            info.text = "";
            info.text += "IPv4 : " + serverThread.ip + "\n";
            info.text += "Port : 8000\n";
            info.text += "Player_Count : " + serverThread.clientCount.ToString() + "\n";
        }
    }

    void OnApplicationQuit() {
        serverThread.StopConnect();
    }
}
