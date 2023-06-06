using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        //Left Diagonal
        Vector2Int leftUpper = new Vector2Int(CurrentX, CurrentY);
        Vector2Int leftLower = new Vector2Int(CurrentX, CurrentY);
        while (true)
        {
            if(leftUpper != -Vector2Int.one)
                leftUpper.y++; leftUpper.x--;

            if(leftLower != -Vector2Int.one)
                leftLower.y--; leftLower.x++;

            if (!IsTileInBound(leftUpper, tileCountX, tileCountY) && !IsTileInBound(leftLower, tileCountX, tileCountY))
                break;

            if (IsTileInBound(leftUpper, tileCountX, tileCountY))
            {
                if (board[leftUpper.x, leftUpper.y] == null || board[leftUpper.x, leftUpper.y].Team != Team)
                    result.Add(leftUpper);

                if (board[leftUpper.x, leftUpper.y] != null)
                    leftUpper = -Vector2Int.one;

            }
            else
            {
                leftUpper = -Vector2Int.one;
            }

            if (IsTileInBound(leftLower, tileCountX, tileCountY))
            {
                if (board[leftLower.x, leftLower.y] == null || board[leftLower.x, leftLower.y].Team != Team)
                    result.Add(leftLower);

                if (board[leftLower.x, leftLower.y] != null)
                    leftLower = -Vector2Int.one;

            }
            else
            {
                leftLower = -Vector2Int.one;
            }
        }

        //Left Diagonal
        Vector2Int rightUpper = new Vector2Int(CurrentX, CurrentY);
        Vector2Int rightLower = new Vector2Int(CurrentX, CurrentY);
        while (true)
        {
            if (rightUpper != -Vector2Int.one)
                rightUpper.y++; rightUpper.x++;

            if (rightLower != -Vector2Int.one)
                rightLower.y--; rightLower.x--;

            if (!IsTileInBound(rightUpper, tileCountX, tileCountY) && !IsTileInBound(rightLower, tileCountX, tileCountY))
                break;

            if (IsTileInBound(rightUpper, tileCountX, tileCountY))
            {
                if (board[rightUpper.x, rightUpper.y] == null || board[rightUpper.x, rightUpper.y].Team != Team)
                    result.Add(rightUpper);

                if (board[rightUpper.x, rightUpper.y] != null)
                    rightUpper = -Vector2Int.one;

            }
            else
            {
                rightUpper = -Vector2Int.one;
            }

            if (IsTileInBound(rightLower, tileCountX, tileCountY))
            {
                if (board[rightLower.x, rightLower.y] == null || board[rightLower.x, rightLower.y].Team != Team)
                    result.Add(rightLower);

                if (board[rightLower.x, rightLower.y] != null)
                    rightLower = -Vector2Int.one;

            }
            else
            {
                rightLower = -Vector2Int.one;
            }
        }

        return result;
    }
}
