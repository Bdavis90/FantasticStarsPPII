using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
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
    [Range(1, 3)] [SerializeField] int jumpsMax;

    [Header("-----Weapon Stats-----")]
    [Range(0.1f, 5)] [SerializeField] float shootRate;
    [Range(1, 30)] [SerializeField] int shootDist;
    [Range(1, 10)] [SerializeField] int shootDamage;
    //[SerializeField] List<gunStats> gunstat = new List<gunStats>();

    private Vector3 playerVelocity;
    Vector3 move = Vector3.zero;
    int timesJumped;
    float playerspeedOrig;
    bool isSprinting = false;
    bool isShooting = false;
    bool isleaningRight = false;
    bool isleaningLeft = false;


    private void Start()
    {
        playerspeedOrig = playerSpeed;
    }

    void Update()
    {
        playerMovement();
        sprint();

        //how u call an Ienumerator
        //StartCoroutine(shoot());
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

        if (Input.GetButtonDown("Lean Left"))
        {
            transform.localRotation = Quaternion.Euler(0, 0, 20);
        }
        if (Input.GetButtonUp("Lean Left"))
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        if (Input.GetButtonDown("Lean Right"))
        {
            transform.localRotation = Quaternion.Euler(0, 0, -20);
        }
        if (Input.GetButtonUp("Lean Right"))
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

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

    //timer func
    //needs to return a 'yield'
    //a timer that makes sense
    // set a time... set a bool... do something for some time... reset the bool... etc etc etc
    //IEnumerator shoot()
    //{
    //    Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red, 0.000001f);
    //    if (gunstat.Count != 0 && Input.GetButton("Shoot") && !isShooting)
    //    {
    //        isShooting = true;
    //        //raycast is using physic lib
    //        RaycastHit hit; //returns information of what we hit
    //        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
    //        {
    //            if (hit.collider.GetComponent<IDamageable>() != null)
    //            {
    //                IDamageable isDamageable = hit.collider.GetComponent<IDamageable>();

    //                isDamageable.takeDamage(shootDamage);
    //            }
    //        }
    //        //this is the yield return to get the ienumerator to stop complaining
    //        //"do something wait and do it again"
    //        yield return new WaitForSeconds(shootRate);

    //        isShooting = false;

    //    }
    //}

    //public void gunPickup(float shootrate, int shootdist, int shootdamage, gunStats stats)
    //{
    //    shootRate = shootrate;
    //    shootDist = shootdist;
    //    shootDamage = shootdamage;
    //    gunstat.Add(stats);
    //}
}