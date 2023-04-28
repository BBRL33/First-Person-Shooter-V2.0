using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //Camera
    public Transform viewPoint;
    private Camera cam;


    //Mouse Controls
    public float mouseSens = 1f;
    private float verticalRot;
    private Vector2 mouseInput;
    public bool invertedControls;


    //Animation
    public Animator anim;
    public GameObject playerModel;


    //Walking
    public float moveSpeed = 5f, runSpeed = 8f, sneakSpeed = 2f;
    private float activeMoveSpeed;
    private Vector3 moveDirection, movement;
    public CharacterController charControl;


    //Jumping
    public float jumpForce = 12f, gravityMod = 2.5f;
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;
    private bool shifting;


    //Shooting
    public Transform modelGunPoint, gunHolder;
    public GameObject bulletImpact;
    private float shotCounter;
    public float maxHeat = 10f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overheat;
    public GunHandling[] allGuns;
    private int selectedGun;
    private float muzzleDisTime, muzzleCounter;
    public GameObject bloodParticles;
    public float scopeSpeed = 5;
    public Transform scopeIn;
    public Transform scopeOut;

    //Health
    public int maxHP = 100;
    private int HP;

    //Outfit
    public Material[] playerSkin;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        UIController.instance.heatCap.maxValue = maxHeat;
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        HP = maxHP;
        if (photonView.IsMine)
        {
            playerModel.SetActive(false);
            UIController.instance.health.maxValue = maxHP;
            UIController.instance.health.value = HP;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
        playerModel.GetComponent<Renderer>().material = playerSkin[photonView.Owner.ActorNumber % playerSkin.Length];
    }

    void Update()
    {
        if(photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSens;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
            verticalRot += mouseInput.y;
            verticalRot = Mathf.Clamp(verticalRot, -60f, 60f);
            if (invertedControls)
            {
                viewPoint.rotation = Quaternion.Euler(verticalRot, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-verticalRot, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(KeyCode.LeftControl))
            {
                activeMoveSpeed = runSpeed;
                shifting = false;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = sneakSpeed;
                shifting = true;
            }
            else
            {
                activeMoveSpeed = moveSpeed;
                shifting = false;
            }
            float yVelocity = movement.y;
            movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
            movement.y = yVelocity;
            if (charControl.isGrounded)
            {
                movement.y = 0f;
            }
            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 1.5f, groundLayers);
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }
            if (shifting)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    movement.y = jumpForce;
                }
            }
            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
            charControl.Move(movement * Time.deltaTime);
            if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);
                }
            }
            if (!overheat)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                    if (allGuns[selectedGun].isDouble)
                    {
                        StartCoroutine(WaitSec());
                    }
                }
                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAuto)
                {
                    shotCounter -= Time.deltaTime;
                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    overheat = false;
                }
            }
            if (heatCounter < 0)
            {
                heatCounter = 0f;
            }
            UIController.instance.heatCap.value = heatCounter;
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                selectedGun++;
                if (selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                selectedGun--;
                if (selectedGun < 0)
                {
                    selectedGun = allGuns.Length - 1;
                }
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
            for (int i = 0; i < allGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
            }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDirection.magnitude);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0) && !UIController.instance.pauseScreen.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            if (Input.GetMouseButton(1))
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,allGuns[selectedGun].scope, scopeSpeed * Time.deltaTime);
                //gunHolder.position = Vector3.Lerp(gunHolder.position,scopeIn.position,scopeSpeed*Time.deltaTime);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60, scopeSpeed * Time.deltaTime);
                //gunHolder.position = Vector3.Lerp(gunHolder.position, scopeOut.position, scopeSpeed * Time.deltaTime);
            }
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(bloodParticles.name,hit.point,Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("dealDamage",RpcTarget.All,photonView.Owner.NickName,allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal,Vector3.up));
            Destroy(bulletImpactObject, 10f);
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;
        heatCounter += allGuns[selectedGun].heatPerShots;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overheat = true;
        }
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisTime;
        allGuns[selectedGun].hit.Stop();
        allGuns[selectedGun].hit.Play();
    }
    [PunRPC]
    public void dealDamage(string damager, int bulleee, int actor)
    {
        takeDamage(damager,bulleee, actor);
    }
    public void takeDamage(string damager, int shoooeeee, int actor)
    {
        if (photonView.IsMine)
        {
            HP -= shoooeeee;
            if (HP <= 0)
            {
                HP = 0;
                playerSpawner.instance.die(damager);
                matchManager.instance.UpdateStatsSend(actor, 0,1);
            }
            UIController.instance.health.value = HP;
        }
    }

    private void LateUpdate()
    {
        if(photonView.IsMine)
        {
            if (matchManager.instance.state == matchManager.GameSate.Playing)
            {
                cam.transform.rotation = viewPoint.rotation;
                cam.transform.position = viewPoint.position;
            }
            else
            {
                cam.transform.position = matchManager.instance.mapCamPoint.position;
                cam.transform.rotation = matchManager.instance.mapCamPoint.rotation;
            }
        }
    }
    void switchGun()
    {
        foreach (GunHandling gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            switchGun();
        }
    }

    IEnumerator WaitSec()
    {
        yield return new WaitForSeconds(0.1f);
        Shoot();
    }
}
