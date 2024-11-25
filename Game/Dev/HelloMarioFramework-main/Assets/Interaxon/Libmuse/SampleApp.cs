using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SampleApp : MonoBehaviour
{
    private bool initialized;
    private int apiLevel;
    private List<string> androidPermissions;

    //--------------------------------------
    // Public members that connects to UI components

    public Button startScanButton;
    public Button connectButton;
    public Button disconnectButton;
    public Dropdown museList;
    public Text dataText;
    public Text connectionText;

    //--------------------------------------
    // Public methods that gets called on UI events.

    public void startScanning()
    {
        Debug.Log("startScanning");
        // Must register at least MuseListeners before scanning for headbands.
        // Otherwise no callbacks will be triggered to get a notification.
        this.muse.startListening();
    }

    public void userSelectedMuse()
    {
        this.userPickedMuse = this.museList.options[this.museList.value].text;
        Debug.Log("Selected muse = " + this.userPickedMuse);
    }

    public void connect()
    {
        Debug.Log("connect");
        // If user just clicks connect without selecting a muse from the
        // dropdown menu, then connect to the one displayed in the dropdown.
        if (this.userPickedMuse == "")
        {
            this.userPickedMuse = this.museList.options[0].text;
        }
        Debug.Log("Connecting to " + this.userPickedMuse);
        this.muse.connect(this.userPickedMuse);
    }

    public void disconnect()
    {
        Debug.Log("disconnect");
        this.muse.disconnect();
    }

#if PLATFORM_STANDALONE_WIN
    private void OnApplicationQuit()
    {
        if (this.muse != null) {
            this.muse.disconnect();
        }
    }
#endif
    //--------------------------------------
    // Private Members

    private string userPickedMuse;
    private string dataBuffer;
    private string connectionBuffer;
    private LibmuseBridge muse;


    //--------------------------------------
    // Private Methods

    // Use this for initialization
    private void Start()
    {
        Debug.Log("Start");

        this.userPickedMuse = "";
        this.dataBuffer = "";
        this.connectionBuffer = "";
        this.androidPermissions = new List<string>();

#if PLATFORM_IOS
        muse = new LibmuseBridgeIos();
#elif PLATFORM_ANDROID
        muse = new LibmuseBridgeAndroid();
#elif PLATFORM_STANDALONE_WIN
        this.muse = new LibmuseBridgeWindows();
#endif
        Debug.Log("Libmuse version = " + this.muse.getLibmuseVersion());

#if PLATFORM_ANDROID
        if (IsMinimumApi(31))
        {
            this.androidPermissions.Add("android.permission.BLUETOOTH_CONNECT");
            this.androidPermissions.Add("android.permission.BLUETOOTH_SCAN");
        }
        else
        {
            this.androidPermissions.Add(Permission.FineLocation);
            this.androidPermissions.Add("android.permission.BLUETOOTH");
            this.androidPermissions.Add("android.permission.BLUETOOTH_ADMIN");
        }

        if (!HasPermissions)
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDenied += OnPermissionDenied;
            Permission.RequestUserPermissions(this.androidPermissions.ToArray(), callbacks);
        }
#endif
    }

    private bool HasPermissions
    {
        get
        {
            var allGranted = true;
#if PLATFORM_ANDROID
            foreach (var permission in this.androidPermissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    allGranted = false;
                    break;
                }
            }
#endif
            return allGranted;
        }
    }

    private bool IsMinimumApi(int minLevel)
    {
        if (this.apiLevel == 0) {
            var rx = new System.Text.RegularExpressions.Regex(@"API-(?<api>\d+)");
            var m = rx.Match(SystemInfo.operatingSystem);
            if (m.Success && int.TryParse(m.Groups["api"].Value, out this.apiLevel))
            {
                Debug.Log($"API level = {this.apiLevel}");
            }
        }

        return this.apiLevel >= minLevel;
    }
    
    private void OnPermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} permission granted");
        if (HasPermissions)
        {
            Initialize();
        }
    }

    private void OnPermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} permission denied");
    }

    private void Initialize()
    {
        if (!this.initialized)
        {
            if (muse == null) {
                return;
            }
            registerListeners();
            registerAllData();
            initialized = true;
        }
    }

    private void registerListeners()
    {
        this.muse.registerMuseListener(name, nameof(receiveMuseList));
        this.muse.registerConnectionListener(name, nameof(receiveConnectionPackets));
        this.muse.registerDataListener(name, nameof(receiveDataPackets));
        this.muse.registerArtifactListener(name, nameof(receiveArtifactPackets));
    }

    private void registerAllData()
    {
        // This will register for all the available data from muse headband
        // Comment out the ones you don't want
        this.muse.listenForDataPacket("ACCELEROMETER");
        this.muse.listenForDataPacket("GYRO");
        this.muse.listenForDataPacket("EEG");
        this.muse.listenForDataPacket("PPG");
        this.muse.listenForDataPacket("BATTERY");
        this.muse.listenForDataPacket("DRL_REF");
        this.muse.listenForDataPacket("ALPHA_ABSOLUTE");
        this.muse.listenForDataPacket("BETA_ABSOLUTE");
        this.muse.listenForDataPacket("DELTA_ABSOLUTE");
        this.muse.listenForDataPacket("THETA_ABSOLUTE");
        this.muse.listenForDataPacket("GAMMA_ABSOLUTE");
        this.muse.listenForDataPacket("ALPHA_RELATIVE");
        this.muse.listenForDataPacket("BETA_RELATIVE");
        this.muse.listenForDataPacket("DELTA_RELATIVE");
        this.muse.listenForDataPacket("THETA_RELATIVE");
        this.muse.listenForDataPacket("GAMMA_RELATIVE");
        this.muse.listenForDataPacket("ALPHA_SCORE");
        this.muse.listenForDataPacket("BETA_SCORE");
        this.muse.listenForDataPacket("DELTA_SCORE");
        this.muse.listenForDataPacket("THETA_SCORE");
        this.muse.listenForDataPacket("GAMMA_SCORE");
        this.muse.listenForDataPacket("HSI_PRECISION");
        this.muse.listenForDataPacket("ARTIFACTS");
    }

    //--------------------------------------
    // These listener methods update the buffer
    // The Update() per frame will display the data.

    private void receiveMuseList(string data)
    {
        // This method will receive a list of muses delimited by white space.
        Debug.Log("Found list of muses = " + data);

        // Convert string to list of muses and populate the dropdown menu.
        var muses = data.Split(' ').ToList();
        this.museList.ClearOptions();
        this.museList.AddOptions(muses);
    }

    private void receiveConnectionPackets(string data)
    {
        Debug.Log("Unity received connection packet: " + data);
        this.connectionBuffer = data;
    }

    private void receiveDataPackets(string data)
    {
        // Debug.Log("Unity received data packet: " + data);
        this.dataBuffer = data;
    }

    private void receiveArtifactPackets(string data)
    {
        // Debug.Log("Unity received artifact packet: " + data);
        this.dataBuffer = data;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!this.initialized && HasPermissions)
        {
            Initialize();
        }
#if PLATFORM_STANDALONE_WIN
        LibmuseBridgeWindows.InvokeDispatchQueue();
#endif
        //Debug.Log("Update");

        // Display the data in the UI Text field
        this.dataText.text = this.dataBuffer;
        this.connectionText.text = this.connectionBuffer;
    }
}
