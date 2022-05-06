using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;


public class client : MonoBehaviour
{

    Socket serverSocket; //伺服器端socket

    IPAddress ip; //主機ip

    IPEndPoint ipEnd;

    string recvStr; //接收的字串

    string sendStr; //傳送的字串

    byte[] recvData = new byte[1024]; //接收的資料，必須為位元組

    byte[] sendData = new byte[1024]; //傳送的資料，必須為位元組

    int recvLen; //接收的資料長度

    Thread connectThread; //連線執行緒

    public Text InputFieldConcept; //輸入的文字

    public Text ChatConcept; //聊天的文字

    public Queue<string> temp = new Queue<string>(); //柱列，用來接受傳進來的訊息


    //初始化
    void InitSocket()
    {
        //定義伺服器的IP和埠，埠與伺服器對應

        ip = IPAddress.Parse("127.0.0.1"); //可以是區域網或網際網路ip，此處是本機

        ipEnd = new IPEndPoint(ip, 7000);

        //開啟一個執行緒連線，必須的，否則主執行緒卡死

        connectThread = new Thread(new ThreadStart(SocketReceive));

        connectThread.Start();
    }

    void SocketConnet()
    {
        if (serverSocket != null)
        {
            serverSocket.Close();
        }

        //定義套接字型別,必須在子執行緒中定義

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        print("ready to connect");

        //連線

        serverSocket.Connect(ipEnd);

        //輸出初次連線收到的字串

        recvLen = serverSocket.Receive(recvData);

        recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);

        print(recvStr);
    }

    void SocketReceive()
    {
        SocketConnet();

        //不斷接收伺服器發來的資料

        while (true)
        {
            recvData = new byte[1024];

            recvLen = serverSocket.Receive(recvData);

            if (recvLen == 0)
            {
                SocketConnet();

                continue;
            }
            else
            {
                recvStr = "Server: " + Encoding.ASCII.GetString(recvData, 0, recvLen);

                print(recvStr);

                temp.Enqueue(recvStr);

                isRecev = true;

            }
        }
    }

    void SocketSend(string sendStr)
    {
        //清空傳送快取
        sendData = new byte[1024];
        //資料型別轉換
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //傳送
        serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    public InputField inputField;

    public void SendMessage() /*發送訊息，放在Button*/
    {
        SocketSend(InputFieldConcept.text);

        ChatConcept.text += "Client: " + InputFieldConcept.text + "\n"; /*發送訊息，送一份給自己的聊天的文字*/

        inputField.text = ""; /*發送訊息，清空inputField*/
    }

    // Use this for initialization
    void Start()
    {
        InitSocket();
    }

    public bool isRecev;


    void Update()
    {

        if (isRecev == true)
        {
            ChatConcept.text += temp.Dequeue() + "\n";

            isRecev = false;
        }

    }

    void SocketQuit()
    {
        //關閉執行緒
        if (connectThread != null)
        {
            connectThread.Interrupt();

            connectThread.Abort();
        }

        //最後關閉伺服器
        if (serverSocket != null)
            serverSocket.Close();

        print("diconnect");
    }

    //程式退出則關閉連線
    void OnApplicationQuit()
    {
        SocketQuit();
    }

}