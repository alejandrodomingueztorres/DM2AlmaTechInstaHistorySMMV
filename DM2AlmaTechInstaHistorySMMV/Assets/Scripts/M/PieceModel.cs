using UnityEngine;

public enum PieceGroup
{
    Base,
    Body,
    Neck,
    Lip
}

public enum SpecialPairType
{
    None,
    Symbol,
    Dots,
    Base,
    Body,
    Neck,
    Lip
}

[System.Serializable]
public class PieceModel
{
    public string id;
    public PieceGroup group;
    public SpecialPairType pairType;

    public Transform targetTransform;

    public bool isPlaced = false;
}