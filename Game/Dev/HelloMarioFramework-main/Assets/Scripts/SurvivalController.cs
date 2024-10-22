using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelloMarioFramework;

public class SurvivalController : MonoBehaviour
{

    [SerializeField] private GameObject goombaPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform goombaParent;
    [SerializeField] private Transform coinParent;
    [SerializeField] private float goombaChaseDistance = 2f;
    [SerializeField] private int goombaCount = 15;
    [SerializeField] private int coinCount = 5;

    public GameObject bg;
    public GameObject bgText;
    public GameObject bgSpeed;
    public GameObject bgJump;
    public GameObject bgDensity;
    public GameObject bgGoomba;

    public int marioSpeedDifficulty = 1; // 0-easy, 1-med, 2-hard
    public int marioJumpDifficulty = 1;
    public int densityDifficulty = 1;
    public int goombaDifficulty = 1;

    public Player mario;
    private float goombasSpeed = 1f;

    // Start is called before the first frame update
    void Start() {

        string json = File.ReadAllText(Application.dataPath + "/variables.json");
        GameVariables gameDifficulty = JsonUtility.FromJson<GameVariables>(json);
        marioSpeedDifficulty = gameDifficulty.marioSpeed;
        marioJumpDifficulty = gameDifficulty.marioJump;
        densityDifficulty = gameDifficulty.firebar;
        goombaDifficulty = gameDifficulty.goomba;

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeDensity(densityDifficulty);
        changeGoomba(goombaDifficulty);

        // Spawn Goombas
        for (int i = 0; i < goombaCount; ++i){
            GenerateGoomba(true);
        }
        Debug.Log(goombaParent.childCount);

        // Spawn coins
        for (int i = 0; i < coinCount; ++i) {
            GenerateCoin();
        }




    }

    // Update is called once per frame
    void Update() {
        if (goombaParent.childCount < goombaCount) {
            GenerateGoomba(false);
        }
        if (coinParent.childCount < coinCount) {
            GenerateCoin();
        }



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
            showStats(!bg.activeSelf);
        }

        bgSpeed.GetComponent<UnityEngine.UI.Text>().text = marioSpeedDifficulty.ToString();
        bgJump.GetComponent<UnityEngine.UI.Text>().text = marioJumpDifficulty.ToString();
        bgDensity.GetComponent<UnityEngine.UI.Text>().text = densityDifficulty.ToString();
        bgGoomba.GetComponent<UnityEngine.UI.Text>().text = goombaDifficulty.ToString();
    }

    private void GenerateGoomba(bool start) {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);

        //Prevent goombas from spawning near the
        if (start){
            while (Mathf.Abs(x) < 5 || Mathf.Abs(z) < 5) {
                x = Random.Range(-19, 19);
                z = Random.Range(-19, 19);
            }
        }
        else{
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


    public void showStats(bool stats) {
        bg.SetActive(stats);
        bgText.SetActive(stats);
        bgSpeed.SetActive(stats);
        bgJump.SetActive(stats);
        bgGoomba.SetActive(stats);
        bgDensity.SetActive(stats);
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
                goombaCount = 10;
                break;
            case 4: 
                goombaCount = 13;
                break;
            case 5: 
                goombaCount = 15;
                break;
            case 6: 
                goombaCount = 17;
                break;
            case 7: 
                goombaCount = 18;
                break;
            case 8: 
                goombaCount = 20;
                break;
            case 9: 
                goombaCount = 22;
                break;
            case 10: 
                goombaCount = 25;
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
                mario.jumpMultiplier = 0.85f;
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

        for(int i = 0; i < goombaParent.childCount; i++) {
            GameObject goomba = goombaParent.transform.GetChild(i).gameObject;
            goomba.GetComponent<Enemy>().speedMultiplier = goombasSpeed;
        }
    }
}
