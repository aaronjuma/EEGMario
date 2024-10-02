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
    [SerializeField] private float goombaChaseDistance = 10f;
    [SerializeField] private int goombaCount = 15;
    [SerializeField] private int coinCount = 5;

    // Start is called before the first frame update
    void Start() {

        // Spawn Goombas
        for (int i = 0; i < goombaCount; ++i){
            GenerateGoomba();
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
            GenerateGoomba();
        }
        if (coinParent.childCount < coinCount) {
            GenerateCoin();
        }
    }



    private void GenerateGoomba() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);

        //Prevent goombas from spawning near the
        while (Mathf.Abs(x) < 5 || Mathf.Abs(z) < 5) {
            x = Random.Range(-19, 19);
            z = Random.Range(-19, 19);
        }
        Vector3 pos = new Vector3(x, 3, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        GameObject goomba = Instantiate(goombaPrefab, pos, currentRotation, goombaParent);
        goomba.GetComponent<Enemy>().chaseDistance = goombaChaseDistance;
    }

    private void GenerateCoin() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);
        Vector3 pos = new Vector3(x, 4, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        Instantiate(coinPrefab, pos, currentRotation, coinParent);
    }
}
