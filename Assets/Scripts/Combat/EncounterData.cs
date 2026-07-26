using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Encounter", fileName = "Encounter")]
public class EncounterData : ScriptableObject
{
    [Tooltip("Score")]
    public int bounty = 100;
    public int spoilsSelections = 2;

    [Tooltip("Seconds between a wave dying and the next one spawning.")]
    public float delayBetweenWaves = 0.8f;

    [Tooltip("Waves spawn in order.")]
    public List<Wave> waves = new List<Wave>();

    [Serializable]
    public class Wave
    {
        public List<Enemy> enemyPrefabs = new List<Enemy>();
    }
}
