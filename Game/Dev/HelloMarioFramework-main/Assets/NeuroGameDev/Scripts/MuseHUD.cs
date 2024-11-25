using UnityEngine;
using UnityEngine.UI;
using Interaxon.Libmuse;

public class MuseHUD : MonoBehaviour
{
    public Image hsiTP9Image;
    public Image hsiAF7Image;
    public Image hsiAF8Image;
    public Image hsiTP10Image;

    public Text batteryText;

    void Update()
    {
        UpdateGUI();
    }

    private void UpdateGUI()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED)
        {
            ClearGUI();
            return;
        }

        // Update Headband Fit
        UpdateHSI(hsiTP9Image, InteraxonInterfacer.Instance.HeadbandFit.fitTP9);
        UpdateHSI(hsiAF7Image, InteraxonInterfacer.Instance.HeadbandFit.fitAF7);
        UpdateHSI(hsiAF8Image, InteraxonInterfacer.Instance.HeadbandFit.fitAF8);
        UpdateHSI(hsiTP10Image, InteraxonInterfacer.Instance.HeadbandFit.fitTP10);

        // Update battery text
        batteryText.text =
            $"{(InteraxonInterfacer.Instance.Battery.level > 0 ? InteraxonInterfacer.Instance.Battery.level.ToString() : "WAIT")}";
    }

    private void UpdateHSI(Image hsiImage, int fit)
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED ||
            !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            hsiImage.color = Color.black;
            return;
        }

        switch (fit)
        {
            case 1:
                hsiImage.color = new Color(0.5f, 0.95f, 0.5f);
                break;
            case 2:
                hsiImage.color = new Color(0.6f, 0.5f, 0.25f);
                break;
            case 4:
                hsiImage.color = new Color(0.5f, 0.0f, 0.0f);
                break;
            default:
                hsiImage.color = Color.black;
                break;
        }
    }

    private void ClearGUI()
    {
        batteryText.text = "OFF";

        UpdateHSI(hsiTP9Image, 0);
        UpdateHSI(hsiAF7Image, 0);
        UpdateHSI(hsiAF8Image, 0);
        UpdateHSI(hsiTP10Image, 0);
    }
}