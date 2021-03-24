using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

public class Server : MonoBehaviour {

    public ServerThread serverThread;

    void Start() {
        Application.runInBackground = true;
        serverThread = GetComponent<ServerThread>();
        serverThread.Listen();
        serverThread.StartConnect();
    }

	void Update() {

	}

    void OnApplicationQuit() {
        serverThread.StopConnect();
    }
}
