#if PLATFORM_STANDALONE_WIN
using Interaxon.Libmuse;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using UnityEngine;

/*
 * This class implements the functionalities in LibmuseBridge.cs for Windows platform.
 */
public class LibmuseBridgeWindows : LibmuseBridge
{
    private Muse muse;
    private readonly MuseManager manager;
    private readonly MuseL museListener;
    private readonly ConnectionListener connectionListener;
    private readonly DataListener dataListener;
    private readonly Dictionary<string, MuseInfo> museMap;
    private readonly List<MuseDataPacketType> dataTypeToListen;
    private readonly Dictionary<string, MuseDataPacketType> stringToDataPacketType;
    private readonly Dictionary<MuseDataPacketType, string> dataPacketTypeToString;

    private readonly List<(string name, string method)> unityDataListener;
    private readonly List<(string name, string method)> unityConnectionListener;
    private readonly List<(string name, string method)> unityMuseListener;
    private readonly List<(string name, string method)> unityArtifactListener;
    private static readonly Queue<UnityMessage> dispatchQueue = new Queue<UnityMessage>();
    private static readonly object mapLock = new object();

    public LibmuseBridgeWindows()
    {
        this.manager = MuseManager.GetInstance();
        this.connectionListener = new ConnectionListener(this);
        this.dataListener = new DataListener(this);
        this.museListener = new MuseL(this);
        this.manager.SetMuseListener(this.museListener);

        this.unityDataListener = new List<(string, string)>();
        this.unityArtifactListener = new List<(string, string)>();
        this.unityConnectionListener = new List<(string, string)>();
        this.unityMuseListener = new List<(string, string)>();
        this.museMap = new Dictionary<string, MuseInfo>();
        this.dataTypeToListen = new List<MuseDataPacketType>();
        this.stringToDataPacketType = new Dictionary<string, MuseDataPacketType>();
        this.dataPacketTypeToString = new Dictionary<MuseDataPacketType, string>();
        InitMaps();
    }

    public override void startListening()
    {
        Debug.Log($"startListening with {this.unityMuseListener.Count} listeners");

        if (this.unityMuseListener.Count > 0)
        {
            this.manager.StopListening();
            this.manager.StartListening();
        }
        else
        {
            Debug.LogError("Please register a muse listener before start listening for headbands, otherwise you won't get any callbacks");
        }
    }

    public override void stopListening()
    {
        this.manager.StopListening();
    }

    public override string getLibmuseVersion()
    {
        return Native.LibmuseVersion;
    }

    public override void connect(string headband)
    {
        lock (mapLock)
        {
            if (this.museMap.ContainsKey(headband))
            {
                this.manager.StopListening();
                this.muse = Muse.GetInstance(this.museMap[headband]);
                this.muse.UnregisterAllListeners();
                this.muse.RegisterConnectionListener(this.connectionListener);

                // Register for all the data packet types requested by unity.
                foreach (var data in this.dataTypeToListen)
                {
                    this.muse.RegisterDataListener(this.dataListener, data);
                }

                this.muse.SetPreset(MusePreset.PRESET_51); // Requesting PPG and 4 channels of EEG

                // Initiate a connection to the headband and stream the data asynchronously.
                this.muse.RunAsynchronously();
            }
            else
            {
                Debug.LogError("Chosen muse to connect to couldn't be found. Make sure you scan for headbands first");
            }
        }
    }

    public override void disconnect()
    {
        this.muse?.Disconnect();
    }

    public override void listenForDataPacket(string packetType)
    {
        if (this.stringToDataPacketType.ContainsKey(packetType))
        {
            this.dataTypeToListen.Add(this.stringToDataPacketType[packetType]);
        }
        else
        {
            Debug.LogError("Invalid input string data packet type");
        }
    }

    public override void registerMuseListener(string obj, string method)
    {
        this.unityMuseListener.Add((obj, method));
    }

    public override void registerConnectionListener(string obj, string method)
    {
        this.unityConnectionListener.Add((obj, method));
    }

    public override void registerDataListener(string obj, string method)
    {
        this.unityDataListener.Add((obj, method));
    }

    public override void registerArtifactListener(string obj, string method)
    {
        this.unityArtifactListener.Add((obj, method));
    }

    public void MuseListChanged()
    {
        var list = this.manager.GetMuses();
        var muses = string.Empty;
        lock (mapLock)
        {
            foreach (var m in list)
            {
                if (!this.museMap.ContainsKey(m.Name))
                {
                    this.museMap.Add(m.Name, m);
                }
                muses += m.Name + " ";
            }
        }

        foreach (var (name, method) in this.unityMuseListener)
        {
            UnitySendMessage(name, method, muses);
        }
    }

    public static void InvokeDispatchQueue()
    {
        // this needs to be called from Update() to run on the main thread
        lock (dispatchQueue)
        {
            while (dispatchQueue.Count > 0)
            {
                var msg = dispatchQueue.Dequeue();
                // Debug.Log($"Invoking {msg.methodName}");
                msg.action.Invoke(msg.gameObjectName, msg.methodName, msg.args);
            }
        }
    }

    private void UnitySendMessage(string gameObjectName, string methodName, object args)
    {
        var msg = new UnityMessage()
        {
            gameObjectName = gameObjectName,
            methodName = methodName,
            args = args,
            action = (name, method, obj) =>
            {
                GameObject.Find(name)?.SendMessage(method, obj);
            }
        };
        lock (dispatchQueue)
        {
            dispatchQueue.Enqueue(msg);
        }
    }
    
    public void ReceiveMuseConnectionPacket(MuseConnectionPacket packet)
    {
        var args = JsonConvert.SerializeObject(packet);
        Debug.Log(args);
        foreach (var (name, method) in this.unityConnectionListener)
        {
            UnitySendMessage(name, method, args);
        }
    }

    public void ReceiveMuseDataPacket(MuseDataPacket packet)
    {
        var args = JsonConvert.SerializeObject(packet);
        // Debug.Log(args);

        foreach (var (name, method) in this.unityDataListener)
        {
            UnitySendMessage(name, method, args);
        }
    }

    public void ReceiveMuseArtifactPacket(MuseArtifactPacket packet)
    {
        var args = JsonConvert.SerializeObject(packet);
        foreach (var (name, method) in this.unityArtifactListener)
        {
            UnitySendMessage(name, method, args);
        }
    }

    private void InitMaps()
    {
        this.stringToDataPacketType.Add("ACCELEROMETER", MuseDataPacketType.ACCELEROMETER);
        this.stringToDataPacketType.Add("GYRO", MuseDataPacketType.GYRO);
        this.stringToDataPacketType.Add("EEG", MuseDataPacketType.EEG);
        this.stringToDataPacketType.Add("QUANTIZATION", MuseDataPacketType.QUANTIZATION);
        this.stringToDataPacketType.Add("BATTERY", MuseDataPacketType.BATTERY);
        this.stringToDataPacketType.Add("DRL_REF", MuseDataPacketType.DRL_REF);
        this.stringToDataPacketType.Add("ALPHA_ABSOLUTE", MuseDataPacketType.ALPHA_ABSOLUTE);
        this.stringToDataPacketType.Add("BETA_ABSOLUTE", MuseDataPacketType.BETA_ABSOLUTE);
        this.stringToDataPacketType.Add("DELTA_ABSOLUTE", MuseDataPacketType.DELTA_ABSOLUTE);
        this.stringToDataPacketType.Add("THETA_ABSOLUTE", MuseDataPacketType.THETA_ABSOLUTE);
        this.stringToDataPacketType.Add("GAMMA_ABSOLUTE", MuseDataPacketType.GAMMA_ABSOLUTE);
        this.stringToDataPacketType.Add("ALPHA_RELATIVE", MuseDataPacketType.ALPHA_RELATIVE);
        this.stringToDataPacketType.Add("BETA_RELATIVE", MuseDataPacketType.BETA_RELATIVE);
        this.stringToDataPacketType.Add("DELTA_RELATIVE", MuseDataPacketType.DELTA_RELATIVE);
        this.stringToDataPacketType.Add("THETA_RELATIVE", MuseDataPacketType.THETA_RELATIVE);
        this.stringToDataPacketType.Add("GAMMA_RELATIVE", MuseDataPacketType.GAMMA_RELATIVE);
        this.stringToDataPacketType.Add("ALPHA_SCORE", MuseDataPacketType.ALPHA_SCORE);
        this.stringToDataPacketType.Add("BETA_SCORE", MuseDataPacketType.BETA_SCORE);
        this.stringToDataPacketType.Add("DELTA_SCORE", MuseDataPacketType.DELTA_SCORE);
        this.stringToDataPacketType.Add("THETA_SCORE", MuseDataPacketType.THETA_SCORE);
        this.stringToDataPacketType.Add("GAMMA_SCORE", MuseDataPacketType.GAMMA_SCORE);
        this.stringToDataPacketType.Add("HSI_PRECISION", MuseDataPacketType.HSI_PRECISION);
        this.stringToDataPacketType.Add("ARTIFACTS", MuseDataPacketType.ARTIFACTS);
        this.stringToDataPacketType.Add("PPG", MuseDataPacketType.PPG);

        // init the reverse map
        foreach (var key in this.stringToDataPacketType.Keys)
        {
            this.dataPacketTypeToString.Add(this.stringToDataPacketType[key], key);
        }
    }

    private class MuseL : IMuseListener
    {
        private readonly LibmuseBridgeWindows parent;
        public MuseL(LibmuseBridgeWindows parent)
        {
            this.parent = parent;
        }

        public void MuseListChanged()
        {
            this.parent.MuseListChanged();
        }
    }

    private class ConnectionListener : IMuseConnectionListener
    {
        private readonly LibmuseBridgeWindows parent;
        public ConnectionListener(LibmuseBridgeWindows parent)
        {
            this.parent = parent;
        }

        public void ReceiveMuseConnectionPacket(MuseConnectionPacket p, Muse muse)
        {
            this.parent.ReceiveMuseConnectionPacket(p);
        }
    }

    private class DataListener : IMuseDataListener
    {
        private readonly LibmuseBridgeWindows parent;
        public DataListener(LibmuseBridgeWindows parent)
        {
            this.parent = parent;
        }

        public void ReceiveMuseDataPacket(MuseDataPacket p, Muse muse)
        {
            this.parent.ReceiveMuseDataPacket(p);
        }

        public void ReceiveMuseArtifactPacket(MuseArtifactPacket p, Muse muse)
        {
            this.parent.ReceiveMuseArtifactPacket(p);
        }
    }

    private class UnityMessage
    {
        public string gameObjectName;
        public string methodName;
        public object args;
        public Action<string, string, object> action;
    }
}
#endif
