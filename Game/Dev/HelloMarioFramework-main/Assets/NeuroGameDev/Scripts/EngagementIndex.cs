using UnityEngine;
using UnityEngine.UI;
using Interaxon.Libmuse;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Text engagement_num;
    
    void Start()
    {
        // _startPosition = transform.position;
    }
    
    void Update()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to start position
            engagement_num.text = "N/A";
            return;
        }

        float alpha_af7     = (float)InteraxonInterfacer.Instance.AlphaAbsolute.AF7;
        float alpha_af8     = (float)InteraxonInterfacer.Instance.AlphaAbsolute.AF8;
        float beta_af7      = (float)InteraxonInterfacer.Instance.BetaAbsolute.AF7;
        float beta_af8      = (float)InteraxonInterfacer.Instance.BetaAbsolute.AF8;
        float theta_af7     = (float)InteraxonInterfacer.Instance.ThetaAbsolute.AF7;
        float theta_af8     = (float)InteraxonInterfacer.Instance.ThetaAbsolute.AF8;

        float alpha_ = ( Mathf.Exp(alpha_af7) + Mathf.Exp(alpha_af8) ) / 2.0f;
        float beta_ = ( Mathf.Exp(beta_af7) + Mathf.Exp(beta_af8) ) / 2.0f;
        float theta_ = ( Mathf.Exp(theta_af7) + Mathf.Exp(theta_af8) ) / 2.0f;
        float engagement = beta_ / (alpha_ + theta_);
        engagement = Mathf.Round(engagement * 100f) * 0.01f;

        engagement_num.text = engagement.ToString();
    }


}
