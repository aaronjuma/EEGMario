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
        private Image buttonPlay;
        [SerializeField]
        private Image buttonSettings;
        [SerializeField]
        private Image buttonQuit;
        [SerializeField]
        private GameObject settingsPanel;

        bool backed = false;


        //Hub world scene
        [SerializeField]
        private SceneReference level1;
        [SerializeField]
        private SceneReference survival;

        private SceneReference chosenScene;
        
        void Start()
        {
            musicPlayer = transform.GetComponent<AudioSource>();
            audioPlayer = gameObject.AddComponent<AudioSource>();
            titleScreenTransform = titleScreenUI.GetComponent<RectTransform>();
            fileSelectTransform = fileSelectUI.GetComponent<RectTransform>();
            jumpAction.action.Enable();
            movementAction.action.Enable();

            //Remember scene information
            chosenScene = survival;
            LoadingScreen.titleScene = SceneManager.GetActiveScene().path;
            LoadingScreen.hubScene = SceneManager.GetActiveScene().path;

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
                    if (jumpAction.action.WasPressedThisFrame() && settingsPanel.activeSelf == false)
                    {
                        if (index == 0){ 
                            musicPlayer.Stop();
                            audioPlayer.PlayOneShot(selectSFX);
                            audioPlayer.PlayOneShot(selectVoiceSFX);

                            // GameVariables newvar = new GameVariables();
                            // newvar.marioJump = int.Parse(marioJump.text);
                            // newvar.marioSpeed = int.Parse(marioSpeed.text);
                            // newvar.goomba = int.Parse(goomba.text);
                            // newvar.firebar = int.Parse(firebar.text);
                            // string json = JsonUtility.ToJson(newvar);
                            // File.WriteAllText(Application.dataPath + "/variables.json", json);
                            // Debug.Log(Application.dataPath + "/variables.json");
                            if (newGame) SaveData.NewGame();
                            StartCoroutine(ChangeScene());
                        }
                        if (index == 1) {
                            if (backed == true){
                                backed = false;
                            }
                            else {
                                settingsPanel.SetActive(true);
                            }
                        }
                        if (index == 2){
                            Application.Quit();
                        }
                    }
                    //Movement keys
                    else
                    {
                        float x_axis = 0;
                        float y_axis = 0;
                        if (settingsPanel.activeSelf == false){
                            x_axis = movementAction.action.ReadValue<Vector2>().x;
                            y_axis = movementAction.action.ReadValue<Vector2>().y;
                        }
                        //Down
                        // if (!buttonDown && y_axis > 0.5f)
                        // {
                        //     // buttonDown = true;
                        //     // audioPlayer.PlayOneShot(moveSFX);
                        //     // index--;
                        //     // if (index < 0) index = 5;
                        //     // moveSelectors();
                        // }
                        // //Up
                        // else if (!buttonDown && y_axis < -0.5f)
                        // {
                        //     // buttonDown = true;
                        //     // audioPlayer.PlayOneShot(moveSFX);
                        //     // index++;
                        //     // if (index > 5) index = 0;
                        //     // moveSelectors();
                        // }
                        //Left
                        if (!buttonDown && x_axis < -0.5f){
                            buttonDown = true;
                            index--;
                            if(index < 0) index = 0;
                            HighlightButton();
                        }
                        //Right
                        else if (!buttonDown && x_axis > 0.5f){
                            buttonDown = true;
                            index++;
                            if(index > 2) index = 2;
                            HighlightButton();
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

        private void HighlightButton() {
            Color highlighted = new Color(0.65f, 0.65f, 0.65f, 0.8f);
            Color notHighlighted = new Color(0f, 0f, 0f, 0.6f);
            if (index == 0) {
                buttonPlay.color = highlighted;
                buttonSettings.color = notHighlighted;
                buttonQuit.color = notHighlighted;
            }
            else if (index == 1){
                buttonPlay.color = notHighlighted;
                buttonSettings.color = highlighted;
                buttonQuit.color = notHighlighted;
            }
            else if (index == 2){
                buttonPlay.color = notHighlighted;
                buttonSettings.color = notHighlighted;
                buttonQuit.color = highlighted;
            }
        }

        public void BackButtonPressed() {
            settingsPanel.SetActive(false);
            backed = true;
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
            HighlightButton();
            fileSelectUI.SetActive(true);
            UpdateFileSelectText();
            selected = false;
            musicPlayer.clip = fileSelectMusic;
            musicPlayer.Play();
        }

        private IEnumerator ChangeScene()
        {
            selected = true;
            yield return new WaitForSeconds(1.5f);
            LoadingScreen.scene = chosenScene.ScenePath;
            FadeControl.singleton.FadeToLoadingScreen();
        }
    }
}
