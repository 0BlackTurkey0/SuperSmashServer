using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class Server : MonoBehaviour {

    public string IP;
    public ServerThread serverThread;
    public GameObject canvas;
    public Text info;

    void Start() {
        Application.runInBackground = true;
        IP = new WebClient().DownloadString("http://icanhazip.com"); ;
        canvas = GameObject.Find("Canvas");
        info = canvas.transform.GetChild(0).gameObject.GetComponent<Text>();
        serverThread = GetComponent<ServerThread>();
        serverThread.Listen();
        serverThread.StartConnect();
    }

	void Update() {
        info.text = "";
        info.text += "IPv4 : " + IP;
        info.text += "Port : 8000\n";
        info.text += "Player_Count : " + serverThread.clientCount.ToString() + "\n";
    }

    void OnApplicationQuit() {
        serverThread.StopConnect();
    }
}
