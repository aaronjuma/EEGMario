/*
 *  Copyright (c) 2024 Hello Fangaming
 *
 *  Use of this source code is governed by an MIT-style
 *  license that can be found in the LICENSE file or at
 *  https://opensource.org/licenses/MIT.
 *  
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class GameplayData
{
    //Current loaded save file
    public static GameplayData save;
    private static string fileName = Path.Combine(Application.persistentDataPath, "survival.json");

    //Saved variables
    public int coins = 0;
    public int starCount = 0;
    public List<float> baselineData;
    public float mean;
    public float std;
    public CSVLogger baselineLogger;
    public CSVLogger gameplayLogger;

    //First 3 are position, last is y euler angle
    public float[] hubPosition = new float[] { 0f, 0f, 0f, 0f };

    public int marioJump = 1;
    public int marioSpeed = 1;
    public int goomba = 1;
    public int density = 1;
    public int firebar = 1;
    public int difficulty = 1;


    public enum GamePhase
    {
        BaselineCollection,
        BiofeedbackLoop
    }

    private GamePhase currentPhase;
    private bool isNewGame;


    //Save
    public void Save()
    {
        System.IO.File.WriteAllText(fileName, JsonUtility.ToJson(this));
    }

    //Load (Make sure fileName is set, will be loaded if it exists)
    public static bool Load()
    {
        if (System.IO.File.Exists(fileName))
        {
            save = JsonUtility.FromJson<GameplayData>(System.IO.File.ReadAllText(fileName));
            return true;
        }
        else
            return false;
    }

    //Create new game
    public static void NewGame()
    {
        save = new GameplayData();
        save.ChangeGamePhase(GamePhase.BaselineCollection);
        save.isNewGame = true;
        save.UpdateMarioSpeed(1);
        save.UpdateMarioJump(1);
        save.UpdateDensity(1);
        save.UpdateGoombaSpeed(1);
        save.UpdateDifficulty(1);
        save.baselineData = new List<float>();
        save.baselineLogger = new CSVLogger("baseline");
        save.gameplayLogger = new CSVLogger("gameplay");
    }

    //Null check
    public static void NullCheck()
    {
        if (save == null)
        {
            if (!Load()) NewGame();
            Debug.Log("Survival: Using test save file!");
        }
    }

    public bool IsNewGame() {
        return isNewGame;
    }

    public static void AcknowledgeNewGame() {
        save.isNewGame = false;
    }

    //Get star count
    public GamePhase GetGamePhase()
    {
        return currentPhase;
    }

    public void ChangeGamePhase(GamePhase newPhase) {
        currentPhase = newPhase;
    }

    public void UpdateMarioSpeed(int value) {
        marioSpeed = value;
    }

    public void UpdateMarioJump(int value) {
        marioJump = value;
    }

    public void UpdateGoombaSpeed(int value) {
        goomba = value;
    }

    public void UpdateDensity(int value) {
        density = value;
    }

    public void UpdateFirebar(int value) {
        firebar = value;
    }

    public void UpdateDifficulty(int value) {
        difficulty = value;
    }

    public int GetMarioSpeed() {
        return marioSpeed;
    }

    public int GetMarioJump() {
        return marioJump;
    }

    public int GetGoombaSpeed() {
        return goomba;
    }

    public int GetDensity() {
        return density;
    }

    public int GetFirebar() {
        return firebar;
    }

    public int GetDifficulty() {
        return difficulty;
    }

    public void AppendBaselineData(float val) {
        save.baselineData.Add(val);
    }

    public void SetMean(float val) {
        mean = val;
    }

    public void SetSTD(float val) {
        std = val;
    }

    public float GetMean() {
        return mean;
    }

    public float GetSTD() {
        return std;
    }

    public void Log(float time, float engagement, int difficulty) {
        if (currentPhase == GamePhase.BaselineCollection) {
            baselineLogger.Update(time, engagement, difficulty);
        }
        else if (currentPhase == GamePhase.BiofeedbackLoop) {
            gameplayLogger.Update(time, engagement, difficulty);
        }
    }
}
