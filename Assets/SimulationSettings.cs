// W nowym pliku SimulationSettings.cs
using UnityEngine;

[System.Serializable] // To sprawia, ¿e klasa jest widoczna w Inspectorze Unity
public class SimulationSettings
{
    [Header("??? GENERAL SIMULATION ???")]
    [Tooltip("Global simulation speed. 1=normal, 2=2x fast, 0=pause.")]
    [Range(0f, 10f)]
    public float SimulationTimeScale = 1.0f;

    [Tooltip("Initial number of prey to spawn.")]
    public int InitialPreyCount = 50;

    [Tooltip("Initial number of predators to spawn.")]
    public int InitialPredatorCount = 5;

    [Tooltip("The radius of the world. A smaller world means more frequent encounters.")]
    [Range(50f, 1000f)]
    public float WorldRadius = 500f;

    [Header("??? PREY BEHAVIOUR ???")]
    [Tooltip("How far prey can see predators.")]
    [Range(5f, 100f)]
    public float PreyDetectionRadius = 30.0f;

    [Tooltip("How strongly prey stick together in a herd.")]
    [Range(0f, 10f)]
    public float PreyCohesionStrength = 3.0f;

    [Tooltip("Maximum running speed of prey when fleeing.")]
    [Range(1f, 10f)]
    public float PreyMaxSpeed = 5.0f;

    [Tooltip("How quickly prey get tired when fleeing.")]
    [Range(0.01f, 0.2f)]
    public float PreyFatigueRate = 0.05f;

    [Header("??? PREDATOR BEHAVIOUR ???")]
    [Tooltip("How far predators can detect prey.")]
    [Range(10f, 200f)]
    public float PredatorDetectionRadius = 70.0f;

    [Tooltip("How strongly predators coordinate as a pack.")]
    [Range(0f, 10f)]
    public float PredatorCohesionStrength = 1.0f;

    [Tooltip("Maximum running speed of a predator when chasing.")]
    [Range(1f, 15f)]
    public float PredatorMaxSpeed = 7.0f;

    [Tooltip("The amount of damage a predator deals with each hit.")]
    [Range(0.1f, 1.0f)]
    public float PredatorDamagePerHit = 0.25f;
}