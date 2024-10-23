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
using UnityEngine.AI;

namespace HelloMarioFramework
{
    public class Enemy : Stompable
    {

        //Components
        private Rigidbody myRigidBody;
        protected Animator animator;
        private AudioSource audioPlayer;
        private Collider myCollider;

        //Audio clips
        [SerializeField]
        protected AudioClip voiceSFX;

        //Game
        protected bool chase = false;
        protected bool cooldown = false;
        private bool onGround = true;
        private int collisionCount = 0;

        //Animator hash values
        protected static int chaseHash = Animator.StringToHash("Chase");
        private static int stompHash = Animator.StringToHash("Stomp");
        private static int speedHash = Animator.StringToHash("Speed");
        private static int groundHash = Animator.StringToHash("onGround");
        private static int waitSpeedHash = Animator.StringToHash("WaitSpeed"); 

        //Stompable
        [SerializeField]
        protected bool stompable = true;
        [SerializeField]
        private bool dropsCoin = false;
        

        public bool isRoamer = false;
        private bool isRoaming = true;

        public float speedMultiplier = 1f;
        public float chaseDistance = 10f;
        
        private Vector3 waypoint;

        [SerializeField]
        private GameObject coinPrefab;

        void Start()
        {
            myRigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            audioPlayer = gameObject.AddComponent<AudioSource>();
            myCollider = GetComponent<Collider>();

            myRigidBody.freezeRotation = true;
            if (!stompable) stompHeightCheck = 100f;
        }

        //Before draw calls
        private void LateUpdate()
        {
            if (chase)
            {
                Vector3 i = new Vector3(myRigidBody.velocity.x, 0f, myRigidBody.velocity.z);
                animator.SetFloat(speedHash, i.magnitude);
            }
            if (isRoamer && isRoaming) {
                Vector3 i = new Vector3(myRigidBody.velocity.x, 0f, myRigidBody.velocity.z);
                animator.SetFloat(waitSpeedHash, 5f);
            }
        }

        //What to do when stomped. Override this.
        protected override void WhenStomped()
        {
            chase = false;
            cooldown = true;
            if (dropsCoin) {
                Quaternion currentRotation = new Quaternion();
                currentRotation.eulerAngles = new Vector3(0, 90, 0);
                Instantiate(coinPrefab, transform.position + Vector3.up, currentRotation);
            }
            animator.SetBool(stompHash, true);
            myCollider.enabled = false;
            myRigidBody.isKinematic = true;
            myRigidBody.detectCollisions = false;
        }

        //What to do when hurting player. Override this.
        protected override void WhenHurtPlayer(Player p)
        {
            chase = false;
            myRigidBody.velocity = Vector3.zero;
            StopAllCoroutines();
            if (p.GetHealth() > 0)
                StartCoroutine(Cooldown(1.1f));
            else
            {
                animator.SetBool(chaseHash, false);
                cooldown = true;
            }
        }

        //Move fixed update to here. Override this.
        protected override void FixedUpdateStompable()
        {
            myRigidBody.rotation = Quaternion.Euler(0f, myRigidBody.rotation.eulerAngles.y, 0f);

            //Collision delay
            if (collisionCount > 0)
            {
                collisionCount--;
                if (collisionCount == 0)
                {
                    onGround = false;
                    animator.SetBool(groundHash, false);
                }
            }

            //Manage drag
            if (!onGround) myRigidBody.drag = 0f;
            else if (chase) myRigidBody.drag = 1.7f;
            // else myRigidBody.drag = 100f;

            //Cooldown
            if (!cooldown)
            {
                //Start chase
                if (onGround && !chase && Player.singleton.CanBeChased(transform.position, chaseDistance))
                {
                    isRoaming = false;
                    chase = true;
                    StartCoroutine(Cooldown(0.9f));
                    audioPlayer.PlayOneShot(voiceSFX);
                }

                //End chase
                else if (!onGround || (chase && !Player.singleton.CanBeChased(transform.position, chaseDistance)))
                {
                    chase = false;
                    StartCoroutine(Cooldown(1.1f));
                    if (onGround) myRigidBody.velocity = Vector3.zero;
                    isRoaming = true;
                    FindNewWaypoint();
                }

                //Chase
                if (chase)
                {
                    //Change direction
                    Player.singleton.LookAtMe(transform);

                    //Move in direction
                    myRigidBody.velocity += transform.forward * 25f * Time.fixedDeltaTime; //0.25f

                    //Speed cap
                    Vector2 mvmntSpeed = new Vector2(myRigidBody.velocity.x, myRigidBody.velocity.z);
                    if (mvmntSpeed.sqrMagnitude > 16f *speedMultiplier)
                    {
                        mvmntSpeed.Normalize();
                        mvmntSpeed = mvmntSpeed * 4f * speedMultiplier;
                        myRigidBody.velocity = new Vector3(mvmntSpeed.x, myRigidBody.velocity.y, mvmntSpeed.y);
                    }
                }
                // Roaming
                else {
                    if (isRoamer) {
                        if (isRoaming) {
                            if (waypoint == Vector3.zero) {
                                FindNewWaypoint();
                            }
                            if (Vector2.Distance(transform.position, waypoint) <= 1f) {
                                FindNewWaypoint();
                            }

                            if (!chase) {
                                transform.LookAt(waypoint);

                                myRigidBody.velocity += transform.forward * 2f * Time.fixedDeltaTime; //0.25f

                                //Speed cap
                                Vector2 mvmntSpeed = new Vector2(myRigidBody.velocity.x, myRigidBody.velocity.z);
                                if (mvmntSpeed.sqrMagnitude > 1f)
                                {
                                    mvmntSpeed.Normalize();
                                    mvmntSpeed = mvmntSpeed * 1f;
                                    myRigidBody.velocity = new Vector3(mvmntSpeed.x, myRigidBody.velocity.y, mvmntSpeed.y);
                                }
                            }
                        }
                        else {
                            StartCoroutine(Cooldown(0.5f));
                        }
                    }
                    else {
                        StartCoroutine(Cooldown(0.5f));
                    }
                }
            }
        }

        //Move on collision stay to here. Override this.
        protected override void OnCollisionStayStompable(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.6f)
                {
                    if (!onGround)
                    {
                        onGround = true;
                        animator.SetBool(groundHash, true);
                    }
                    collisionCount = 8;
                    break;
                }
            }
        }

        //Cooldown
        private IEnumerator Cooldown(float f)
        {
            animator.SetBool(chaseHash, chase);
            cooldown = true;
            yield return new WaitForSeconds(f); //0.9f
            cooldown = false;
        }

        private void FindNewWaypoint() {
            if (isRoamer) {
                waypoint = transform.position + Random.insideUnitSphere * 7f;
                waypoint.y = transform.position.y;
                while (Mathf.Abs(waypoint.x) > 19.5 || Mathf.Abs(waypoint.z) > 19.5) {
                    waypoint = transform.position + Random.insideUnitSphere * 7f;
                    waypoint.y = transform.position.y;
                }
            }
        }

        public void DropsCoins(bool dropCoin) {
            dropsCoin = dropCoin;
        }

    }
}
