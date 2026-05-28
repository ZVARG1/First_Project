using UnityEngine;
using FishNet.Object;

[CreateAssetMenu(fileName = "NewCombatEntity", menuName = "Aircraft Data/Combat Entity Data")]
public class CombatEntityData : ScriptableObject
{
    [Header("Identity")]
    public string entityName = "Standard Fighter";
    public FactionType faction = FactionType.Human;

    [Header("Visuals & Networking")]
    [Tooltip("The actual network prefab for this specific vehicle/craft")]
    public GameObject entityPrefab;

    [Header("Gameplay Stats (Examples)")]
    public float maxSpeed = 150f;
    public float handlingRotSpeed = 45f;
    public int maxShields = 100;
}

// Global enum to track factions cleanly
public enum FactionType { Human, Alien }