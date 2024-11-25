using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Interaxon.Libmuse;

public class TitleSettingsGUI : MonoBehaviour
{
    [Header("GUI Elements")] 
    public Dropdown museDropdown;
    public Button scanButton;
    public Button connectButton;
    public Button disconnectButton;

    private bool initialized;


    // Update is called once per frame
    void Update()
    {
        if (!this.initialized)
        {
            if (!string.IsNullOrEmpty(InteraxonInterfacer.Instance.museList))
            {
                this.ReceiveMuseList(InteraxonInterfacer.Instance.museList);
                this.initialized = true;
            }

            return;
        }

        CheckConnectButton();
    }

    private void CheckConnectButton()
    {
        var connectButtonText = this.connectButton.GetComponentInChildren<Text>();

        if (InteraxonInterfacer.Instance.currentConnectionState == ConnectionState.CONNECTED && museDropdown.options[museDropdown.value].text == InteraxonInterfacer.Instance.userMuse)
        {
            connectButtonText.text = "Connected";

            this.connectButton.interactable = false;
            this.disconnectButton.interactable = true;
        }
        else if (InteraxonInterfacer.Instance.currentConnectionState == ConnectionState.CONNECTING)
        {
            connectButtonText.text = "Connecting...";

            this.connectButton.interactable = false;
            this.disconnectButton.interactable = false;
        }
        else
        {
            connectButtonText.text = "Connect";

            this.connectButton.interactable = true;
            this.disconnectButton.interactable = false;
        }
    }

    private void ReceiveMuseList(string data)
    {
        // Convert string to list of muses and populate the dropdown menu.
        var muses = data.Split(' ').ToList();
        this.museDropdown.ClearOptions();
        this.museDropdown.AddOptions(muses);
    }

    public void ScanHeadsets()
    {
        //Debug.Log("startScanning");
        InteraxonInterfacer.Instance.startScanning();
        StartCoroutine(WaitBeforeFillingMuseList());
    }

    private IEnumerator WaitBeforeFillingMuseList()
    {
        float elapsedTime = 0f;

        Debug.Log("Waiting for connection");
        do
        {
            yield return new WaitForSeconds(0.25f);
            elapsedTime += 0.25f;

            ReceiveMuseList(InteraxonInterfacer.Instance.museList);
        } while (string.IsNullOrEmpty(InteraxonInterfacer.Instance.museList) && elapsedTime < 5f);

        if (elapsedTime >= 5f)
        {
            Debug.Log("No Muse could be found");
        }
        else
        {
            Debug.Log("Connection established");
        }
    }

    public void UserSelectedMuse()
    {
        InteraxonInterfacer.Instance.userSelectedMuse(this.museDropdown.options[this.museDropdown.value].text);
    }

    public void Connect()
    {
        Debug.Log("Connect");
        // If user just clicks connect without selecting a muse from the
        // dropdown menu, then connect to the one displayed in the dropdown.

        if (InteraxonInterfacer.Instance.userMuse == "")
        {
            InteraxonInterfacer.Instance.userMuse = this.museDropdown.options[0].text;
        }

        //Debug.Log("Connecting to " + InteraxonInterface.userMuse);
        InteraxonInterfacer.Instance.connect();
        this.scanButton.Select();
    }

    public void Disconnect()
    {
        Debug.Log("Disconnect");
        //this.muse.disconnect();
        InteraxonInterfacer.Instance.disconnect();
        this.scanButton.Select();
    }

    private void OnApplicationQuit()
    {
#if PLATFORM_STANDALONE_WIN
        if (InteraxonInterfacer.Instance.connected)
        {
            InteraxonInterfacer.Instance.disconnect();
        }
#endif
    }
}