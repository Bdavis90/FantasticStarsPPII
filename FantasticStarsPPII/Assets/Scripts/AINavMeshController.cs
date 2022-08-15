using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AINavMeshController : MonoBehaviour
{
    [Header("----- Drag N Drops -----")]
    [SerializeField] GameObject childGameObject_FOV;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] SphereCollider radarTrigger;

    [Header("----- Required Fields -----")]
    [Range(1, 101)] [SerializeField] float radarSize;
    [Range(1, 100)] [SerializeField] float viewDistance;
    [Range(1, 360)] [SerializeField] float fieldOfView;
    [Range(0, 90)] [SerializeField] float headPivot_OffsetMax = 80;
    [Range(0, 1000)] [SerializeField] int waitTimer;

    [Header("----- Optionals -----")]
    [SerializeField] bool TurnLookoutOn;
    [SerializeField] bool HunterMode;   
    [SerializeField] bool PatrolMode;
    [SerializeField] List<GameObject> patrolNodePrefabs;
    [SerializeField] int nextPatrolIndex;

    [Header("----- Character General -----")]   
    private LineRenderer FOV_LR;
    private bool cleanupOnDeath = true;
    private bool cycleHitList = false;

    [Header("----- Radar Lists -----")]
    [SerializeField] ushort Target;
    [SerializeField] List<ushort> HitList = new List<ushort>();
    [SerializeField] List<ushort> Enemies = new List<ushort>();
    [SerializeField] List<ushort> Allies = new List<ushort>();

    [Header("----- Hunter Parameters -----")]
    [SerializeField] bool isHunting;
    [SerializeField] bool isTracking;
    //[SerializeField] int walkRadius;

    [Header("----- Patrol Parameters -----")]
    [SerializeField] bool isPatrolling;
    [SerializeField] bool isGuarding;

    [Header("----- Home Parameters -----")]
    [SerializeField] Vector3 homePoint;
    [SerializeField] float homePointDirection;
    [SerializeField] float pathHomeTimer;

    [Header("----- Destinations -----")]
    [SerializeField] Vector3 nextMoveDestination;
    [SerializeField] float destinationAngle;
    [SerializeField] float targetAngle;

    [Header("----- Modes -----")]
    [SerializeField] bool CombatMode;
    [SerializeField] bool WanderMode;

    [Header("----- Character Controller Fields -----")]
    [SerializeField] float rotationSpeed = 5;

    [Header("----- Head Pivot Parameters -----")]
    [SerializeField] float headPivotTime = 2;
    [SerializeField] float headPivot_LookoutRange;
    
    [SerializeField] float headPivot_OffsetCur;
    [SerializeField] float headPivotSpeed;

    public bool TryGetTarget(out Vector3 _targetDirection)
    {
        bool hasTarget = false;
        _targetDirection = new Vector3();
        //Vector3 _targetDirection;
        if (VerfiyTargetSpawnExists())
        {
            hasTarget = true;
            _targetDirection = RetrieveObjectDirection(gameManager.instance.GetIDPosition(Target));

        }
        return hasTarget;
    }

    void Start()
    {
        //agent = GetComponent<NavMeshAgent>();

        FOV_Prototype_Initialization();
        radarTrigger.radius = radarSize;

        homePoint = transform.position;

        homePointDirection = GlobalAngularDisplacement(transform.forward);


        //Initialize List  of PatroleMode
        if (PatrolMode && patrolNodePrefabs.Count > 0)
        {
            if (patrolNodePrefabs.Count < nextPatrolIndex)
            {
                navMeshAgent.SetDestination(patrolNodePrefabs[0].transform.position);
            }
            else
                navMeshAgent.SetDestination(patrolNodePrefabs[nextPatrolIndex].transform.position);
        }
        else
            PatrolMode = false;

    }

    private void FixedUpdate()
    {
        //Only do if alive
        if (GetComponent<CharacterSheet>().isAlive)
        {
            //Debug Code - Field of View sight Lines
            FOV_Prototype_Update();
            //Detect Enemy
            Check_EnterCombatMode();

            //Temp Code:: to Keep allies tracking uptodate.
            #region  Keep Unused Allies list clean here for now
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
                isTracking = false;
                isHunting = false;
                WanderMode = false;
                navMeshAgent.stoppingDistance = 2;
                //headPivot_OffsetCur = 0;//probabably delete this

                if (pathHomeTimer <= 0)
                {
                    //Delay Timer to Return to home point
                    pathHomeTimer = Random.Range(0, waitTimer);
                }

                //Fancy Destination done with NavMesh
                if (VerfiyTargetSpawnExists())
                {
                    navMeshAgent.SetDestination(gameManager.instance.GetIDPosition(Target));
                }
                
                //Combat Head Rotation keeps head rotated on Target
                LockHeadToTarget(RelativeAngle(navMeshAgent.nextPosition));

                if(navMeshAgent.remainingDistance <= 15)//agent.stoppingDistance)
                {
                    LerpRotateToTarget();

                    if (VerfiyTargetSpawnExists())
                    {
                        GetComponent<CharacterSheet>().ShootWeapon();
                        //IDamageable damageable = gameManager.instance.character_Spawns.GetValueOrDefault(Target).GetGameObject().GetComponent<IDamageable>();
                        //damageable.takeDamage(1);
                    }
                }

            }
            else if (WanderMode) //Non-Combat Day Job
            {
                if (HunterMode)
                {
                    //Detects and Hunts nearby Enemies - Can be used with Patrol
                    DoHunterMode();
                }
                else if (PatrolMode)
                {
                    //Patrols a List of predefined Points
                    DoPatrolMode();
                }
                else
                {
                    //Mopes Around zone using end of Combat loop code
                    homePoint = SetRandomPoint();
                    WanderMode = false;
                    if (pathHomeTimer <= 0)
                    {
                        pathHomeTimer = Random.Range(0, waitTimer);
                    }
                }

                //Simulates Head Movement while out of Combat
                if (TurnLookoutOn)
                {
                    DoLookOutMode();
                }
            }
            else //End of Combat Logic Loop
            {
                //Path Back to Home Point
                if (pathHomeTimer > 0)
                {
                    pathHomeTimer -= Time.fixedDeltaTime;
                    navMeshAgent.isStopped = true;
                }
                else
                {
                    navMeshAgent.isStopped = false;

                    navMeshAgent.SetDestination(homePoint);
                    navMeshAgent.stoppingDistance = 0;

                    if (navMeshAgent.remainingDistance <= 0.2f)
                    {
                        WanderMode = true;
                        navMeshAgent.stoppingDistance = 2;
                    }

                }

            }
        }
        else
        {
            navMeshAgent.enabled = false;
            //If Dead
            if (cleanupOnDeath)
            {
                Destroy(childGameObject_FOV);
                GetComponent<SphereCollider>().enabled = false;
                Allies = null;
                Enemies = null;
                HitList = null;       

                cleanupOnDeath = false;

            }
        }
    }

    private IEnumerator HuntRandomDestination()
    {
        if (isTracking == false)
        {
            isTracking = true;
            navMeshAgent.SetDestination(SetRandomPoint());
            yield return new WaitForSeconds(waitTimer);
            isTracking = false;
        }

    }

    private IEnumerator WaitTimeDelay()
    {
        if(isGuarding == false)
        {
            Debug.Log("Test1");
            isGuarding = true;
            yield return new WaitForSeconds(waitTimer);
            Debug.Log("Test2");
            isGuarding = false;
            isPatrolling = false;
        }

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
        if (other.gameObject.GetComponent<CharacterSheet>() != null)
        {
            //TODO:: Clean UP
            ushort otherSpawnID = other.gameObject.GetComponent<CharacterSheet>().GetSpawnID();
            ushort thisSpawnID = GetComponent<CharacterSheet>().GetSpawnID();
            CharacterManager otherEntity;
            CharacterManager thisEntity = gameManager.instance.character_Spawns.GetValueOrDefault(thisSpawnID);

            if (gameManager.instance.character_Spawns.TryGetValue(otherSpawnID, out otherEntity))
            {

                //if Either is dead
                if (!otherEntity.GetEntity().isAlive || !GetComponent<CharacterSheet>().isAlive)
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
                //else
                    //Debug.Log("Unresolved Case Adding SpawnID to NPC Lists");
            }
            //else
                //It has an Entity Script, It should be in gameManager Spawns Dictionary
                //Debug.Log("ID Not in GameManager");
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
        if (other.gameObject.GetComponent<CharacterSheet>() != null)
        {
            ushort otherSpawnID = other.gameObject.GetComponent<CharacterSheet>().GetSpawnID();
            if (GetComponent<CharacterSheet>().isAlive)
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
            if (gameManager.instance.character_Spawns.ContainsKey(HitList[0]))
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
                foreach (ushort enemyID in Enemies)
                {
                    if (gameManager.instance.character_Spawns.ContainsKey(enemyID))
                    {
                        UpdatedList.Add(enemyID);

                        GameObject enemy = gameManager.instance.character_Spawns.GetValueOrDefault(enemyID).GetGameObject();
                        Vector3 enemyDirection = RetrieveObjectDirection(enemy.transform.position);
                        float globalDisplacement = GlobalAngularDisplacement(enemyDirection);
                        float localDisplacement = RelativeAngle(globalDisplacement);

                        //If Enemy Detected, Enter Combat Mode
                        if (enemyDirection.magnitude <= viewDistance && Mathf.Abs(localDisplacement) + headPivot_OffsetCur <= (fieldOfView / 2))
                        {

                            if (gameManager.instance.character_Spawns.ContainsKey(enemyID))
                            {
                                
                                 Target = enemyID;
                                 CombatMode = true;
                                 HitList.Add(enemyID);

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
    /*           Patrol Mode Behavior          */
    /*******************************************/
    #region DoPatrolMode()
    private void DoPatrolMode()
    {
        navMeshAgent.stoppingDistance = 0;
        //Get Destination from Patrol Points List and handle the index tracking
        if (isPatrolling == false && !isGuarding)
        {
            nextPatrolIndex++;
            if (nextPatrolIndex >= patrolNodePrefabs.Count)
            {
                nextPatrolIndex = 0;
            }
            navMeshAgent.SetDestination(patrolNodePrefabs[nextPatrolIndex].transform.position);
            isPatrolling = true;

        }

        //When next position is reached.
        if (navMeshAgent.remainingDistance <= 0.2f)
        {
            //Reach Destination, Start wait timer, Then Lerp Rotate to Patrolpoint object data
            StartCoroutine(WaitTimeDelay());

            LerpRotateToTarget(patrolNodePrefabs[nextPatrolIndex].transform.forward);
            //Lerp Rotation

        }
    }
    #endregion
    /*******************************************/
    /*           Hunter Mode Behavior          */
    /*******************************************/
    #region DoHunterMode()
    private void DoHunterMode()
    {
        if (isHunting)
        {
            //Destination is set
            if (navMeshAgent.remainingDistance <= 4f)
            {
                isHunting = false;
                isTracking = false;
            }
            else
            {
                //TODO:: Lerp Rotation
                //set homepoint here (lots of updates)
            }
        }
        else
        {
            if (Enemies.Count > 0) //Set a Destination enemies in Radar (isHunting to True)
            {

                //Get Random EnemyID from Enemies List;
                int randomIndex = Random.Range(0, Enemies.Count - 1);
                Vector3 Enemypoint = gameManager.instance.GetIDPosition(Enemies[(ushort)randomIndex]);
                navMeshAgent.SetDestination(Enemypoint);
                isHunting = true;
                navMeshAgent.stoppingDistance = 0;
                homePoint = transform.position;
            }
            else //if Radar is empty- patrol duty, or Random Point
            {
                if (PatrolMode)
                {
                    //Do his Day Job
                    DoPatrolMode();
                }
                else
                {
                    //Set Random navmesh destion on a Interval
                    StartCoroutine(HuntRandomDestination());
                }
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
    /*******************************************/
    /*        Rotate transform to Target       */
    /*******************************************/
    #region LerpRotateToTarget()
    public void LerpRotateToTarget()
    {
        if (VerfiyTargetSpawnExists())
        {
            Vector3 targetPos = RetrieveObjectDirection(gameManager.instance.GetIDPosition(Target));
            targetPos.y = 0;
            Quaternion rotate = Quaternion.LookRotation(targetPos);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotate, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    #endregion

    public void LerpRotateToTarget(Vector3 _direction)
    {
        _direction.y = 0;
        //Vector3 PatrolPointDirection = _direction;
        //PatrolPointDirection.y = 0;
        Quaternion rotate = Quaternion.LookRotation(_direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotate, rotationSpeed * Time.fixedDeltaTime);
    }

    /*******************************************/
    /*          Functionality Methods          */
    /*******************************************/
    #endregion

    #region Functionality Methods
    private Vector3 SetRandomPoint()
    {
        //// Random POint;
        //Vector3 randomPoint = (Random.insideUnitSphere * walkRadius) + transform.position;
        //NavMeshHit hit;
        //if(NavMesh.SamplePosition(randomPoint, out hit, walkRadius, 1))
        //{
        //    isTracking = true;
        //}
        //agent.SetDestination(hit.position);
 
        NavMeshTriangulation test = NavMesh.CalculateTriangulation();
        int randomIndex = Random.Range(0, test.indices.Length - 5);
        Vector3 position = test.vertices[test.indices[randomIndex]];
        //Debug.Log(test.vertices.Length);
        //Debug.Log("Random Point Selected");
        return position;


    }

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
        if (gameManager.instance.character_Spawns.ContainsKey(Target))
        {
            isThere = true;
        }
        else
        {
            //If not in world, clean Lists
            if(Allies != null || Enemies != null)
            {
                Allies.Remove(Target);
                Enemies.Remove(Target);
            }

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
        FOV_LR = childGameObject_FOV.AddComponent<LineRenderer>();
        FOV_LR.name = "FOV_Draw";

        Material test = new Material(Shader.Find("Standard"));
        test.color = Color.red;

        FOV_LR.material = new Material(test);
        FOV_LR.positionCount = 12;
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
        for (int i = 0; i <= 10; i++)
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
