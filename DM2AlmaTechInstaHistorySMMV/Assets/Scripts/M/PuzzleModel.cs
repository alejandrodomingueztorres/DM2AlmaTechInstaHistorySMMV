using System.Collections.Generic;
using UnityEngine;

public class PuzzleModel : MonoBehaviour
{
    public List<PieceModel> pieces;

    public bool AreAllPlaced()
    {
        foreach (var p in pieces)
        {
            if (!p.isPlaced)
                return false;
        }
        return true;
    }

    public bool CheckSpecialPair(SpecialPairType type)
    {
        if (type == SpecialPairType.None) return false;

        int count = 0;

        foreach (var p in pieces)
        {
            if (p.pairType == type && p.isPlaced)
                count++;
        }

        return count >= 2;
    }
}
