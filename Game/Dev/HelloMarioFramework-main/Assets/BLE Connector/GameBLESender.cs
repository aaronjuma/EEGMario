using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HelloMarioFramework; 

public class GameBLESender : MonoBehaviour
{
    public static GameBLESender Instance { get; set; }
    private string _ip;
    private const int Port = 1210;
    private IPEndPoint _remoteEndPoint;
    private UdpClient _client;

    public static bool IsGameSending;

    // public GameController controller;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _ip = LocalIpAdress();
        Init();
    }

    private void Update()
    {
        SendString("GameVariable,Difficulty,float," + 0f + ",1,5");
        // SendString("GameVariable,MarioJump,float," + 0f + ",1,3");
        // SendString("GameVariable,GoombaSpeed,float," + 0f + ",2,6");
        // SendString("GameVariable,FirebarSpeed,float," + 0f + ",2,6");
    }
	        
    private void Init()
    {
        _remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), Port);
        _client = new UdpClient();
    }

	//Invoke this function whenever you want to send a GameVariable update to the BLE
	//Example: GameBLESender.Instance.SendString("GameVariable,BallSpeed,float," + speed + "," + "1,20");
	public void SendString(string message)
    {
        try
        {
            if (message != "")
            {
                var data = Encoding.UTF8.GetBytes(message);
                _client.Send(data, data.Length, _remoteEndPoint);
            }
        }

        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    private string LocalIpAdress()
    {
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            return null;
        }

        var localIp = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIp = ip.ToString();
            }
        }
        return localIp;
    }

    private void OnApplicationQuit()
    {
        if (_client != null)
            _client.Close();
    }

}

