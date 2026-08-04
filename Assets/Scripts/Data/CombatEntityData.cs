using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatEntity", menuName = "SpaceCombat/Combat Entity Data")]
public class CombatEntityData : ScriptableObject
{
    [Header("Identity & Faction")]
    public string entityName = "F-72 Interceptor";
    
    [Tooltip("The faction this vehicle belongs to (e.g., 'Human' or 'Alien')")]
    public string faction = "Human";

    [Header("Visuals & Networking")]
    [Tooltip("The actual network prefab for this vehicle (handling physics, weapons, etc.)")]
    public GameObject entityPrefab;

    [Header("Physical Dimensions (Simulation/Radar)")]
    [Tooltip("Length of the aircraft fuselage in meters")]
    public float lengthMeters = 18.5f;
    [Tooltip("Wingspan from wingtip to wingtip in meters")]
    public float wingspanMeters = 13.2f;
    [Tooltip("Height from landing gear to top of the tail fin")]
    public float heightMeters = 4.8f;
    [Tooltip("Empty weight of the airframe in kilograms")]
    public float emptyWeightKg = 9500f;

    [Header("Flight Performance Profiles")]
    [Tooltip("Optimal cruise speed in meters per second (m/s)")]
    public float cruiseSpeedMs = 240f; 
    [Tooltip("Maximum structural/engine airspeed limit before airframe damage (m/s)")]
    public float maxSpeedMs = 380f;    
    
    [Header("Simplified UI Metrics")]
    [Range(1, 10)]
    [Tooltip("Visual 1-10 stat representation for the hangar selection screen")]
    public int maneuverabilityRating = 8;
    [Range(1, 10)]
    [Tooltip("Visual 1-10 stat representation of target signature/stealth")]
    public int radarSignatureRating = 4;
}