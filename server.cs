using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;

public class server : MonoBehaviour
{

    Socket serverSocket; //伺服器端socket

    Socket clientSocket; //客戶端socket

    IPEndPoint ipEnd; //偵聽埠

    string recvStr; //接收的字串

    string sendStr; //傳送的字串

    byte[] recvData = new byte[1024]; //接收的資料，必須為位元組

    byte[] sendData = new byte[1024]; //傳送的資料，必須為位元組

    int recvLen; //接收的資料長度

    Thread connectThread; //連線執行緒

    public Text InputFieldConcept; //輸入的文字

    public Text ChatConcept; //聊天的文字

    public Queue<string> temp = new Queue<string>(); //柱列，用來接受傳進來的訊息

    public bool isRecev;

    public InputField inputField;

    void InitSocket() //初始化
    {
        //定義偵聽埠,偵聽任何IP
        ipEnd = new IPEndPoint(IPAddress.Any, 7000);

        //定義套接字型別,在主執行緒中定義
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //連線
        serverSocket.Bind(ipEnd);

        //開始偵聽,最大10個連線
        serverSocket.Listen(10);

        //開啟一個執行緒連線，必須的，否則主執行緒卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));

        connectThread.Start();
    }

    void SocketConnet() //連線
    {
        if (clientSocket != null)
            clientSocket.Close();

        //控制檯輸出偵聽狀態
        print("Waiting for a client");

        //一旦接受連線，建立一個客戶端
        clientSocket = serverSocket.Accept();

        //獲取客戶端的IP和埠
        IPEndPoint ipEndClient = (IPEndPoint)clientSocket.RemoteEndPoint;

        //輸出客戶端的IP和埠
        print("Connect with " + ipEndClient.Address.ToString() + ":" + ipEndClient.Port.ToString());

        //連線成功則傳送資料
        sendStr = "Welcome to my server";

        SocketSend(sendStr);
    }

    void SocketReceive() //伺服器接收
    {
        //連線
        SocketConnet();

        //進入接收迴圈
        while (true)
        {
            //對data清零
            recvData = new byte[1024];

            //獲取收到的資料的長度
            recvLen = clientSocket.Receive(recvData);

            //如果收到的資料長度為0，則重連並進入下一個迴圈
            if (recvLen == 0)
            {
                SocketConnet();

                continue;
            }
            else
            {
                //輸出接收到的資料

                recvStr = "Client: " + Encoding.ASCII.GetString(recvData, 0, recvLen);

                print(recvStr); /*這裡是重點，用queue解決文字顯示的問題*/

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
        clientSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    public void SendMessage() /*發送訊息，放在Button*/
    {
        SocketSend(InputFieldConcept.text);

        ChatConcept.text += "Server: " + InputFieldConcept.text + "\n"; /*發送訊息，送一份給自己的聊天的文字*/

        inputField.text = ""; /*發送訊息，清空inputField*/
    }

    void Start()
    {
        InitSocket();
    }

    void Update()
    {

        if (isRecev == true)
        {
            ChatConcept.text += temp.Dequeue() + "\n";

            isRecev = false;
        }

    }

    void SocketQuit() //連線關閉
    {
        //先關閉客戶端
        if (clientSocket != null)
        {
            clientSocket.Close();
        }

        //再關閉執行緒
        if (connectThread != null)
        {
            connectThread.Interrupt();

            connectThread.Abort();
        }

        //最後關閉伺服器
        serverSocket.Close();

        print("diconnect");
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}