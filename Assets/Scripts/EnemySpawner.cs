using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Game;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //The last EnemySpawner to start running, TODO: Replace with a more comprehensive system if multiple enemy spawners are used.
    public static EnemySpawner LatestLaunched;

    public List<SpawnRequest> spawnRequests = new();

    [Min(0)] [Tooltip("The delay before declaring all waves completed.")]
    public float PostWaveDelay;

    private float enemiesSpawnedThisWave;
    private float totalWavesCompleted;
    private float totalWavesToSpawn;
    public Action WaveEnded;
    public Action WavesCompleted;

    private float spawnDelayTimer = 0;
    

    public Action WaveStarted;

    public void Start()
    {
        StartCoroutine(EnemySpawnerLoop());
    }

    [Tooltip("On a scale from 0-1 returns how close the current wave is to completion.")]
    public float WavePercentage()
    {
        if (spawnRequests.Count <= 0)
            //If there are no spawn requests we assume 100% completion of  this wave. (Last wave ends and we have no requests left)
            return 1;
        var current = spawnRequests[0];

        //What percent of enemies spawned this wave have we killed?
        return 1 - Mathf.Min(Mathf.Max(0, transform.childCount / Mathf.Max(1, enemiesSpawnedThisWave)),
            1);
    }

    [Tooltip("On a scale from 0-1 returns how close this entire enemy spawner is to 100% completion.")]
    public float TotalPercentage()
    {
        return totalWavesCompleted / totalWavesToSpawn;
    }

    public SpawnRequest.DifficultyRating CurrentWaveDifficulty()
    {
        if (spawnRequests.Count <= 0)
            //No waves? This is easy mode!
            return SpawnRequest.DifficultyRating.Easy;

        var current = spawnRequests[0];
        return current.ReportedDifficultyRating;
    }

    private bool hasNoChildren()
    {
        return transform.childCount <= 0;
    }

    public IEnumerator EnemySpawnerLoop()
    {
        while (spawnRequests.Count > 0)
        {
            LatestLaunched = this;
            if (spawnRequests.Count > totalWavesToSpawn) totalWavesToSpawn = spawnRequests.Count;
            WaveStarted?.Invoke();
            var current = spawnRequests[0];
            enemiesSpawnedThisWave = 0;
            for (var i = 0; i < current.SpawnAmount; i++)
            {
                enemiesSpawnedThisWave++;
                var spawnPos = transform.position;
                PathNode initTarget = null;
                if(current.EnemyConfig==null)
                {
                    Debug.LogError("Enemy Configuration is null, enemy configs are either unset or may have been lost in data migration?");
                }

                if (current.snapSpawning) initTarget = PathManager.Instance.getEntryNode();
                if (initTarget != null) spawnPos = initTarget.transform.position;
                var g = Instantiate(current.EnemyConfig.prefab, spawnPos, transform.rotation, transform);
                g.GetComponent<Enemy>().Initialize(current.EnemyConfig);

                if (initTarget != null)
                {
                    var p = g.GetComponent<Pathfinder>();
                    p.currentNode = initTarget;
                }

                // yield return new WaitForSeconds(current.SpawnDelayTime);
                yield return new WaitUntil(() => GameSpeed.GameTimerCheck(ref spawnDelayTimer, current.SpawnDelayTime));
            }

            if (current.WaitUntilCompletion) yield return new WaitUntil(hasNoChildren);
            totalWavesCompleted++;
            WaveEnded?.Invoke();

            yield return new WaitForSeconds(current.PostWaveDelay);

            yield return new WaitForEndOfFrame();
            spawnRequests.RemoveAt(0);
        }

        while (transform.childCount > 0) yield return new WaitForEndOfFrame();
        if (PostWaveDelay >= 0) yield return new WaitForSeconds(PostWaveDelay);
        if (LatestLaunched == this)
            LatestLaunched = null;
        WavesCompleted?.Invoke();
    }

    [Serializable]
    public class SpawnRequest
    {
        public enum DifficultyRating
        {
            Normal,
            Easy,
            Boss
        }

        public EnemyScriptableObject EnemyConfig;

        [Tooltip("How many Enemies to spawn with this order.")]
        public float SpawnAmount = 1;

        [Tooltip("Delay between each spawning.")]
        public float SpawnDelayTime;

        [Tooltip("How long to wait before next wave.")]
        public float PostWaveDelay;

        [Tooltip("True if the next wave must wait before this one is finished to start.")]
        public bool WaitUntilCompletion;


        [Tooltip("Should this enemy snap to its entry node when it spawns?")]
        public bool snapSpawning = true;

        [Tooltip("Optional rating of difficulty for any current wave UI.")]
        public DifficultyRating ReportedDifficultyRating;
    }
}