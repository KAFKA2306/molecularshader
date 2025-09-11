using UnityEngine;

// Centralized spatial configuration for molecule rendering.
// Defines how to interpret XYZ coordinates (in Angstroms) and how to
// center/scale them into Unity world units.
//
// Usage:
// - Assign an instance to MoleculeRaymarchDriver.spatial.
// - If null, the driver falls back to its legacy fields.
public class MoleculeSpatialConfig : ScriptableObject
{
    [Header("Normalization")]
    [Tooltip("If true, subtract the raw-bounds center so molecule is centered at world origin.")]
    public bool centerAtOrigin = true;

    [Tooltip("If true, scales uniformly so the longest side of the raw bounds equals targetBoundsSize (in Unity units). If false, uses angstromToUnity for a fixed conversion.")]
    public bool autoFit = true;

    [Tooltip("When autoFit is true, longest side of molecule's bounds after scaling will match this size (Unity units, meters).")]
    public float targetBoundsSize = 1.0f;

    [Header("Unit Conversion")]
    [Tooltip("Fixed conversion from Angstrom to Unity meters when autoFit is false. Typical: 0.01 -> 1Å maps to 1cm.")]
    public float angstromToUnity = 0.01f;

    [Header("Visual Scale Multipliers")]
    [Tooltip("Additional multiplier for atom radii (dimensionless). Applied after unit scaling.")]
    public float atomScale = 1.0f;

    [Tooltip("Additional multiplier for bond radius (dimensionless). Applied after unit scaling.")]
    public float bondScale = 1.0f;

    [Header("Bond Parameters (Angstrom)")]
    [Tooltip("Default bond cylinder radius in Angstrom when generating bonds.")]
    public float bondRadiusAngstrom = 0.15f;
}

