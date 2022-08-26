using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_spawnParameters : MonoBehaviour
{
    [Header("----- Adjustable Parameters -----")]
    [SerializeField] private GameObject spawnPrefab;
    
    [SerializeField] private float spawnDelay;
    [SerializeField] private int maxSpawns;
    [SerializeField] int refreshListTime = 10;

    [Header("----- Keep True -----")]
    [SerializeField] bool refreshLockout = true;
    [SerializeField] private bool isSpawning = true;

    [Header("----- Tracking -----")]
    [SerializeField] private List<ushort> spawnList = new List<ushort>();
    [SerializeField] private int spawnCount;
    [SerializeField] private ushort placeHolderID;

    void Update()
    {
        if (isSpawning && spawnCount < maxSpawns)
        {
            StartCoroutine(SpawnAICharacter());
        }

        if (refreshLockout)
        {
            StartCoroutine(RefreshList());
        }
    }

    private IEnumerator SpawnAICharacter()
    {
        isSpawning = false;
        GameObject newAISpawn = Instantiate(spawnPrefab, transform.position, transform.rotation);
        spawnCount++;
        yield return new WaitForSeconds(spawnDelay);
        placeHolderID = newAISpawn.GetComponent<CharacterSheet>().GetSpawnID();

        spawnList.Add(placeHolderID);
        isSpawning = true;

    }

    private IEnumerator RefreshList()
    {
        
        refreshLockout = false;
        yield return new WaitForSeconds(refreshListTime);
        refreshLockout = true;

        for(int i = spawnList.Count - 1; i >= 0; i--)
        {
            //Debug.Log(gameManager.instance.ContainsSpawn(spawnList[i]));
            if (!gameManager.instance.ContainsSpawn(spawnList[i]))
            {
                spawnList.RemoveAt(i);
                spawnCount--;
            }
            
        }
    
    }
}
