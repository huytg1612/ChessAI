using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        Vector2Int topLeft1 = new Vector2Int(CurrentX - 2, CurrentY + 1);
        Vector2Int topLeft2 = new Vector2Int(CurrentX - 1, CurrentY + 2);

        Vector2Int topRight1 = new Vector2Int(CurrentX + 2, CurrentY + 1);
        Vector2Int topRight2 = new Vector2Int(CurrentX + 1, CurrentY + 2);

        Vector2Int bottomLeft1 = new Vector2Int(CurrentX - 2, CurrentY - 1);
        Vector2Int bottomLeft2 = new Vector2Int(CurrentX - 1, CurrentY - 2);

        Vector2Int bottomRight1 = new Vector2Int(CurrentX + 2, CurrentY - 1);
        Vector2Int bottomRight2 = new Vector2Int(CurrentX + 1, CurrentY - 2);

        if (IsTileValid(board, topLeft1, tileCountX, tileCountY))
            result.Add(topLeft1);
        if (IsTileValid(board, topLeft2, tileCountX, tileCountY))
            result.Add(topLeft2);
        if (IsTileValid(board, topRight1, tileCountX, tileCountY))
            result.Add(topRight1);
        if (IsTileValid(board, topRight2, tileCountX, tileCountY))
            result.Add(topRight2);
        if (IsTileValid(board, bottomLeft1, tileCountX, tileCountY))
            result.Add(bottomLeft1);
        if (IsTileValid(board, bottomLeft2, tileCountX, tileCountY))
            result.Add(bottomLeft2);
        if (IsTileValid(board, bottomRight1, tileCountX, tileCountY))
            result.Add(bottomRight1);
        if (IsTileValid(board, bottomRight2, tileCountX, tileCountY))
            result.Add(bottomRight2);

        return result;
    }
}
