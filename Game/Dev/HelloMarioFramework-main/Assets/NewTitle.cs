/*
 *  Copyright (c) 2024 Hello Fangaming
 *
 *  Use of this source code is governed by an MIT-style
 *  license that can be found in the LICENSE file or at
 *  https://opensource.org/licenses/MIT.
 *  
 * */
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace HelloMarioFramework
{
    public class NewTitle : MonoBehaviour
    {

        //Components
        private AudioSource audioPlayer;
        private AudioSource musicPlayer;

        //Game
        private bool fileSelect = false;
        private int index = 0;
        private bool buttonDown = false;
        private bool selected = false;
        private bool newGame = false;

        //Audio clips
        [SerializeField]
        private AudioClip selectSFX;
        [SerializeField]
        private AudioClip moveSFX;
        [SerializeField]
        private AudioClip selectVoiceSFX;
        [SerializeField]
        private AudioClip fileSelectMusic;

        //Input
        [SerializeField]
        private InputActionReference jumpAction;
        [SerializeField]
        private InputActionReference movementAction;

        //UI
        [SerializeField]
        private GameObject titleScreenUI;
        [SerializeField]
        private GameObject fileSelectUI;
        [SerializeField]
        private Text fileText;
        [SerializeField]
        private Text dataText;
        [SerializeField]
        private SpriteBlinker aButton;
        private RectTransform titleScreenTransform;
        private RectTransform fileSelectTransform;

        [SerializeField]
        private GameObject selector;
        [SerializeField]
        private GameObject leftArrow;
        [SerializeField]
        private GameObject rightArrow;

        [SerializeField]
        private Text marioSpeed;
        [SerializeField]
        private Text marioJump;
        [SerializeField]
        private Text firebar;
        [SerializeField]
        private Text goomba;
        [SerializeField]
        private Text diff;

        //Hub world scene
        [SerializeField]
        private SceneReference hubScene;
        
        void Start()
        {
            musicPlayer = transform.GetComponent<AudioSource>();
            audioPlayer = gameObject.AddComponent<AudioSource>();
            titleScreenTransform = titleScreenUI.GetComponent<RectTransform>();
            fileSelectTransform = fileSelectUI.GetComponent<RectTransform>();
            jumpAction.action.Enable();
            movementAction.action.Enable();

            //Remember scene information
            LoadingScreen.titleScene = SceneManager.GetActiveScene().path;
            LoadingScreen.hubScene = hubScene.ScenePath;

#if (UNITY_STANDALONE && !UNITY_EDITOR)
            //Disable mouse cursor on windows standalone
            Cursor.visible = false;
#endif
#if (UNITY_ANDROID || UNITY_IOS)
            //Set screen resolution on Android (Saved automatically on Windows, but Unity is inconsistent)
            if (OptionsSave.save == null) OptionsSave.Load();
            OptionsApplier.ChangeResolution(OptionsSave.save.resolution);
#endif
        }

        void Update()
        {
            if (!selected)
            {
                //Title screen
                if (!fileSelect)
                {
                    //Press A to go to file select
                    if (jumpAction.action.WasPressedThisFrame())
                    {
                        fileSelect = true;
                        musicPlayer.Stop();
                        audioPlayer.PlayOneShot(selectSFX);
                        audioPlayer.PlayOneShot(selectVoiceSFX);
                        StartCoroutine(Delay());
                    }
                }

                //File select
                else
                {
                    //Press A to select this file
                    if (jumpAction.action.WasPressedThisFrame())
                    {
                        musicPlayer.Stop();
                        audioPlayer.PlayOneShot(selectSFX);
                        audioPlayer.PlayOneShot(selectVoiceSFX);

                        GameVariables newvar = new GameVariables();
                        newvar.marioJump = int.Parse(marioJump.text);
                        newvar.marioSpeed = int.Parse(marioSpeed.text);
                        newvar.goomba = int.Parse(goomba.text);
                        newvar.firebar = int.Parse(firebar.text);
                        string json = JsonUtility.ToJson(newvar);
                        File.WriteAllText(Application.dataPath + "/variables.json", json);
                        Debug.Log(Application.dataPath + "/variables.json");
                        if (newGame) SaveData.NewGame();
                        StartCoroutine(ChangeScene());
                    }
                    //Movement keys
                    else
                    {
                        float x_axis = movementAction.action.ReadValue<Vector2>().x;
                        float y_axis = movementAction.action.ReadValue<Vector2>().y;
                        Text currentText = selectedText();
                        //Down
                        if (!buttonDown && y_axis > 0.5f)
                        {
                            buttonDown = true;
                            audioPlayer.PlayOneShot(moveSFX);
                            index--;
                            if (index < 0) index = 4;
                            moveSelectors();
                        }
                        //Up
                        else if (!buttonDown && y_axis < -0.5f)
                        {
                            buttonDown = true;
                            audioPlayer.PlayOneShot(moveSFX);
                            index++;
                            if (index > 4) index = 0;
                            moveSelectors();
                        }
                        //Left
                        else if (!buttonDown && x_axis < -0.5f){
                            buttonDown = true;
                            if (index != 4){
                                int val = int.Parse(currentText.text);
                                val--;
                                if(val < 1) val = 1;
                                currentText.text = val.ToString();
                                if (!diff.text.Equals("Custom")){
                                    diff.text = "Custom";
                                    diff.color = Color.blue;
                                }
                            }
                            else{
                                int val = getDifficultyID();
                                val--;
                                if(val < 0) val = 0;
                                changeDifficulty(val);
                            }
                        }
                        //Right
                        else if (!buttonDown && x_axis > 0.5f){
                            buttonDown = true;
                            if (index != 4){
                                int val = int.Parse(currentText.text);
                                val++;
                                if(val > 10) val = 10;
                                currentText.text = val.ToString();
                                if (!diff.text.Equals("Custom")){
                                    diff.text = "Custom";
                                    diff.color = Color.blue;
                                }
                            }
                            else{
                                int val = getDifficultyID();
                                val++;
                                if(val > 4) val = 4;
                                changeDifficulty(val);
                            }
                        }

                        //Reset
                        else if (buttonDown && y_axis > -0.5f && y_axis < 0.5f && x_axis > -0.5f && x_axis < 0.5f)
                        {
                            buttonDown = false;
                        }
                    }
                    //Move file select box into view
                    if (fileSelectTransform.anchoredPosition.y < -135f)
                    {
                        fileSelectTransform.anchoredPosition += Vector2.up * 480f * Time.deltaTime;
                        titleScreenTransform.anchoredPosition += Vector2.down * 480f * Time.deltaTime;
                        if (fileSelectTransform.anchoredPosition.y >= 0f)
                            fileSelectTransform.anchoredPosition = new Vector2(fileSelectTransform.anchoredPosition.x, 0f);
                    }
                }
            }
        }

        private void moveSelectors() {
            float selector_x = -37.5f;
            float right_x = 13.5f;
            float left_x = -89f;

            float small_left_x = -89f;
            float small_selector_x = -37.5f;
            float small_right_x = 13.5f;
            float small_selector_width = 45f;

            float large_selector_width = 125f;


            if (index == 0){
                selector.GetComponent<RectTransform>().sizeDelta = new Vector2(45f, 42f);
                selector.GetComponent<RectTransform>().anchoredPosition = 61f * Vector2.up + selector_x * Vector2.right;
                rightArrow.GetComponent<RectTransform>().anchoredPosition = 76f * Vector2.up + right_x * Vector2.right;
                leftArrow.GetComponent<RectTransform>().anchoredPosition = 49f * Vector2.up + left_x * Vector2.right;
            }
            if (index == 1){
                selector.GetComponent<RectTransform>().anchoredPosition = 18f * Vector2.up + selector_x * Vector2.right;
                rightArrow.GetComponent<RectTransform>().anchoredPosition = 32f * Vector2.up + right_x * Vector2.right;
                leftArrow.GetComponent<RectTransform>().anchoredPosition = 5f * Vector2.up + left_x * Vector2.right;
            }
            if (index == 2){
                selector.GetComponent<RectTransform>().anchoredPosition = -25f * Vector2.up + selector_x * Vector2.right;
                rightArrow.GetComponent<RectTransform>().anchoredPosition = -11f * Vector2.up + right_x * Vector2.right;
                leftArrow.GetComponent<RectTransform>().anchoredPosition = -37f * Vector2.up + left_x * Vector2.right;
            }
            if (index == 3){
                selector.GetComponent<RectTransform>().sizeDelta = new Vector2(45f, 42f);
                selector.GetComponent<RectTransform>().anchoredPosition = -68f * Vector2.up + selector_x * Vector2.right;
                rightArrow.GetComponent<RectTransform>().anchoredPosition = -54f * Vector2.up + right_x * Vector2.right;
                leftArrow.GetComponent<RectTransform>().anchoredPosition = -81f * Vector2.up + left_x * Vector2.right;
            }
            if (index == 4){
                selector.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, 42f);
                selector.GetComponent<RectTransform>().anchoredPosition = -115f * Vector2.up + 1.6f * Vector2.right;
                rightArrow.GetComponent<RectTransform>().anchoredPosition = -100f * Vector2.up + 92f * Vector2.right;
                leftArrow.GetComponent<RectTransform>().anchoredPosition = -127f * Vector2.up + -89f * Vector2.right;
            }
        }

        private Text selectedText() {
            if (index == 0){
                return marioSpeed;
            }
            if (index == 1){
                return marioJump;
            }
            if (index == 2){
                return firebar;
            }
            if (index == 3) {
                return goomba;
            }
            else{
                return diff;
            }
        } 


        private int getDifficultyID() {
            if (diff.text.Equals("Easy")){
                return 1;
            }
            if (diff.text.Equals("Medium")){
                return 2;
            }
            if (diff.text.Equals("Hard")){
                return 3;
            }
            if (diff.text.Equals("Extreme")){
                return 4;
            }
            else {
                return 0;
            }
        }


        private void changeDifficulty(int id) {
            if (id == 1){
                diff.text = "Easy";
                diff.color = Color.green;
                marioSpeed.text = "1";
                marioJump.text = "1";
                goomba.text = "1";
                firebar.text = "1";
            }
            else if (id == 2){
                diff.text = "Medium";
                diff.color = Color.yellow;
                marioSpeed.text = "3";
                marioJump.text = "3";
                goomba.text = "3";
                firebar.text = "4";
            }
            else if (id == 3){
                diff.text = "Hard";
                diff.color = Color.red;
                marioSpeed.text = "5";
                marioJump.text = "5";
                goomba.text = "5";
                firebar.text = "6";
            }
            else if (id == 4){
                diff.text = "Extreme";
                diff.color = Color.magenta;
                marioSpeed.text = "7";
                marioJump.text = "7";
                goomba.text = "7";
                firebar.text = "7";
            }
            else {
                diff.text = "Custom";
                diff.color = Color.blue;
                marioSpeed.text = "1";
                marioJump.text = "9";
                goomba.text = "7";
                firebar.text = "8";
            }
        }

        private void UpdateFileSelectText()
        {
            //Get file character
            char c = (char)('A');

            SaveData.SetFileName("File" + c);

            //Attempt to load
            newGame = !SaveData.Load();
        }

        private IEnumerator Delay()
        {
            selected = true;
            aButton.delay = 0.1f;
            yield return new WaitForSeconds(1.5f);
            //titleScreenUI.SetActive(false);
            fileSelectUI.SetActive(true);
            UpdateFileSelectText();
            moveSelectors();
            changeDifficulty(getDifficultyID());
            selected = false;
            musicPlayer.clip = fileSelectMusic;
            musicPlayer.Play();
        }

        private IEnumerator ChangeScene()
        {
            selected = true;
            yield return new WaitForSeconds(1.5f);
            LoadingScreen.scene = hubScene.ScenePath;
            FadeControl.singleton.FadeToLoadingScreen();
        }
    }
}
