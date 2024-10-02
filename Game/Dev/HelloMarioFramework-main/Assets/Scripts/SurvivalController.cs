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
    
    private int goombaCount = 10;
    private int coinCount = 5;

    // Start is called before the first frame update
    void Start() {

        // Spawn Goombas
        for (int i = 0; i < goombaCount; ++i){
            generateGoomba();
        }
        Debug.Log(goombaParent.childCount);

        // Spawn coins
        for (int i = 0; i < coinCount; ++i) {
            generateCoin();
        }
    }

    // Update is called once per frame
    void Update() {
        if (goombaParent.childCount < goombaCount) {
            generateGoomba();
        }
        if (coinParent.childCount < coinCount) {
            generateCoin();
        }
    }



    private void generateGoomba() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);
        Vector3 pos = new Vector3(x, 3, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        GameObject goomba = Instantiate(goombaPrefab, pos, currentRotation, goombaParent);
        goomba.GetComponent<Enemy>().chaseDistance = goombaChaseDistance;
    }

    private void generateCoin() {
        float x = Random.Range(-19, 19);
        float z = Random.Range(-19, 19);
        Vector3 pos = new Vector3(x, 4, z);
        Quaternion currentRotation = new Quaternion();
        currentRotation.eulerAngles = new Vector3(0, 90, 0);
        Instantiate(coinPrefab, pos, currentRotation, coinParent);
    }
}
