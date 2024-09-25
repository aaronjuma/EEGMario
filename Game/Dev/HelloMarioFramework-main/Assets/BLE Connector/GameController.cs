using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelloMarioFramework;

public class GameController : MonoBehaviour
{
    public int marioSpeedDifficulty; // 0-easy, 1-med, 2-hard
    public int marioJumpDifficulty;
    public int firebarDifficulty;
    public int goombaDifficulty;

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


    public GameObject bg;
    public GameObject bgText;
    public GameObject bgSpeed;
    public GameObject bgJump;
    public GameObject bgFirebar;
    public GameObject bgGoomba;

    // Start is called before the first frame update
    void Start()
    {
        string json = File.ReadAllText(Application.dataPath + "/variables.json");
        GameVariables gameDifficulty = JsonUtility.FromJson<GameVariables>(json);
        marioSpeedDifficulty = gameDifficulty.marioSpeed;
        marioJumpDifficulty = gameDifficulty.marioJump;
        firebarDifficulty = gameDifficulty.firebar;
        goombaDifficulty = gameDifficulty.goomba;

        changeMarioSpeed(marioSpeedDifficulty);
        changeMarioJump(marioJumpDifficulty);
        changeFirebar(firebarDifficulty);
        changeGoomba(goombaDifficulty);
        showStats(false);
    }

    // Update is called once per frame
    void Update(){
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
            showStats(!bg.activeSelf);
        }
    }

    public void showStats(bool stats) {
        // bg.GetComponent<CanvasElementVisibility>().visible = stats;
        // bgText.GetComponent<CanvasElementVisibility>().visible = stats;
        // bgSpeed.GetComponent<CanvasElementVisibility>().visible = stats;
        // bgJump.GetComponent<CanvasElementVisibility>().visible = stats;
        // bgGoomba.GetComponent<CanvasElementVisibility>().visible = stats;
        // bgFirebar.GetComponent<CanvasElementVisibility>().visible = stats;
        bg.SetActive(stats);
        bgText.SetActive(stats);
        bgSpeed.SetActive(stats);
        bgJump.SetActive(stats);
        bgGoomba.SetActive(stats);
        bgFirebar.SetActive(stats);
    }

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
}