#if PLATFORM_IOS
using System;
using System.Runtime.InteropServices;

/*
 * This class implements the functionalities in LibmuseBridge.cs for iOS platform.
 */
public class LibmuseBridgeIos : LibmuseBridge
{

    //-------------------------------------------
    // extern C functions
    // These functions are defined in the objc++ (.mm) files

    [DllImport("__Internal")]
    private static extern void _startListening();

    [DllImport("__Internal")]
    private static extern void _stopListening();

    [DllImport("__Internal")]
    private static extern void _connect(IntPtr headband);

    [DllImport("__Internal")]
    private static extern void _disconnect();

    [DllImport("__Internal")]
    private static extern void _registerMuseListener(IntPtr obj, IntPtr method);

    [DllImport("__Internal")]
    private static extern void _registerConnectionListener(IntPtr obj, IntPtr method);

    [DllImport("__Internal")]
    private static extern void _registerDataListener(IntPtr obj, IntPtr method);

    [DllImport("__Internal")]
    private static extern void _registerArtifactListener(IntPtr obj, IntPtr method);

    [DllImport("__Internal")]
    private static extern void _listenForDataPacket(IntPtr packetType);

    [DllImport("__Internal")]
    public static extern IntPtr _getLibmuseVersion();


    //-------------------------------------------
    // Derived public methods
    // Many of these methods need to convert string to IntPtr before calling the extern c functions

    public override void startListening()
    {
        _startListening();
    }

    public override void stopListening()
    {
        _stopListening();
    }

    public override void connect(string headband)
    {
        var hband = Marshal.StringToHGlobalAuto(headband);
        _connect(hband);
        Marshal.FreeHGlobal(hband);
    }

    public override void disconnect()
    {
        _disconnect();
    }

    public override void registerMuseListener(string obj, string method)
    {
        var objec = Marshal.StringToHGlobalAuto(obj);
        var func = Marshal.StringToHGlobalAuto(method);
        _registerMuseListener(objec, func);
        Marshal.FreeHGlobal(objec);
        Marshal.FreeHGlobal(func);
    }

    public override void registerConnectionListener(string obj, string method)
    {
        var objec = Marshal.StringToHGlobalAuto(obj);
        var func = Marshal.StringToHGlobalAuto(method);
        _registerConnectionListener(objec, func);
        Marshal.FreeHGlobal(objec);
        Marshal.FreeHGlobal(func);
    }

    public override void registerDataListener(string obj, string method)
    {
        var objec = Marshal.StringToHGlobalAuto(obj);
        var func = Marshal.StringToHGlobalAuto(method);
        _registerDataListener(objec, func);
        Marshal.FreeHGlobal(objec);
        Marshal.FreeHGlobal(func);
    }

    public override void registerArtifactListener(string obj, string method)
    {
        var objec = Marshal.StringToHGlobalAuto(obj);
        var func = Marshal.StringToHGlobalAuto(method);
        _registerArtifactListener(objec, func);
        Marshal.FreeHGlobal(objec);
        Marshal.FreeHGlobal(func);
    }

    public override void listenForDataPacket(string packetType)
    {
        var pType = Marshal.StringToHGlobalAuto(packetType);
        _listenForDataPacket(pType);
        Marshal.FreeHGlobal(pType);
    }

    public override string getLibmuseVersion()
    {
        return Marshal.PtrToStringAuto(_getLibmuseVersion());
    }
}
#endif
