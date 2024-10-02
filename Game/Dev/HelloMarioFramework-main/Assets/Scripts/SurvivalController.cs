using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelloMarioFramework;

public class SurvivalController : MonoBehaviour
{

    [SerializeField] private GameObject goombaPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform goombaParent;
    
    // Start is called before the first frame update
    void Start() {

        // Spawn Goombas
        for (int i = 0; i < 10; ++i){
            float x = Random.Range(-19, 19);
            float z = Random.Range(-19, 19);
            Vector3 pos = new Vector3(x, 3, z);
            Quaternion currentRotation = new Quaternion();
            currentRotation.eulerAngles = new Vector3(0, 90, 0);
            Instantiate(goombaPrefab, pos, currentRotation, goombaParent);
            
        }
        Debug.Log(goombaParent.childCount);
    }

    // Update is called once per frame
    void Update() {
        
    }
}
