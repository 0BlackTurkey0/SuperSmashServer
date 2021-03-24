using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class ServerThread : MonoBehaviour{
    
    public string ip;
    public int port;
    public List<Client> players;
    public int clientCount;
    public int idGenerator;
    private Socket serverSocket, clientSocket;
    private Thread connectThread;

    public class Client {
        public Socket client;
        public GameObject Cat;
        public Player player;
        private Thread receiveThread;

        public Client(Socket client, int id) {
            this.client = client;
        }
        
        public void Receive() {
            if (receiveThread != null && receiveThread.IsAlive)
                return;
            else {
                receiveThread = new Thread(ReceiveData) {
                    IsBackground = true
                };
                receiveThread.Start();
            }
        }

        private void ReceiveData() {
            if (client.Connected) {
                byte[] data = new byte[1024];
                int len = client.Receive(data);
                char[] chars = new char[len];
                Decoder decoder = Encoding.ASCII.GetDecoder();
                decoder.GetChars(data, 0, len, chars, 0);
                String receiveMSG = new String(chars);
                String[] Message = receiveMSG.Split(' ');
                foreach (string msg in Message) {
                    switch (msg) {
                        case "Move_Right":
                            player.moveRight = true;
                            break;

                        case "Move_Left":
                            player.moveLeft = true;
                            break;

                        case "Jump":
                            player.jump = true;
                            break;

                        case "Light_Attack":
                            player.lightATK = true;
                            break;

                        case "Heavy_Attack":
                            player.heavyATK = true;
                            break;

                        case "Dodge":
                            player.dodge = true;
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    void Awake() {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    void Start() {
        clientCount = 0;
        idGenerator = 0;
        players = new List<Client>();
    }

    void Update() {
        int count = 0;
        while (count < clientCount) {
            if (IsConnected(players[count].client)) {
                players[count].Receive();
                count++;
            }
            else {
                Destroy(players[count].Cat);
                players.Remove(players[count]);
                Debug.Log("Say GoodBye");
                clientCount--;
            }
        }
        for (int i = clientCount; i < players.Count; i++) {
            GameObject Cat = Instantiate(Resources.Load("Cat") as GameObject, new Vector3(0, 0, 1), Quaternion.identity);
            players[i].Cat = Cat;
            players[i].player = Cat.GetComponent<Player>();
            players[i].player.ID = idGenerator;
            players[i].player.Name = "Player" + idGenerator;
            idGenerator++;
            Debug.Log("Link!");
            clientCount++;
        }
    }

    public bool IsConnected(Socket socket) {
        return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
    }

    public void Listen() {
        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
        serverSocket.Listen(4);
    }

    public void StartConnect() {
        connectThread = new Thread(Connect) {
            IsBackground = true
        };
        connectThread.Start();
    }

    private void Connect() {
        while (true) {
            clientSocket = serverSocket.Accept();
            players.Add(new Client(clientSocket, clientCount));
        }
    }

    public void StopConnect() {
        foreach(Client c in players) {
            c.client.Close();
        }
        serverSocket.Close();
    }

    public void SendData(Client target, string sendMSG) {
        if (target.client.Connected) {
            target.client.Send(Encoding.ASCII.GetBytes(sendMSG));
        }
    }
}