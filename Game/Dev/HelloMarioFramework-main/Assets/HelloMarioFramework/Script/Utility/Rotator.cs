/*
 *  Copyright (c) 2024 Hello Fangaming
 *
 *  Use of this source code is governed by an MIT-style
 *  license that can be found in the LICENSE file or at
 *  https://opensource.org/licenses/MIT.
 *  
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HelloMarioFramework
{
    public class Rotator : MonoBehaviour
    {

        [SerializeField]
        private Vector3 rotationSpeed = Vector3.up;
        public float rotationMultiplier = 1f;

        void Start()
        {
            rotationSpeed = rotationSpeed * 100f; //Makes this feel more consistent with speeds used in Mover
        }

        void FixedUpdate()
        {
            transform.Rotate(rotationSpeed * rotationMultiplier * Time.fixedDeltaTime);
        }

        public void updateSpeed(float multiplier) {
            rotationMultiplier = multiplier;
        }

    }
}
