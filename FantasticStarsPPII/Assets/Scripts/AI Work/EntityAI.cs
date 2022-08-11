using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    [Header("----- Adjustable Fields -----")]
    [SerializeField] GameObject FOV_Object;
    [SerializeField] float fieldOfView;
    [SerializeField] float viewDistance;
    [SerializeField] float objectDetectionRange;
    [SerializeField] float runSpeed;  
    [SerializeField] int pathHomeTimer_Range;
    [SerializeField] bool HunterMode;
    [SerializeField] bool PatrolMode;
    [SerializeField] bool LookoutMode;


    [Header("----- Character General -----")]
    private CharacterController controller;
    private LineRenderer FOV_LR;
    private bool cleanupOnDeath = true;
    private bool cycleHitList = false;
    
    [Header("----- Character Lists -----")]
    [SerializeField] ushort Target;
    [SerializeField] List<ushort> HitList = new List<ushort>();
    [SerializeField] List<ushort> Enemies = new List<ushort>();
    [SerializeField] List<ushort> Allies = new List<ushort>();
    //[SerializeField] List<GameObject> environmentObjects;
    //[SerializeField] List<GameObject> abilityObjects;

    [Header("----- Home Parameters -----")]
    [SerializeField] Vector3 homePoint;
    [SerializeField] float homePointDirection;
    [SerializeField] float pathHomeTimer;

    [Header("----- Destinations -----")]
    [SerializeField] Vector3 nextMoveDestination;
    //[SerializeField] bool atDestination = false;
    [SerializeField] float destinationAngle;
    [SerializeField] float targetAngle;
    //[SerializeField] bool hasTarget;

    [Header("----- Modes -----")]
    [SerializeField] bool CombatMode;
    [SerializeField] bool WanderMode;

    [Header("----- Character Controller Fields -----")]
    [SerializeField] float rotationSpeed = 5;
    [SerializeField] float buoyancyValue = 40;
    //[SerializeField] float jumpForce = 2;
    [SerializeField] float characterFallSpeed;
    private Vector3 moveInputs = Vector3.zero;

    [Header("----- Head Pivot Parameters -----")]
    [SerializeField] float headPivotTime = 2;
    [SerializeField] float headPivot_LookoutRange;
    [Range(0, 90)] [SerializeField] float headPivot_OffsetMax = 80;
    [SerializeField] float headPivot_OffsetCur;
    [SerializeField] float headPivotSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        FOV_Prototype_Initialization();
        GetComponent<SphereCollider>().radius = objectDetectionRange;

        //y axis Magnitudes Causes Problems, only set homepoint when grounded
        if (controller.isGrounded)
            homePoint = transform.position;
        else
            StartCoroutine(SetHomePoint());
        
        homePointDirection = GlobalAngularDisplacement(transform.forward);
        
    }

    private void FixedUpdate()
    {
        //Only do if alive
        if (GetComponent<Entity>().IsAlive())
        {
            //Debug Code - Field of View sight Lines
            FOV_Prototype_Update();
            //Detect Enemy
            Check_EnterCombatMode();

            //Temp Code:: to Keep allies tracking uptodate.
            #region Allies List Cleaner Temp Code
            if (Allies.Count > 0)
            {
                List<ushort> newAllies = new List<ushort>();
                foreach (ushort ally in Allies)
                {
                    if (gameManager.instance.ContainsSpawn(ally))
                        newAllies.Add(ally);
                }
                Allies = newAllies;
            }
            #endregion


            if (CombatMode)
            {
                WanderMode = false;
                headPivot_OffsetCur = 0;//probabably delete this

                if (pathHomeTimer <= 0)
                {
                    //Delay Timer to Return to home point
                    pathHomeTimer = Random.Range(0, pathHomeTimer_Range);
                }

                //Set Destination. Target may not always be the player
                //TODO:: Fancy Destination logic needed
                if (VerfiyTargetSpawnExists()) 
                {
                    nextMoveDestination = gameManager.instance.GetIDPosition(Target); // <----- TODO:: Temporary PlaceHolder solution
                    destinationAngle = RelativeAngle(nextMoveDestination);
                }

                //Combat Head Rotation keeps rotated on Target
                LockHeadToTarget(destinationAngle);

                //Rotate towards Destination
                //if((int)destinationAngle > 0)
                transform.Rotate(Vector3.up, destinationAngle * rotationSpeed * Time.fixedDeltaTime);

                //Move to Destination or Attack Here
                if (RetrieveObjectDirection(nextMoveDestination).magnitude > 2)
                {
                    controller.Move(transform.forward * runSpeed * Time.fixedDeltaTime);
                }
                else
                {                
                    if (VerfiyTargetSpawnExists())
                    {
                        IDamageable damageable = gameManager.instance.entitySpawns.GetValueOrDefault(Target).GetGameObject().GetComponent<IDamageable>();
                        damageable.takeDamage(1);
                    }
                }

            }
            else if (WanderMode)
            {
                if (HunterMode)
                {
                    //Search Random cooridnate within specified Radius;
                }
                else if (PatrolMode)
                {
                    //List of vector 3 points;
                }
                else
                {
                    //Mopes around the Zone AI
                    float ranX = Random.Range(-40, 40);
                    float ranZ = Random.Range(-40, 40);
                    homePoint = new Vector3(ranX, transform.position.y, ranZ);
                    WanderMode = false;
                    if(pathHomeTimer <= 0)
                    {
                        pathHomeTimer = Random.Range(0, pathHomeTimer_Range);
                    }

                }

                if (LookoutMode)
                {
                    //Simulates Head Movement while out of Combat
                    DoLookOutMode();
                }
            }
            else //End of Combat Logic Loop
            {
                //Path Back to Home Point
                if (pathHomeTimer > 0)
                {
                    pathHomeTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    //TODO:: Magnitude is Compensating for the Y axis because I'm using Primitive models to test.
                    if (RetrieveObjectDirection(homePoint).magnitude > 0.2f)
                    {

                        if (Mathf.Abs((int)RelativeAngle(homePoint)) > 5)
                            transform.Rotate(Vector3.up, RelativeAngle(homePoint) * Time.fixedDeltaTime);
                        else
                        {
                            transform.Rotate(Vector2.up, RelativeAngle(homePoint));
                            controller.Move(transform.forward * (runSpeed / 2) * Time.fixedDeltaTime);
                        }
                    }
                    else
                    {
                        transform.position = homePoint;
                        if ((int)RelativeAngle(homePointDirection) > 2)
                            transform.Rotate(Vector3.up, RelativeAngle(homePointDirection) * Time.fixedDeltaTime);
                        else
                        {
                            transform.Rotate(Vector3.up, RelativeAngle(homePointDirection));
                            WanderMode = true;
                        }
                    }

                }

            }
        }
        else
        {
            //If Dead
            if (cleanupOnDeath)
            {
                Destroy(FOV_Object);
                GetComponent<SphereCollider>().enabled = false;
                Allies = null;
                Enemies = null;
                HitList = null;

                cleanupOnDeath = false;
                
            }
        }
        
        //simple Gravity
        if (controller.isGrounded)
        {
            characterFallSpeed = 0;
        }
        else
            characterFallSpeed -= (1 / buoyancyValue) * Time.fixedDeltaTime;

        moveInputs = transform.up * characterFallSpeed;
        controller.Move(moveInputs);
    }
    /*******************************************/
    /*          (Collider Triggers)            */
    /*******************************************/
    #region AI List Managment
        /*******************************************/
        /*     (Trigger)Add Objects to Lists       */
        /*******************************************/
    #region List Updater(Add)
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.GetComponent<Entity>() != null)
        {
            //TODO:: Clean UP
            ushort otherSpawnID = other.gameObject.GetComponent<Entity>().GetSpawnID();
            ushort thisSpawnID = GetComponent<Entity>().GetSpawnID();
            EntityManager otherEntity;
            EntityManager thisEntity = gameManager.instance.entitySpawns.GetValueOrDefault(thisSpawnID);

            if (gameManager.instance.entitySpawns.TryGetValue(otherSpawnID, out otherEntity))
            {

                //if Either is dead
                if (!otherEntity.GetEntity().IsAlive() || !GetComponent<Entity>().IsAlive())
                {

                    //Ignore Corpses for Now
                    //Debug.Log("Corpse"); 
                }
                else if (otherEntity.GetEntity().faction == thisEntity.GetEntity().faction)
                {
                    //Not Self
                    if (otherEntity.GetGameObject() != gameObject)
                    {
                        //Its an Ally
                        if (!Allies.Contains(otherSpawnID))
                        {
                            Allies.Add(otherSpawnID);
                        }
                    }
                }
                else if (otherEntity.GetEntity().faction != thisEntity.GetEntity().faction)
                {
                    //Its an Enemey
                    if (!Enemies.Contains(otherSpawnID))
                    {
                        Enemies.Add(otherSpawnID);
                    }

                }
                else
                    Debug.Log("Unresolved Case Adding SpawnID to NPC Lists");
            }
            else
                //It has an Entity Script, It should be in gameManager Spawns Dictionary
                Debug.Log("ID Not in GameManager");
        }
    }
    #endregion
        /*******************************************/
        /*      (Trigger) Remove from Lists        */
        /*******************************************/
    #region List Updater(Remove)
    private void OnTriggerExit(Collider other)
    {
        //Purpose of this method is to remove unnessary objects from Lists.
        //No code to change behavior is in this method.
        if (other.gameObject.GetComponent<Entity>() != null)
        {
            ushort otherSpawnID = other.gameObject.GetComponent<Entity>().GetSpawnID();
            if(GetComponent<Entity>().IsAlive())
            {
                Allies.Remove(otherSpawnID);
                Enemies.Remove(otherSpawnID);
            }

        }
    }
    #endregion
    #endregion

    /*******************************************/
    /*          AI Behavior Methods            */
    /*******************************************/
    #region Methods that Define Behaviors

    /*******************************************/
    /*             Behavior Methods            */
    /*******************************************/
    #region Check Enemy positions from List
    private void Check_EnterCombatMode()
    {
        
        if (HitList.Count > 0)
        {
            //Keep HitList Clean

            if (cycleHitList)
            {
                List<ushort> newList = new List<ushort>();
                foreach (ushort target in HitList)
                {
                    if (gameManager.instance.ContainsSpawn(target))
                        newList.Add(target);
                }
                Allies = newList;
                cycleHitList = false;
            }

            //Temporary targeting Code until an Aggro system is invented
            if (gameManager.instance.entitySpawns.ContainsKey(HitList[0]))
            {
                Target = HitList[0]; 
                CombatMode = true;
            }
            else
                HitList.RemoveAt(0);

        }
        else if (!CombatMode)
        {

            if (Enemies.Count > 0)
            {
                List<ushort> UpdatedList = new List<ushort>(4);
                //Check Enemy List
                foreach(ushort enemyID in Enemies)
                {
                    if (gameManager.instance.entitySpawns.ContainsKey(enemyID))
                    {
                        UpdatedList.Add(enemyID);

                        GameObject enemy = gameManager.instance.entitySpawns.GetValueOrDefault(enemyID).GetGameObject();
                        Vector3 enemyDirection = RetrieveObjectDirection(enemy.transform.position);
                        float globalDisplacement = GlobalAngularDisplacement(enemyDirection);
                        float localDisplacement = RelativeAngle(globalDisplacement);

                        //If Enemy Detected, Enter Combat Mode
                        if (enemyDirection.magnitude <= viewDistance && Mathf.Abs(localDisplacement) + headPivot_OffsetCur <= (fieldOfView / 2))
                        {
                            
                            if (gameManager.instance.entitySpawns.ContainsKey(enemyID))
                            {
                                RaycastHit hit;
                                if(Physics.Raycast(transform.position, enemyDirection.normalized, out hit))
                                {
                                    if (hit.transform.gameObject == enemy)
                                    {
                                        Target = enemyID;
                                        CombatMode = true;
                                        HitList.Add(enemyID);
                                    }
                                }
                            }

                        }
                    }
                }
                Enemies = UpdatedList;
            }
        }
    }
    #endregion
        /*******************************************/
        /*           LookoutMode Behavior          */
        /*******************************************/
    #region Lookout Mode Behavior
    private void DoLookOutMode()
    {
        if (headPivotTime < 0)
        {
            headPivotTime = Random.Range(2, 6);
            headPivot_LookoutRange = Random.Range(-headPivot_OffsetMax, headPivot_OffsetMax);
            headPivotSpeed = Random.Range(12, 25);
        }
        headPivotTime -= Time.fixedDeltaTime;
        SmoothHeading(headPivot_LookoutRange);
    }
    #endregion
        /*******************************************/
        /*           Lock Head to Target          */
        /*******************************************/
    #region Lock Head to Target
    private void LockHeadToTarget(float _Destination)
    {
        //Get Angle to Destination and Get angle to target
        //Head will look at destination point or its target
        if (VerfiyTargetSpawnExists())
        {
            targetAngle = RelativeAngle(gameManager.instance.GetIDPosition(Target));
            headPivotSpeed = 40;
            if (Mathf.Abs(targetAngle) > headPivot_OffsetMax)
                SmoothHeading(destinationAngle);
            else
                SmoothHeading(targetAngle);
        }


    }
    #endregion

    #endregion

    /*******************************************/
    /*          Functionality Methods          */
    /*******************************************/
    #region Functionality Methods

    /*******************************************/
    /*         Sets character Homepoint        */
    /*******************************************/
    #region SetHomePoint() initialization
    public IEnumerator SetHomePoint()
    {
        yield return new WaitForSeconds(1f);
        homePoint = transform.position;
        //gameObject.SetActive(true);
    }
    #endregion
        /*******************************************/
        /*    Gets this Heading in Euler Angles    */
        /*******************************************/
    #region Heading()
    //Returns Euler Angle based on Object Rotation Y Component
    private float Heading()
    {
        
        return transform.rotation.eulerAngles.y;
    }
    #endregion
        /*******************************************/
        /*         Retrieve Object Direction       */
        /*******************************************/
    #region RetrieveTargetDirection()
    //Retrieves direction vector to object
    private Vector3 RetrieveObjectDirection(Vector3 _otherObject)
    {
        //Vector3 direction = new Vector3(
        //    _otherObject.transform.position.x - transform.position.x, 
        //    0f, 
        //    _otherObject.transform.position.z - transform.position.z);
        Vector3 direction = _otherObject - transform.position;
        return direction;
    }
    #endregion
        /*******************************************/
        /*    Difference in Global Euler Angles    */
        /*******************************************/
    #region GlobalAngularDisplacement()
    private float GlobalAngularDisplacement(Vector3 _otherPoint)
    {       
        
        float angle = Mathf.Atan2(_otherPoint.x, _otherPoint.z) * Mathf.Rad2Deg;
        return angle;
    }
    #endregion
        /*******************************************/
        /*     Difference in local EulerAngles     */
        /*******************************************/
    #region RelativeAngle(float)
    private float RelativeAngle(float _globalAngle)
    {
        return Mathf.DeltaAngle(Heading(), _globalAngle);
    }
    #endregion
    #region RelativeAngle(Vector3)
    private float RelativeAngle(Vector3 _desintationPoint)
    {
        float angularDisplacement = GlobalAngularDisplacement(RetrieveObjectDirection(_desintationPoint));
        return Mathf.DeltaAngle(Heading(), angularDisplacement);
    }
    #endregion

        /*******************************************/
        /*        Smooth Heading rotation          */
        /*******************************************/
    #region Realistic Head Movement over instantly snapping to direction
    private void SmoothHeading(float _lookDestination)
    {

        if ((int)headPivot_OffsetCur != (int)_lookDestination)
        {
            if (headPivot_OffsetCur < _lookDestination)//CW
            {
                headPivot_OffsetCur = headPivot_OffsetCur + (Time.fixedDeltaTime * headPivotSpeed);
            }
            else if (headPivot_OffsetCur > _lookDestination)//CCW
            {
                headPivot_OffsetCur = headPivot_OffsetCur - (Time.fixedDeltaTime * headPivotSpeed);
            }
        }

    }
    #endregion

        /*******************************************/
        /*        Quick Call to GameManager        */
        /*******************************************/
    #region VerifyTargetSpawnExists()
    private bool VerfiyTargetSpawnExists()
    {
        bool isThere = false;
        if (gameManager.instance.entitySpawns.ContainsKey(Target))
        {
            isThere = true;
        }
        else
        {
            //If not in world, clean Lists
            Allies.Remove(Target);
            Enemies.Remove(Target);
            Target = 0;
            CombatMode = false;
        }
        return isThere;
    }
    #endregion

    #endregion

    /*******************************************/
    /*      Debugging Field of View Lines      */
    /*******************************************/
    #region Field of View Displays
    /*******************************************/
    /*       Field of View Initialization      */
    /*******************************************/
    private void FOV_Prototype_Initialization()
    {
        //FOV_Object = new GameObject();
        //FOV_Object.name = "Entity FOV";
        FOV_LR = FOV_Object.AddComponent<LineRenderer>();

        FOV_LR.name = "FOV_Draw";
        FOV_LR.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        //FOV_LR.material = new Material(Shader.);
        FOV_LR.positionCount = 12;
        FOV_LR.startColor = Color.red;
        FOV_LR.endColor = Color.red;
        FOV_LR.startWidth = 0.1f;
        FOV_LR.endWidth = 0.1f;
        FOV_LR.loop = true;
        FOV_Prototype_Update();
    }
        /*******************************************/
        /*   Update Field of View lines to object  */
        /*******************************************/
    private void FOV_Prototype_Update()
    {
        FOV_LR.SetPosition(0, transform.position);
        for(int i = 0; i <= 10; i++)
        {
            //Field of View in Euler Angles      
            float theta = Mathf.Deg2Rad * (i * (fieldOfView / 10) + Heading() + headPivot_OffsetCur - (fieldOfView / 2));
            Vector3 fov_Point = new Vector3(
                Mathf.Sin(theta), 
                0f, 
                Mathf.Cos(theta)) * viewDistance;

            FOV_LR.SetPosition(i + 1, fov_Point + transform.position);
              
        }
    }
    #endregion
}
