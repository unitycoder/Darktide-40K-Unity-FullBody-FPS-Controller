using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class FPSWeapon : MonoBehaviour
    {
        public FPSPlayerController PlayerController;
        public FPSCameraController FPSController;
        public Animator BodyAnimator;
        public Animator ArmsAnimator;
        public GameObject mainCam;
        [Space(10)]

        public bool UseFPSArmsConstant = true;
        public bool UseFPSArmsOnAttack = true;
        public bool UseFPSArmsOnAim = true;
        [Space(10)]


        public HealthUIController HUDController;
        public WeaponAdjuster WeaponAdjust;
        public Vector3 aimCamPosition;
        public Vector3 aimCamOffset = new Vector3(-15, 0, 0);
        [Space(10)]


        public bool UseRandomAttack;
        [Range(-5, 5)] public int MinRandom;
        [Range(-5, 5)] public int MaxRandom;
        public float MoveSetID = 1;
        public WeaponTypes Weapon;
        public int Damage;
        public GameObject[] SecondaryObjects;
        public MeleeTrigger[] Hitboxes;
        [Space(5)]
        public bool attacking;
        public bool swapping;
        public bool sprinting;
        public bool aiming;
        [Space(10)]

        public float originalCamFov = 60;
        public float aimTime = 5;
        public float zoomInAmount = 50;
        public float aimInOutDuration = 0.1f;




        internal Vector3 originalCamPos;
        internal Vector3 originalAimOffsetCamPos;

        internal bool aimFinished = true;
        internal float aimTimeElapsed;

        public enum WeaponTypes { Rifle, Pistol, Melee };




        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            if (WeaponAdjust) WeaponAdjust.enabled = false;
            if (WeaponAdjust) WeaponAdjust.enabled = true;

            if (UseFPSArmsConstant)
            {
                FPSController.LockFullbodyArms = true;
            }
            if (!UseFPSArmsConstant)
            {
                FPSController.LockFullbodyArms = false;
            }

            if (Hitboxes.Length > 0)
            {
                for (int i = 0; i < Hitboxes.Length; i++)
                {
                    Hitboxes[i].PlayerController = PlayerController;
                }
            }
        }
        void Start()
        {
            if (!HUDController) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HealthUIController>();

            originalCamPos = new Vector3(0, .35f, 0.15f);
        }

        void Update()
        {
            //Attack

            if (Input.GetKeyDown(KeyCode.Mouse0) && Weapon == WeaponTypes.Melee)
            {
               
            }

            if (Input.GetKey(KeyCode.Mouse0) && Weapon == WeaponTypes.Melee)
            {            
                if (UseRandomAttack && !ArmsAnimator.GetCurrentAnimatorStateInfo(0).IsName("RandomAttack"))
                {
                    float randomAttackFloat = Random.Range(MinRandom, MaxRandom);
                    ArmsAnimator.SetFloat("RandomAttackID", randomAttackFloat);
                    ArmsAnimator.Play("RandomAttack");
                }

                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    FPSController.LockFullbodyArms = true;
                }

                if (Hitboxes.Length > 0)
                {
                    for (int i = 0; i < Hitboxes.Length; i++)
                    {
                        Hitboxes[i].Trigger.enabled = true;
                    }
                }
              
                attacking = true;
            }
            if (Input.GetKeyUp(KeyCode.Mouse0) && Weapon == WeaponTypes.Melee)
            {
                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    FPSController.LockFullbodyArms = false;
                }

                if (Hitboxes.Length > 0)
                {
                    for (int i = 0; i < Hitboxes.Length; i++)
                    {
                        Hitboxes[i].Trigger.enabled = false;
                    }
                }

                attacking = false;
            }

            //Sprint
            if (Input.GetKey(KeyCode.LeftShift) && !aiming)
            {
                sprinting = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                sprinting = false;
            }

            //PlayerAnimators
            if (!UseRandomAttack) ArmsAnimator.SetBool("Attack", attacking);
            ArmsAnimator.SetBool("Sprint", sprinting);
        }
        void LateUpdate()
        {

            //Aiming
            if (Input.GetKeyDown(KeyCode.Mouse1) && aimFinished && !swapping)
            {
                if(UseFPSArmsOnAim && !UseFPSArmsConstant)
                {
                    FPSController.LockFullbodyArms = true;
                }

                mainCam.transform.localEulerAngles = aimCamOffset;

                originalAimOffsetCamPos = aimCamPosition;
                aimCamPosition += originalCamPos;

                ArmsAnimator.SetBool("Aiming", true);
                BodyAnimator.SetFloat("idleAnimSpeed", 0);

                aimTimeElapsed = 0;
                aiming = true;
                aimFinished = false;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1) && aiming && !swapping)
            {
                if (UseFPSArmsOnAim && !UseFPSArmsConstant)
                {
                    FPSController.LockFullbodyArms = false;
                }

                mainCam.transform.localEulerAngles = Vector3.zero;

                aiming = false;
                aimTimeElapsed = 0;
                Invoke("aimingOutFinished", aimInOutDuration);

                ArmsAnimator.SetBool("Aiming", false);
                BodyAnimator.SetFloat("idleAnimSpeed", 1);

            }

            if (aiming && !aimFinished)
            {
                LerpAimIn();
            }
            else if (!aiming && !aimFinished)
            {
                LerpAimOut();
            }
        }



        void aimingOutFinished()
        {
            mainCam.GetComponent<Camera>().fieldOfView = originalCamFov;
            mainCam.transform.localPosition = originalCamPos;

            aimCamPosition = originalAimOffsetCamPos;
            aimFinished = true;
        }
        void LerpAimIn()
        {
            if (aimTimeElapsed < aimInOutDuration)
            {
                mainCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(originalCamFov, zoomInAmount, aimTimeElapsed / aimInOutDuration);
                mainCam.transform.localPosition = Vector3.Lerp(originalCamPos, aimCamPosition, aimTimeElapsed / aimInOutDuration);

                aimTimeElapsed += Time.deltaTime;
            }
            else
            {
                mainCam.GetComponent<Camera>().nearClipPlane = 0.01f;
                mainCam.GetComponent<Camera>().fieldOfView = zoomInAmount;
                mainCam.transform.localPosition = new Vector3(aimCamPosition.x, aimCamPosition.y, aimCamPosition.z);
            }
        }
        void LerpAimOut()
        {
            if (aimTimeElapsed < aimInOutDuration)
            {
                mainCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(zoomInAmount, originalCamFov, aimTimeElapsed / aimInOutDuration);
                mainCam.transform.localPosition = Vector3.Lerp(aimCamPosition, originalCamPos, aimTimeElapsed / aimInOutDuration);

                aimTimeElapsed += Time.deltaTime;
            }
            else
            {
                mainCam.GetComponent<Camera>().fieldOfView = originalCamFov;
                mainCam.transform.localPosition = originalCamPos;
            }
        }


    }
}