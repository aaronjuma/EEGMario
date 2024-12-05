using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HelloMarioFramework;
using Interaxon.Libmuse;

public class Level1Controller : MonoBehaviour
{
    public int marioSpeedDifficulty; // 0-easy, 1-med, 2-hard
    public int marioJumpDifficulty;
    public int firebarDifficulty;
    public int goombaDifficulty;
    public int difficulty;

    public Player mario;
    public Rotator firstFirebar;
    public Rotator secondFirebar;
    public Rotator thirdFirebar1;
    public Rotator thirdFirebar2;
    public Rotator fourthFirebar1;
    public Rotator fourthFirebar2;
    public Rotator fourthFirebar3;
    public Rotator fourthFirebar4;

    public Enemy firstGoomba1;
    public Enemy firstGoomba2;
    public Enemy secondGoomba1;
    public Enemy secondGoomba2;
    public Enemy fourthGoomba;

    [SerializeField] public GameObject gameStats;
    [SerializeField] public GameObject bgSpeed;
    [SerializeField] public GameObject bgJump;
    [SerializeField] public GameObject bgFirebar;
    [SerializeField] public GameObject bgGoomba;
    [SerializeField] private GameObject musePanelHUD;

    [SerializeField] private GameObject baselinePanel;
    [SerializeField] private Slider baselineSlider;
    [SerializeField] public Text engagementText;
    [SerializeField] public GameObject biofeedbackText;
    [SerializeField] public Text meanText;
    [SerializeField] public Text stdText;

    private float mean;
    private float std;
    private float baselineTimeValue;
    float currentEngagement = 0;
    float prevEngagement;

    [SerializeField] public bool debugMode;

    public int baselineDuration = 180; //180 Seconds



    void Awake() {
        GameplayData.NullCheck();
        baselineSlider.maxValue = baselineDuration;
        baselineSlider.minValue = 0;
    }


    // Start is called before the first frame update
    void Start() {
        marioSpeedDifficulty = GameplayData.save.GetMarioSpeed();
        marioJumpDifficulty = GameplayData.save.GetMarioJump();
        firebarDifficulty = GameplayData.save.GetDensity();
        goombaDifficulty = GameplayData.save.GetGoombaSpeed();
        difficulty = GameplayData.save.GetDifficulty();

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeFirebar(firebarDifficulty);
        changeGoomba(goombaDifficulty);
        changeDifficulty(difficulty);

        if (debugMode){
            ShowStatsUI(true);
        }
        else {
            ShowStatsUI(false);
        }

        if (GameplayData.save.IsNewGame()){
            baselineTimeValue = Time.time;
            GameplayData.AcknowledgeNewGame();
            baselinePanel.SetActive(true);
            mario.EnableControls(false);
        }
        if (GameplayData.save.GetGamePhase() == GameplayData.GamePhase.BiofeedbackLoop) {
            baselinePanel.SetActive(false);
            mean = GameplayData.save.GetMean();
            std = GameplayData.save.GetSTD();
            meanText.text = mean.ToString();
            stdText.text = std.ToString();
        }
    }

    // Update is called once per frame
    void Update(){
        UpdateDifficultyOnPressed();
        UpdateStatsUI();
        
        // If the game is in the Baseline collection Phase
        if (GameplayData.save.GetGamePhase() == GameplayData.GamePhase.BaselineCollection) {

            // Updating slider value
            baselineSlider.value = ( Time.time - baselineTimeValue );

            //Checks if baseline colleciton phase is over
            if ((Time.time - baselineTimeValue) >= baselineDuration) {

                GameplayData.save.ChangeGamePhase(GameplayData.GamePhase.BiofeedbackLoop);
                baselinePanel.SetActive(false);
                PerformCalculations();
                GameplayData.save.SetMean(mean);
                GameplayData.save.SetSTD(std);
                meanText.text = mean.ToString();
                stdText.text = std.ToString();
                mario.EnableControls(true);
            }
        }

        // If the game is in the biofeedback loop phase
        else if (GameplayData.save.GetGamePhase() == GameplayData.GamePhase.BiofeedbackLoop) {
            //Do something
        }
    }

    int counter = 0;
    void FixedUpdate() {
        counter++;

        if (counter % 15 == 0){ // 5 Hz
            UpdateEngagement();
            UpdateEngagementUI();
            GameplayData.save.Log(Time.time, currentEngagement, difficulty);

            //During Baseline Collection
            if (GameplayData.save.GetGamePhase() == GameplayData.GamePhase.BaselineCollection) {
                GameplayData.save.AppendBaselineData(currentEngagement);
            }

            else if (GameplayData.save.GetGamePhase() == GameplayData.GamePhase.BiofeedbackLoop) {
                BiofeedbackLoopControl();
            }

        }
    }

    private void BiofeedbackLoopControl() {
        // Check if it engagement is past 2STD
        if (prevEngagement <= mean+2*std && currentEngagement > mean+2*std) {
            difficulty++;
            if (difficulty > 10) difficulty = 10;
            changeDifficulty(difficulty);
        }   
        else if (prevEngagement >= mean-2*std && currentEngagement < mean-2*std){
            difficulty--;
            if (difficulty < 1) difficulty = 1;
            changeDifficulty(difficulty);
        }
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
            firebarDifficulty++;
            if (firebarDifficulty > 10) firebarDifficulty = 1;
            changeFirebar(firebarDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)) {
            goombaDifficulty++;
            if (goombaDifficulty > 10) goombaDifficulty = 1;
            changeGoomba(goombaDifficulty);
        }
        if(Input.GetKeyDown(KeyCode.F1)) {
            ShowStatsUI(!gameStats.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.F2)) {
            difficulty++;
            if (difficulty > 10) difficulty = 1;
            changeDifficulty(difficulty);
        }
    }

    public void UpdateStatsUI() {
        bgSpeed.GetComponent<UnityEngine.UI.Text>().text = marioSpeedDifficulty.ToString();
        bgJump.GetComponent<UnityEngine.UI.Text>().text = marioJumpDifficulty.ToString();
        bgFirebar.GetComponent<UnityEngine.UI.Text>().text = firebarDifficulty.ToString();
        bgGoomba.GetComponent<UnityEngine.UI.Text>().text = goombaDifficulty.ToString();
    }


    public void ShowStatsUI(bool stats) {
        gameStats.SetActive(stats);
        biofeedbackText.SetActive(stats);
        engagementText.gameObject.SetActive(stats);
        musePanelHUD.SetActive(stats);
    }








    /* Game Difficulty Changer */


    public void changeFirebar(int desiredFirebarDifficulty) {
        float firebarsSpeed = 1.0f;

        switch(desiredFirebarDifficulty){
            case 1: 
                firebarsSpeed = 0.75f;
                break;
            case 2: 
                firebarsSpeed = 0.85f;
                break;
            case 3: 
                firebarsSpeed = 1f;
                break;
            case 4: 
                firebarsSpeed = 1.25f;
                break;
            case 5: 
                firebarsSpeed = 1.5f;
                break;
            case 6: 
                firebarsSpeed = 1.75f;
                break;
            case 7: 
                firebarsSpeed = 2.0f;
                break;
            case 8: 
                firebarsSpeed = 2.25f;
                break;
            case 9: 
                firebarsSpeed = 2.5f;
                break;
            case 10: 
                firebarsSpeed = 2.75f;
                break;
            default: break;
        }

        firstFirebar.updateSpeed(firebarsSpeed);
        secondFirebar.updateSpeed(firebarsSpeed);
        thirdFirebar1.updateSpeed(firebarsSpeed);
        thirdFirebar2.updateSpeed(firebarsSpeed);
        fourthFirebar1.updateSpeed(firebarsSpeed);
        fourthFirebar2.updateSpeed(firebarsSpeed);
        fourthFirebar3.updateSpeed(firebarsSpeed);
        fourthFirebar4.updateSpeed(firebarsSpeed);
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
                mario.jumpMultiplier = 0.85f;
                break;
            default: break;
        }
    }

    public void changeGoomba(int desiredGoombaSpeed) {
        float goombasSpeed = 0f;
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


        firstGoomba1.speedMultiplier = goombasSpeed;
        firstGoomba2.speedMultiplier = goombasSpeed;
        secondGoomba1.speedMultiplier = goombasSpeed;
        secondGoomba2.speedMultiplier = goombasSpeed;
    }
    
    
    public void changeDifficulty(int desiredDifficulty) {
        switch(desiredDifficulty){
            case 1:
                marioSpeedDifficulty = 1;
                marioJumpDifficulty = 1;
                firebarDifficulty = 1;
                goombaDifficulty = 1;
                break;
            case 2:
                marioSpeedDifficulty = 2;
                marioJumpDifficulty = 2;
                firebarDifficulty = 3;
                goombaDifficulty = 2;
                break;
            case 3:
                marioSpeedDifficulty = 3;
                marioJumpDifficulty = 3;
                firebarDifficulty = 4;
                goombaDifficulty = 3;
                break;
            case 4: 
                marioSpeedDifficulty = 4;
                marioJumpDifficulty = 4;
                firebarDifficulty = 5;
                goombaDifficulty = 4;
                break;
            case 5: 
                marioSpeedDifficulty = 5;
                marioJumpDifficulty = 5;
                firebarDifficulty = 6;
                goombaDifficulty = 4;
                break;
            case 6: 
                marioSpeedDifficulty = 6;
                marioJumpDifficulty = 5;
                firebarDifficulty = 5;
                goombaDifficulty = 4;
                break;
            case 7: 
                marioSpeedDifficulty = 7;
                marioJumpDifficulty = 6;
                firebarDifficulty = 6;
                goombaDifficulty = 5;
                break;
            case 8: 
                marioSpeedDifficulty = 8;
                marioJumpDifficulty = 7;
                firebarDifficulty = 7;
                goombaDifficulty = 6;
                break;
            case 9: 
                marioSpeedDifficulty = 8;
                marioJumpDifficulty = 8;
                firebarDifficulty = 8;
                goombaDifficulty = 8;
                break;
            case 10: 
                marioSpeedDifficulty = 9;
                marioJumpDifficulty = 9;
                firebarDifficulty = 10;
                goombaDifficulty = 10;
                break;
            default: break;
        }

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeFirebar(firebarDifficulty);
        changeGoomba(goombaDifficulty);

        GameplayData.save.UpdateMarioSpeed(marioSpeedDifficulty);
        GameplayData.save.UpdateMarioJump(marioJumpDifficulty);
        GameplayData.save.UpdateFirebar(firebarDifficulty);
        GameplayData.save.UpdateGoombaSpeed(goombaDifficulty);
        GameplayData.save.UpdateDifficulty(difficulty);
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
        mean = MathHelper.Average(GameplayData.save.baselineData);
        std = MathHelper.StandardDeviation(GameplayData.save.baselineData);
    }

    public void UpdateEngagementUI() {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to start position
            engagementText.text = "Engagement: N/A";
        }
        else{
            engagementText.text = "Engagemenet: " + currentEngagement.ToString();
        }
    }
}