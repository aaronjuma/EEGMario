using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HelloMarioFramework;
using Interaxon.Libmuse;

public class SurvivalController : MonoBehaviour
{

    [SerializeField] private GameObject goombaPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform goombaParent;
    [SerializeField] private Transform coinParent;
    [SerializeField] private float goombaChaseDistance = 2f;
    [SerializeField] private int goombaCount = 15;
    [SerializeField] private int coinCount = 5;

    [SerializeField] private GameObject bg;
    [SerializeField] private GameObject bgText;
    [SerializeField] private GameObject bgSpeed;
    [SerializeField] private GameObject bgJump;
    [SerializeField] private GameObject bgDensity;
    [SerializeField] private GameObject bgGoomba;
    [SerializeField] public Text engagement_num;
    [SerializeField] public GameObject biofeedbackText;
    [SerializeField] public Text meanText;
    [SerializeField] public Text stdText;

    public int marioSpeedDifficulty;
    public int marioJumpDifficulty;
    public int densityDifficulty;
    public int goombaDifficulty;
    public int difficulty;

    public Player mario;
    private float goombasSpeed = 1f;
    private float prevGoombaSpeed = 1f;
    private float mean;
    private float std;

    public int baselineDuration = 180; //180 Seconds

    [SerializeField] public bool debugMode;
    [SerializeField] private Slider baselineSlider;
    private float baselineTimeValue;
    float currentEngagement = 0;
    float prevEngagement;




    void Awake() {
        SurvivalData.NullCheck();
        baselineSlider.maxValue = baselineDuration;
        baselineSlider.minValue = 0;
    }


    // Start is called before the first frame update
    void Start() {

        marioSpeedDifficulty = SurvivalData.save.GetMarioSpeed();
        marioJumpDifficulty = SurvivalData.save.GetMarioJump();
        densityDifficulty = SurvivalData.save.GetDensity();
        goombaDifficulty = SurvivalData.save.GetGoombaSpeed();
        difficulty = SurvivalData.save.GetDifficulty();

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeDensity(densityDifficulty);
        changeGoomba(goombaDifficulty);
        changeDifficulty(difficulty);

        if (debugMode){
            ShowStatsUI(true);
            biofeedbackText.SetActive(true);
        }
        else {
            ShowStatsUI(false);
            biofeedbackText.SetActive(false);
        }

        if (SurvivalData.save.IsNewGame()){
            baselineTimeValue = Time.time;
            SurvivalData.AcknowledgeNewGame();
        }
        if (SurvivalData.save.GetGamePhase() == SurvivalData.GamePhase.BiofeedbackLoop) {
            baselineSlider.gameObject.SetActive(false);
            mean = SurvivalData.save.GetMean();
            std = SurvivalData.save.GetSTD();
        }
    }

    // Update is called once per frame
    void Update() {
        UpdateDifficultyOnPressed();
        UpdateStatsUI();
        
        // If the game is in the Baseline collection Phase
        if (SurvivalData.save.GetGamePhase() == SurvivalData.GamePhase.BaselineCollection) {

            // Updating slider value
            baselineSlider.value = ( Time.time - baselineTimeValue );

            //Checks if baseline colleciton phase is over
            if ((Time.time - baselineTimeValue) >= baselineDuration) {

                SurvivalData.save.ChangeGamePhase(SurvivalData.GamePhase.BiofeedbackLoop);
                baselineSlider.gameObject.SetActive(false);
                InitialGenerateItems();
                Debug.Log("Array Length: " + SurvivalData.save.baselineData.Count);
                PerformCalculations();
                Debug.Log("Mean: " + mean);
                Debug.Log("std: " + std);
                SurvivalData.save.SetMean(mean);
                SurvivalData.save.SetSTD(std);
                meanText.text = mean.ToString();
                stdText.text = std.ToString();
                biofeedbackText.SetActive(true);
            }
        }

        // If the game is in the biofeedback loop phase
        else if (SurvivalData.save.GetGamePhase() == SurvivalData.GamePhase.BiofeedbackLoop) {
            GenerateItems();
        }
    }

    int counter = 0;
    void FixedUpdate() {
        counter++;

        if (counter % 15 == 0){ // 5 Hz
            UpdateEngagement();
            UpdateEngagementUI();

            //During Baseline Collection
            if (SurvivalData.save.GetGamePhase() == SurvivalData.GamePhase.BaselineCollection) {
                SurvivalData.save.AppendBaselineData(currentEngagement);
            }

            else if (SurvivalData.save.GetGamePhase() == SurvivalData.GamePhase.BiofeedbackLoop) {
                BiofeedbackLoopControl();
            }

        }
    }

    private void BiofeedbackLoopControl() {
        // Check if it engagement is past 2STD
        if (prevEngagement <= mean+2*std && currentEngagement > mean+2*std) {
            Debug.Log("INCREASE");
            difficulty++;
            if (difficulty > 10) difficulty = 10;
            changeDifficulty(difficulty);
        }   
        else if (prevEngagement >= mean-2*std && currentEngagement < mean-2*std){
            Debug.Log("DECREASE");
            difficulty--;
            if (difficulty < 1) difficulty = 1;
            changeDifficulty(difficulty);
        }
    }



    private void GenerateGoomba() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);

        Vector3 marioPos = Player.singleton.transform.position;
        //Prevent goombas from spawning near mario
        while (Mathf.Abs(marioPos.x - x) < 5 || Mathf.Abs(marioPos.z - z) < 5) {
            x = Random.Range(-19, 19);
            z = Random.Range(-19, 19);
        }

        Vector3 pos = new Vector3(x, 3, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        GameObject goomba = Instantiate(goombaPrefab, pos, currentRotation, goombaParent);
        goomba.GetComponent<Enemy>().chaseDistance = goombaChaseDistance;
        goomba.GetComponent<Enemy>().speedMultiplier = goombasSpeed;
        goomba.GetComponent<Enemy>().isRoamer = true;
        goomba.GetComponent<Enemy>().DropsCoins(true);
    }

    private void GenerateCoin() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);
        Vector3 pos = new Vector3(x, 4, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        Instantiate(coinPrefab, pos, currentRotation, coinParent);
    }


    public void ShowStatsUI(bool stats) {
        bg.SetActive(stats);
        bgText.SetActive(stats);
        bgSpeed.SetActive(stats);
        bgJump.SetActive(stats);
        bgGoomba.SetActive(stats);
        bgDensity.SetActive(stats);
    }

    public void UpdateStatsUI() {
        bgSpeed.GetComponent<UnityEngine.UI.Text>().text = marioSpeedDifficulty.ToString();
        bgJump.GetComponent<UnityEngine.UI.Text>().text = marioJumpDifficulty.ToString();
        bgDensity.GetComponent<UnityEngine.UI.Text>().text = densityDifficulty.ToString();
        bgGoomba.GetComponent<UnityEngine.UI.Text>().text = goombaDifficulty.ToString();
    }

    public void InitialGenerateItems() {
        for (int i = 0; i < goombaCount; ++i){
            GenerateGoomba();
        }

        // Spawn coins
        for (int i = 0; i < coinCount; ++i) {
            GenerateCoin();
        }
    }

    public void GenerateItems() {
        if (goombaParent.childCount < goombaCount) {
            GenerateGoomba();
        }
        if (goombaParent.childCount > goombaCount) {
            int randomInt = Random.Range(0, goombaParent.childCount-1);
            Destroy(goombaParent.transform.GetChild(randomInt).gameObject);
        }

        if (coinParent.childCount < coinCount) {
            GenerateCoin();
        }
        if (prevGoombaSpeed != goombasSpeed) {
            for(int i = 0; i < goombaParent.childCount; i++) {
                GameObject goomba = goombaParent.transform.GetChild(i).gameObject;
                goomba.GetComponent<Enemy>().speedMultiplier = goombasSpeed;
            }
        }
        prevGoombaSpeed = goombasSpeed;

    }


    public void UpdateDifficultyOnPressed() {
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            marioSpeedDifficulty++;
            if (marioSpeedDifficulty > 10) marioSpeedDifficulty = 1;
            changeMarioSpeed(marioSpeedDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            marioJumpDifficulty++;
            if (marioJumpDifficulty > 10) marioJumpDifficulty = 1;
            changeMarioJump(marioJumpDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            densityDifficulty++;
            if (densityDifficulty > 10) densityDifficulty = 1;
            changeDensity(densityDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)) {
            goombaDifficulty++;
            if (goombaDifficulty > 10) goombaDifficulty = 1;
            changeGoomba(goombaDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.F1)) {
            ShowStatsUI(!bg.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.F2)) {
            difficulty++;
            if (difficulty > 10) difficulty = 1;
            changeDifficulty(difficulty);
        }
    }




    public void changeDensity(int desiredDensityDifficulty) {

        switch(desiredDensityDifficulty){
            case 1: 
                goombaCount = 5;
                break;
            case 2: 
                goombaCount = 8;
                break;
            case 3: 
                goombaCount = 11;
                break;
            case 4: 
                goombaCount = 14;
                break;
            case 5: 
                goombaCount = 17;
                break;
            case 6: 
                goombaCount = 20;
                break;
            case 7: 
                goombaCount = 23;
                break;
            case 8: 
                goombaCount = 26;
                break;
            case 9: 
                goombaCount = 29;
                break;
            case 10: 
                goombaCount = 32;
                break;
            default: break;
        }
    }

    public void changeMarioSpeed(int desiredMarioSpeed) {
        switch(desiredMarioSpeed){
            case 1: 
                mario.speedMultiplier = 1f;
                break;
            case 2:
                mario.speedMultiplier = 0.9f;
                break;
            case 3:
                mario.speedMultiplier = 0.8f;
                break;
            case 4:
                mario.speedMultiplier = 0.7f;
                break;
            case 5:
                mario.speedMultiplier = 0.6f;
                break;
            case 6:
                mario.speedMultiplier = 0.5f;
                break;
            case 7:
                mario.speedMultiplier = 0.4f;
                break;
            case 8:
                mario.speedMultiplier = 0.3f;
                break;
            case 9:
                mario.speedMultiplier = 0.2f;
                break;
            case 10:
                mario.speedMultiplier = 0.1f;
                break;
            default: break;
        }
    }

    public void changeMarioJump(int desiredMarioSpeed) {
        switch(desiredMarioSpeed){
            case 1: 
                mario.jumpMultiplier = 1f;
                break;
            case 2: 
                mario.jumpMultiplier = 0.98f;
                break;
            case 3: 
                mario.jumpMultiplier = 0.95f;
                break;
            case 4: 
                mario.jumpMultiplier = 0.93f;
                break;
            case 5: 
                mario.jumpMultiplier = 0.9f;
                break;
            case 6:
                mario.jumpMultiplier = 0.89f;
                break;
            case 7:
                mario.jumpMultiplier = 0.88f;
                break;
            case 8:
                mario.jumpMultiplier = 0.87f;
                break;
            case 9:
                mario.jumpMultiplier = 0.86f;
                break;
            case 10:
                mario.jumpMultiplier = 0.83f;
                break;
            default: break;
        }
    }

    public void changeGoomba(int desiredGoombaSpeed) {
        switch(desiredGoombaSpeed){
            case 1: 
                goombasSpeed = 0.8f;
                break;
            case 2: 
                goombasSpeed = 0.9f;
                break;
            case 3: 
                goombasSpeed = 1f;
                break;
            case 4: 
                goombasSpeed = 1.1f;
                break;
            case 5: 
                goombasSpeed = 1.2f;
                break;
            case 6: 
                goombasSpeed = 1.3f;
                break;
            case 7: 
                goombasSpeed = 1.4f;
                break;
            case 8: 
                goombasSpeed = 1.5f;
                break;
            case 9: 
                goombasSpeed = 1.6f;
                break;
            case 10: 
                goombasSpeed = 1.7f;
                break;
            default: break;
        }
    }


    public void changeDifficulty(int desiredDifficulty) {
        switch(desiredDifficulty){
            case 1:
                marioSpeedDifficulty = 1;
                marioJumpDifficulty = 1;
                goombaDifficulty = 1;
                densityDifficulty = 1;
                break;
            case 2:
                marioSpeedDifficulty = 2;
                marioJumpDifficulty = 2;
                densityDifficulty = 3;
                goombaDifficulty = 2;
                break;
            case 3:
                marioSpeedDifficulty = 3;
                marioJumpDifficulty = 3;
                densityDifficulty = 4;
                goombaDifficulty = 3;
                break;
            case 4: 
                marioSpeedDifficulty = 4;
                marioJumpDifficulty = 4;
                densityDifficulty = 5;
                goombaDifficulty = 4;
                break;
            case 5: 
                marioSpeedDifficulty = 5;
                marioJumpDifficulty = 5;
                densityDifficulty = 6;
                goombaDifficulty = 4;
                break;
            case 6: 
                marioSpeedDifficulty = 6;
                marioJumpDifficulty = 5;
                densityDifficulty = 5;
                goombaDifficulty = 4;
                break;
            case 7: 
                marioSpeedDifficulty = 7;
                marioJumpDifficulty = 6;
                densityDifficulty = 6;
                goombaDifficulty = 5;
                break;
            case 8: 
                marioSpeedDifficulty = 8;
                marioJumpDifficulty = 7;
                densityDifficulty = 7;
                goombaDifficulty = 6;
                break;
            case 9: 
                marioSpeedDifficulty = 8;
                marioJumpDifficulty = 8;
                densityDifficulty = 8;
                goombaDifficulty = 8;
                break;
            case 10: 
                marioSpeedDifficulty = 9;
                marioJumpDifficulty = 9;
                densityDifficulty = 10;
                goombaDifficulty = 10;
                break;
            default: break;
        }

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeDensity(densityDifficulty);
        changeGoomba(goombaDifficulty);

        SurvivalData.save.UpdateMarioSpeed(marioSpeedDifficulty);
        SurvivalData.save.UpdateMarioJump(marioJumpDifficulty);
        SurvivalData.save.UpdateDensity(densityDifficulty);
        SurvivalData.save.UpdateGoombaSpeed(goombaDifficulty);
        SurvivalData.save.UpdateDifficulty(difficulty);
    }


    private void UpdateEngagement() {
        prevEngagement = currentEngagement;
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to start position
            currentEngagement = 0;
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
        currentEngagement = engagement;
    }

    public void PerformCalculations() {
        mean = MathHelper.Average(SurvivalData.save.baselineData);
        std = MathHelper.StandardDeviation(SurvivalData.save.baselineData);
    }

    public void UpdateEngagementUI() {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to start position
            engagement_num.text = "N/A";
        }
        else{
            engagement_num.text = currentEngagement.ToString();
        }
    }
}