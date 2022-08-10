using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    private CharacterController controller;
    /*******************************************/
    /*         Prototype / Debug Members       */
    /*******************************************/
    private GameObject FOV_Object;
    private LineRenderer FOV_LR;
    private bool isAlive = true;
    private bool cleanupOnDeath = true;
    /*******************************************/
    /*         Object Detection Members        */
    /*******************************************/
    [Header("----- Entity Lists -----")]
    #region Members for Detection Lists
    [SerializeField] ushort spawnID;
    [SerializeField] float objectDetectionRange;
    [SerializeField] List<ushort> HitList = new List<ushort>();
    [SerializeField] List<ushort> Enemies = new List<ushort>();
    [SerializeField] List<ushort> Allies = new List<ushort>();
    //[SerializeField] List<GameObject> hitList;
    //[SerializeField] List<GameObject> enemies;
    //[SerializeField] List<GameObject> allies;
    [SerializeField] List<GameObject> environmentObjects;
    [SerializeField] List<GameObject> abilityObjects;
    #endregion

    [Header("----- Destinations -----")]
    [SerializeField] ushort Target;
    //[SerializeField] GameObject target;
    [SerializeField] Vector3 nextMoveDestination;
    [SerializeField] float destinationAngle;
    [SerializeField] float targetAngle;
    [SerializeField] bool hasTarget;
    [SerializeField] Vector3 homePoint;
    [SerializeField] float homePointDirection;
    [SerializeField] float homePointDelay;
    private float rotationSpeed = 5;

    [Header("----- Modes -----")]
    [SerializeField] bool CombatMode;
    [SerializeField] bool WanderMode;
    [SerializeField] bool HunterMode;
    [SerializeField] bool PatrolMode;
    [SerializeField] bool LookoutMode;

    [Header("----- Head Pivot Parameters -----")]
    [SerializeField] float headPivotTime = 2;
    [SerializeField] float headPivot_LookoutRange;
    [Range(0, 90)] [SerializeField] float headPivot_OffsetMax = 80;
    [SerializeField] float headPivot_OffsetCur;
    [SerializeField] float headPivotSpeed;

    [Header("----- Field of View Data -----")]
    [SerializeField] float fieldOfView;
    [SerializeField] float viewDistance;
    [SerializeField] float runSpeed;
    [Header("----- Character Controller Fields -----")]
    private Vector3 moveInputs = Vector3.zero;
    [SerializeField] float buoyancyValue = 40;
    [SerializeField] float jumpForce = 2;
    [SerializeField] float characterFallSpeed;

    public void SetSpawnID(ushort _id)
    {
        spawnID = _id;
    }

    public void SetAlive(bool _value)
    {
        isAlive = _value;
    }

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

    private void Update()
    {
        if (!isAlive && cleanupOnDeath)
        {
            Destroy(FOV_Object);
            GetComponent<SphereCollider>().enabled = false;
            Allies = null;
            Enemies = null;
            HitList = null;         
            
            cleanupOnDeath = false;

        }
    }

    private void FixedUpdate()
    {
        //Only do if alive
        if (isAlive)
        {
            FOV_Prototype_Update();
            //Behavior to Detect and Engage Enemy
            Check_EnterCombat();

            if (CombatMode)
            {
                WanderMode = false;
                headPivot_OffsetCur = 0;//probabably delete this

                if (homePointDelay <= 0)
                {
                    //Delay Timer to Return to home point
                    homePointDelay = 2;
                }

                //Combat Mode means the Targets have been identified
                //Entity needs to relocate from his position to a destination.

                //Set Destination. Target may not always be the player
                //TODO:: Fancy Destionion logic needed
                //DELETE THIS
                //nextMoveDestination = target.transform.position;
                if (VerfiyTargetSpawnExists())
                {
                    nextMoveDestination = gameManager.instance.spawns.GetValueOrDefault(Target).GetGameObject().transform.position;
                }

                //It seem my Move might able to be put in a Imoveable or method
                //Each else will only need to solve destinations.

                //Get Angle to Destination and Get angle to target
                //Head will look at destination point or its target
                destinationAngle = RelativeAngle(nextMoveDestination);
                //TODO Not the right reference Points because the target is always the destination right now.
                targetAngle = RelativeAngle(nextMoveDestination);

                headPivotSpeed = 40;
                if (Mathf.Abs(targetAngle) > headPivot_OffsetMax)
                {
                    SmoothHeading(destinationAngle);
                    //headPivot_OffsetCur = destinationAngle;
                }
                else
                    SmoothHeading(targetAngle);
                //headPivot_OffsetCur = targetAngle;

                //Rotate towards Destination
                //if((int)destinationAngle > 0)
                transform.Rotate(Vector3.up, destinationAngle * rotationSpeed * Time.fixedDeltaTime);

                //Move to Destination
                //Defined destination will be different for enemy types.

                if (RetrieveObjectDirection(nextMoveDestination).magnitude > 2)
                {
                    controller.Move(transform.forward * runSpeed * Time.fixedDeltaTime);
                }
                else
                {

                    if (VerfiyTargetSpawnExists())
                    {
                        if (true)//(target.GetComponent<IDamageable>() != null)
                        {
                            //Get Target Damageable
                            IDamageable damageable = gameManager.instance.spawns.GetValueOrDefault(Target).GetGameObject().GetComponent<IDamageable>();
                            if (damageable.takeDamage(3))
                            {
                                //if Alive                     
                            }
                            else
                            {
                                //if Dead    
                                VerfiyTargetSpawnExists();
                                Target = 0;
                                //nextMoveDestination = homePoint;
                                CombatMode = false;
                                //When Target Dies, Turn off Combat Mode
                                //The next fixed frame will determine hit list, destination and modes.
                            }
                        }
                    }


                }

                //MoveObject(transform.forward * runSpeed * Time.deltaTime);

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
                }

                if (LookoutMode)
                {
                    //Simulates Head Movement while out of Combat
                    DoLookOutMode();
                }
            }
            else
            {


                if (homePointDelay > 0)
                {
                    homePointDelay -= Time.fixedDeltaTime;
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
        
        //moveInput
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
    /*          AI Behavior Methods            */
    /*******************************************/
    #region Methods that Define Behaviors

        /*******************************************/
        /*             Behavior Methods            */
        /*******************************************/
    #region Check Enemy positions from List
    private void Check_EnterCombat()
    {
        //Only Check Enemies List if Not in Combat
        if (!CombatMode)
        {

            //TODO::Run Destination Algorithim.
            //Check Hitlist for next Target
            if (HitList.Count > 0)
            {
                if (gameManager.instance.spawns.ContainsKey(HitList[0]))
                {
                    Target = HitList[0];
                    CombatMode = true;
                }
                else
                {

                    HitList.RemoveAt(0);
                }
                //target = gameManager.instance.spawns.GetValueOrDefault(HitList[0]).GetGameObject();
                
                
            }
            else if (Enemies.Count > 0)
            {

                //Check Enemy List
                foreach(ushort enemyID in Enemies)
                {
                    if (gameManager.instance.spawns.ContainsKey(enemyID))
                    {
                        GameObject enemy = gameManager.instance.spawns.GetValueOrDefault(enemyID).GetGameObject();
                        Vector3 enemyDirection = RetrieveObjectDirection(enemy.transform.position);
                        float globalDisplacement = GlobalAngularDisplacement(enemyDirection);
                        float localDisplacement = RelativeAngle(globalDisplacement);

                        //If Enemy Detected, Enter Combat Mode
                        if (enemyDirection.magnitude <= viewDistance && Mathf.Abs(localDisplacement) + headPivot_OffsetCur <= (fieldOfView / 2))
                        {
                            RaycastHit hit;
                            Physics.Raycast(transform.position, enemyDirection.normalized, out hit);
                            if (hit.transform.gameObject == enemy.gameObject)
                            {
                                Target = enemyID;
                                CombatMode = true;
                                HitList.Add(enemyID);
                            }
                        }
                    }
                    else
                    {
                        //Enemies.Remove(enemyID);
                    }
                }
            }
        }
    }

    //DELETE CODE
    //private void SearchListForEnemies(List<GameObject> enemies)
    //{
    //    foreach (GameObject enemy in enemies)
    //    {
    //        //Gather Cooridinates Data on Enemies
    //        Vector3 enemyDirection = RetrieveObjectDirection(enemy.transform.position);
    //        float globalDisplacement = GlobalAngularDisplacement(enemyDirection);
    //        float localDisplacement = RelativeAngle(globalDisplacement);

    //        //If Enemy Detected, Enter Combat Mode
    //        if (enemyDirection.magnitude <= viewDistance && Mathf.Abs(localDisplacement) + headPivot_OffsetCur <= (fieldOfView / 2))
    //        {
    //            RaycastHit hit;
    //            //TODO:: Raycast to Detect Line of Sight ??
    //            Physics.Raycast(transform.position, enemyDirection.normalized, out hit);
    //            Debug.Log(hit.transform.gameObject);
    //            if (hit.transform.gameObject == enemy.gameObject)
    //            {
                    
    //                target = enemy;
    //                CombatMode = true;
    //                hitList.Add(enemy);
    //            }
    //        }
    //    }
    //}

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
    /*       Move Object to Desintation        */
    /*******************************************/
    private void MoveObject(Vector3 _moveDirection)
    {
        MoveObject(_moveDirection);
    }
    #endregion

    #endregion

    /*******************************************/
    /*          Functionality Methods          */
    /*******************************************/
    #region Functionality Methods

    public IEnumerator SetHomePoint()
    {
        yield return new WaitForSeconds(2f);
        homePoint = transform.position;
        //gameObject.SetActive(true);
    }
    private bool VerfiyTargetSpawnExists()
    {
        bool inspawnManager = false;
        if (gameManager.instance.spawns.ContainsKey(Target))
        {
            inspawnManager = true;
        }
        else
        {
            //If not in world, clean Lists
            Allies.Remove(Target);
            Enemies.Remove(Target);
            Target = 0;
            CombatMode = false;
        }
        return inspawnManager;
    }
        /*******************************************/
        /*          Add Objects to Lists           */
        /*******************************************/
    #region List Updater(Add)
    private void OnTriggerEnter(Collider other)
    {
        
        //Debug.Log("Trigger Entered");
        if(other.gameObject.GetComponent<Entity>() != null)
        {
            
            ushort otherSpawnID = other.gameObject.GetComponent<Entity>().GetSpawnID();
            EntityManager otherEntity;
            EntityManager thisEntity = gameManager.instance.spawns.GetValueOrDefault(spawnID);

            if(gameManager.instance.spawns.TryGetValue(otherSpawnID, out otherEntity))
            {
                if (otherEntity.GetEntity().faction == Entity_Faction.Corpse)
                {
                    //Ignore Corpses for Now
                    //Debug.Log("Corpse"); 
                }
                else if(otherEntity.GetEntity().faction == thisEntity.GetEntity().faction)
                {
                    //Not Self
                    if (otherSpawnID != spawnID )
                    {
                        //Its an Ally
                        if (!Allies.Contains(otherSpawnID))
                        {
                            Allies.Add(otherSpawnID);
                        }
                        
                    }
                    
                }
                else if(otherEntity.GetEntity().faction != thisEntity.GetEntity().faction)
                {
                    //Its an Enemey
                    if (!Enemies.Contains(otherSpawnID))
                    {
                        Enemies.Add(otherSpawnID);
                    }
                    
                }
                else
                {
                    Debug.Log("Unresolved Case Adding SpawnID to NPC Lists");
                }
            }
            else
            {
                //It has an Entity Script, It should be in gameManager Spawns Dictionary
                Debug.Log("ID Not in GameManager");
            }          
        }
        else
        {
            Debug.Log("Instantiated Object Test");
        }
    }
    #endregion
        /*******************************************/
        /*        Remove Objects from Lists        */
        /*******************************************/
    #region List Updater(Remove)
    private void OnTriggerExit(Collider other)
    {
        //Purpose of this method is to remove unnessary objects from Lists.
        //No code to change behavior is in this method.
        if(other.gameObject.GetComponent<Entity>() != null)
        {
            ushort otherSpawnID = other.gameObject.GetComponent<Entity>().GetSpawnID();
            Allies.Remove(otherSpawnID);
            Enemies.Remove(otherSpawnID);
        }
    }
    #endregion

        /*******************************************/
        /*    Gets this Heading in Euler Angles    */
        /*******************************************/
    #region Get Object Heading
    //Returns Euler Angle based on Object Rotation Y Component
    private float Heading()
    {
        
        return transform.rotation.eulerAngles.y;
    }
    #endregion

        /*******************************************/
        /*         Retrieve Object Direction       */
        /*******************************************/
    #region Retrieve Target Direction
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
        /*        Retrieve Euler Displacment       */
        /*******************************************/
    #region Get Angular Discplacement to object
    private float GlobalAngularDisplacement(Vector3 _otherPoint)
    {       
        
        float angle = Mathf.Atan2(_otherPoint.x, _otherPoint.z) * Mathf.Rad2Deg;
        return angle;
    }
    #endregion
        /*******************************************/
        /*         Relative Angle to object        */
        /*******************************************/

    #region Retrieve Angle from Heading to Object
    //Heading Angle to object angular discplacement
    private float RelativeAngle(float _globalAngle)
    {
        return Mathf.DeltaAngle(Heading(), _globalAngle);
    }
    #endregion

    /*******************************************/
    /*      Quick Relative Angle to object     */
    /*******************************************/
    #region Relative Angle by calling all three methods
    private float RelativeAngle(Vector3 _desintationPoint)
    {
        float angularDisplacement = GlobalAngularDisplacement(RetrieveObjectDirection(_desintationPoint));
        return Mathf.DeltaAngle(Heading(), angularDisplacement);
    }
    #endregion
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
    /*      Debugging Field of View Lines      */
    /*******************************************/
    #region Field of View Displays
    /*******************************************/
    /*       Field of View Initialization      */
    /*******************************************/
    private void FOV_Prototype_Initialization()
    {
        FOV_Object = new GameObject();
        FOV_Object.name = "Entity FOV";
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
