#if PLATFORM_ANDROID
using UnityEngine;

/*
 * This class implements the functionalities in LibmuseBridge.cs for Android platform.
 */
public class LibmuseBridgeAndroid : LibmuseBridge
{
    private readonly AndroidJavaClass unityJavaClass;
    private readonly AndroidJavaObject unityMainActivity;
    private readonly AndroidJavaObject libmuseObj;

    public LibmuseBridgeAndroid()
    {
        this.unityJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        this.unityMainActivity = this.unityJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

        // Used to call member method
        this.libmuseObj = new AndroidJavaObject("com.choosemuse.example.unity.LibmuseUnityAndroid", this.unityMainActivity);
    }

    public override void startListening()
    {
        this.libmuseObj.Call("startListening");
    }

    public override void stopListening()
    {
        this.libmuseObj.Call("stopListening");
    }

    public override void connect(string headband)
    {
        this.libmuseObj.Call("connect", headband);
    }

    public override void disconnect()
    {
        this.libmuseObj.Call("disconnect");
    }

    public override void registerMuseListener(string obj, string method)
    {
        this.libmuseObj.Call("registerMuseListener", obj, method);
    }

    public override void registerConnectionListener(string obj, string method)
    {
        this.libmuseObj.Call("registerConnectionListener", obj, method);
    }

    public override void registerDataListener(string obj, string method)
    {
        this.libmuseObj.Call("registerDataListener", obj, method);
    }

    public override void registerArtifactListener(string obj, string method)
    {
        this.libmuseObj.Call("registerArtifactListener", obj, method);
    }

    public override void listenForDataPacket(string packetType)
    {
        this.libmuseObj.Call("listenForDataPacket", packetType);
    }

    public override string getLibmuseVersion()
    {
        return this.libmuseObj.Call<string>("getLibmuseVersion");
    }
}
#endif