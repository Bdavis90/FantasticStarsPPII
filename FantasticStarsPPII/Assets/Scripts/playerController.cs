using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamageable
{

    [Header("-----Components-----")]
    //serilizefield allows the inspector to have a field that I can drag a component onto
    //public does the same thing, but serialized is like the private version
    [SerializeField] CharacterController controller;

    [Header("-----Player Attributes-----")]
    //this creates drop downs in the inspector that the editor can change
    [Range(1, 10)] [SerializeField] float playerSpeed;
    [Range(1, 4)] [SerializeField] float sprintMult;
    [Range(8, 20)] [SerializeField] float jumpHeight;
    [Range(15, 30)] [SerializeField] float gravityValue;

    [SerializeField] List<healthStats> healthStat = new List<healthStats>();
    [Range(0, 10)] public int hp;
    [Range(1, 3)] [SerializeField] int jumpsMax;


    [Header("-----Weapon Stats-----")]
    [Range(0.1f, 5)] [SerializeField] float shootRate;
    [Range(1, 30)] [SerializeField] int shootDist;
    [Range(1, 10)] [SerializeField] int shootDamage;
    // [SerializeField] List<gunStats> gunstat = new List<gunStats>();

    Vector3 playerDir;

    private Vector3 playerVelocity;
    Vector3 move = Vector3.zero;
    int timesJumped;
    float playerspeedOrig;
    bool isSprinting = false;
    bool isShooting = false;
    private int hpOrig;
    bool isleaningRight = false;
    bool isleaningLeft = false;



    private void Start()
    {
        playerspeedOrig = playerSpeed;
        hpOrig = hp;
    }

    void Update()
    {
        playerMovement();
        sprint();

        //how u call an Ienumerator
        StartCoroutine(shoot());
    }

    void playerMovement()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            timesJumped = 0;
        }

        //get input from Unity input system
        //we want this to be our horizontal or strafe controls
        move = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical"));

        //add our move vector to the character controller
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && timesJumped < jumpsMax)
        {
            playerVelocity.y = jumpHeight;
            timesJumped++;
        }

        //applies the gravity, drags the player down
        playerVelocity.y -= gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


        //if (Input.GetButtonDown("Lean Left"))
        //{
        //    playerDir.x = 0;
        //    playerDir.y = 0;
        //    playerDir.z = 20;
        //    Quaternion rotation = Quaternion.LookRotation(playerDir);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 1);
        //}
        //if (Input.GetButtonUp("Lean Left"))
        //{
        //    transform.localRotation = Quaternion.Euler(0, 0, 0);
        //}

        //if (Input.GetButtonDown("Lean Right"))
        //{
        //    transform.localRotation = Quaternion.Euler(0, 0, -20);
        //}
        //if (Input.GetButtonUp("Lean Right"))
        //{
        //    transform.localRotation = Quaternion.Euler(0, 0, 0);
        //}

    }

    void sprint()
    {
        //get button = button being held down
        //get button down = button is pressed
        //get button up = buton is released
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            playerSpeed = playerSpeed * sprintMult;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            playerSpeed = playerspeedOrig;
        }
    }

    public void respawn()
    {
        controller.enabled = false;
        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true;
    }

    public void death()
    {
        gameManager.instance.cursorLock();
        gameManager.instance.currentMenuOpen = gameManager.instance.playerDeadMenu; ;
        gameManager.instance.currentMenuOpen.SetActive(true);
    }
    IEnumerator damageFlash()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(.01f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }

    public void takeDamage(int damage)
    {
        hp -= damage;
        StartCoroutine(damageFlash());
        if (hp <= 0)
        {
            death();
        }
    }

    public void resetHP()
    {
        hp = hpOrig;
    }

    //timer func
    //needs to return a 'yield'
    //a timer that makes sense
    // set a time... set a bool... do something for some time... reset the bool... etc etc etc
    IEnumerator shoot()
    {
        if (GetComponent<CharacterSheet>().rightHand != null)
        {
            WeaponStats weaponEquipped = GetComponent<CharacterSheet>().rightHand;
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * weaponEquipped.range, Color.red, 0.000001f);
            if (GetComponent<CharacterSheet>().gunBag.Count != 0 && Input.GetButton("Shoot") && !isShooting)
            {
                isShooting = true;
                //raycast is using physic lib
                RaycastHit hit; //returns information of what we hit
                if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
                {
                    if (hit.collider.GetComponent<IDamageable>() != null)
                    {
                        IDamageable isDamageable = hit.collider.GetComponent<IDamageable>();

                        if (hit.collider is SphereCollider)
                        {
                            //head shot
                            isDamageable.takeDamage(weaponEquipped.damage * 2);
                        }
                        else
                        {
                            //body shoot
                            isDamageable.takeDamage(weaponEquipped.damage);
                        }

                    }
                }
                //this is the yield return to get the ienumerator to stop complaining
                //"do something wait and do it again"
                yield return new WaitForSeconds(weaponEquipped.rateOfFire);

                isShooting = false;

            }

        }

    }


    public void healthPickUp(int Hp, healthStats stat)
    {
        hp = Hp;
        healthStat.Add(stat);
    }
}