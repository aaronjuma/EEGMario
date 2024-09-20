using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelloMarioFramework;

public class BLEGameReceiver : MonoBehaviour
{
    public Player marioScript;
    public GameController controller;

    private Thread _receiveThread;
    private UdpClient _client;

    public static int Port = 2000;//port must be defined in BLE (for sending)
    public static bool IsConnected, CanConnect;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (CanConnect && !IsConnected)
        {
            Init();
            CanConnect = false;
        }
        else if (CanConnect && IsConnected)
        {
            CloseConnection();
        }
    }

    private void Init()
    {
        _receiveThread = new Thread(ReceiveData) {IsBackground = true};
        _receiveThread.Start();
        IsConnected = true;
    }

    private void ReceiveData()
    {
        _client = new UdpClient(Port);

        while (IsConnected)
        {
            try
            {
                var ip = new IPEndPoint(IPAddress.Loopback, 0);
                var udpdata = _client.Receive(ref ip);
                var data = Encoding.UTF8.GetString(udpdata);

                TranslateData(data);
            }

            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void TranslateData(string data)
    {
        string[] separators = { "[$]", "[$$]", "[$$$]", ",", ";", " " };

        var words = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        Debug.Log(data);

        for (var i = 0; i < words.Length; i++)
        {
            //getting the values for the game variables that require to be updated -> this will change from game to game, you need
            //to add them manually here

            //* Exerpong Example:
            //if (words[i] == "PaddleSize")
            //Control.Size = float.Parse(words[i + 1]);

            //if (words[i] == "BallSize")
            //    Ball.Size = float.Parse(words[i + 1]);

            //if (words[i] == "BallSpeed")
            //    Ball.Speed = float.Parse(words[i + 1]);

            if (words[i] == "MarioSpeed"){
                controller.changeMarioSpeed(1);
            }
            if (words[i] == "MarioJump"){
                controller.changeMarioJump(1);
            }
            if (words[i] == "FirebarSpeed"){
                controller.changeFirebar(1);
            }
            if (words[i] == "GoombaSpeed"){
                controller.changeGoomba(1);
            }
            
        }
    }

    private void CloseConnection()
    {
        if (IsConnected)
        {
            _receiveThread.Abort();
            _client.Close();
            IsConnected = false;
            CanConnect = false;
        }
    }

    private void OnDisable()
    {
        CloseConnection();
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }
}

