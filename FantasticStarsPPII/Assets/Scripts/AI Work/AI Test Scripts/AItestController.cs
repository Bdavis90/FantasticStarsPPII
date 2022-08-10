using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AItestController : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] Camera theCamera;
    private Vector3 moveInputs = Vector3.zero;
    [SerializeField] float baseSpeed;

    [SerializeField] float sprintSpeed = 1;
    private bool isSprinting;

    [SerializeField] float buoyancyValue = 40;
    [SerializeField] float jumpForce = 2;
    [SerializeField] float playerFallSpeed;
    [SerializeField] float terminalVelocity = 0.01f;
    [SerializeField] int playerJumpsMax;
    [SerializeField] int playerJumps = 0;

    private float creatureFireRate = 0.2f;
    private bool isDwarfMaking;
    private bool isOrglingMaking;
    [SerializeField] GameObject dwarfPrefab;
    [SerializeField] GameObject orglingPrefab;
    private float shootDistance = 30;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    void Update()
    {
        PlayerMovement();
        //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red, 0.00000001f);
        //Debug.DrawRay()
        StartCoroutine(MakeDwarf());
        StartCoroutine(MakeOrgling());
    }

    void PlayerMovement()
    {
        if (playerJumps < playerJumpsMax && Input.GetButtonDown("Jump"))
        {
            //buoancy is how floaty the character is
            //Jump force is how high he can jump
            //Character will fall faster over time,

            //Adds upward Force
            playerFallSpeed = jumpForce * Time.fixedDeltaTime;
            playerJumps++;
        }
        else //if(playerForces.y > -terminalVelocity)
        {

            if (controller.isGrounded)
            {
                if (playerFallSpeed < -terminalVelocity)
                {
                    //TODO:: Fall Damage Based on Force.y value
                    //Debug.Log("Fall Damage Simulator:" + playerFallSpeed);
                }
                playerFallSpeed = 0;
                playerJumps = 0;
            }
            else
                playerFallSpeed -= (1 / buoyancyValue) * Time.fixedDeltaTime;
        }

        float speed = (baseSpeed * Sprint()) * Time.deltaTime;


        moveInputs = transform.right * speed * Input.GetAxis("Horizontal") +
                     transform.up * playerFallSpeed +
                     transform.forward * speed * Input.GetAxis("Vertical");

        controller.Move(moveInputs);

    }

    private float Sprint()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            isSprinting = true;
            sprintSpeed = 2;
        }

        if (Input.GetButtonUp("Fire3"))
        {
            isSprinting = false;
            sprintSpeed = 1;
        }

        return sprintSpeed;
    }

    IEnumerator MakeDwarf()
    {

        if (Input.GetButton("Fire1") && !isDwarfMaking)
        {
            isDwarfMaking = true;
            
            RaycastHit hit;
            if (Physics.Raycast(theCamera.ViewportPointToRay(new Vector3(0.5f, 0f, 0.5f)), out hit, shootDistance))
            {
                Vector3 snapPosition = new Vector3(hit.point.x, hit.point.y + 1.0f, hit.point.z);
                Instantiate(dwarfPrefab, snapPosition, dwarfPrefab.transform.rotation);

            }
            yield return new WaitForSeconds(creatureFireRate);
            isDwarfMaking = false;
        }

    }

    IEnumerator MakeOrgling()
    {

        if (Input.GetKeyDown(KeyCode.Mouse2) && !isOrglingMaking)
        {
            isOrglingMaking = true;

            RaycastHit hit;
            if (Physics.Raycast(theCamera.ViewportPointToRay(new Vector3(0.5f, 0f, 0.5f)), out hit, shootDistance))
            {
                Vector3 snapPosition = new Vector3(hit.point.x, hit.point.y + 1.0f, hit.point.z);
                Instantiate(orglingPrefab, snapPosition, orglingPrefab.transform.rotation);
            }
            yield return new WaitForSeconds(creatureFireRate);
            isOrglingMaking = false;
        }

    }
}
