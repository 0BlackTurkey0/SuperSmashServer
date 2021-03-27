using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
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
    private string sendMSG;

    public class Client {
        public Socket client;
        public GameObject Cat;
        public Player player;
        private Thread receiveThread;
        public int commandCount;
        public string commamd;

        public Client(Socket client) {
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
            if (client.Connected && player != null) {
                commamd = "";
                commandCount = 0;
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
                            commamd += "Move_Right ";
                            commandCount++;
                            break;

                        case "Move_Left":
                            player.moveLeft = true;
                            commamd += "Move_Left ";
                            commandCount++;
                            break;

                        case "Jump":
                            player.jump = true;
                            commamd += "Jump ";
                            commandCount++;
                            break;

                        case "Light_Attack":
                            player.lightATK = true;
                            commamd += "Light_Attack ";
                            commandCount++;
                            break;

                        case "Heavy_Attack":
                            player.heavyATK = true;
                            commamd += "Heavy_Attack ";
                            commandCount++;
                            break;

                        case "Dodge":
                            player.dodge = true;
                            commamd += "Dodge ";
                            commandCount++;
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
        SendData();
        int count = 0;
        while (count < clientCount) {
            if (IsConnected(players[count].client)) {
                players[count].Receive();
                count++;
            }
            else {
                Destroy(players[count].Cat);
                players[count].client.Close();
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
            players.Add(new Client(clientSocket));
        }
    }

    public void StopConnect() {
        foreach(Client c in players) {
            c.client.Close();
        }
        serverSocket.Close();
    }

    public void SendData() {
        sendMSG = (clientCount.ToString() + " ");
        for (int i = 0; i < clientCount; i++) {
            sendMSG += (players[i].player.ID.ToString() + " ");
            sendMSG += (players[i].Cat.transform.position.x.ToString() + " ");
            sendMSG += (players[i].Cat.transform.position.y.ToString() + " ");
            if (IsConnected(players[i].client)) {
                sendMSG += (players[i].commandCount.ToString() + " ");
                sendMSG += players[i].commamd;
            }
            else {
                sendMSG += "1 Destroy ";
            }

        }
        for (int i = 0; i < clientCount; i++) {
            if (IsConnected(players[i].client))
                players[i].client.Send(Encoding.ASCII.GetBytes(players[i].player.ID.ToString() + " " + sendMSG));
            Debug.Log(sendMSG);
        }
    }
}