using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        //Vertical move
        int upper = CurrentY;
        int lower = CurrentY;
        while (true)
        {
            upper++;
            lower--;

            if (upper >= tileCountY && lower < 0)
                break;

            if (upper < tileCountY)
            {
                if(board[CurrentX, upper] == null)
                {
                    result.Add(new Vector2Int(CurrentX, upper));
                }
                else
                {
                    if(board[CurrentX, upper].Team != Team)
                        result.Add(new Vector2Int(CurrentX, upper));

                    upper = tileCountY;
                }
            }

            if (lower >= 0)
            {
                if (board[CurrentX, lower] == null)
                {
                    result.Add(new Vector2Int(CurrentX, lower));
                }
                else
                {
                    if (board[CurrentX, lower].Team != Team)
                        result.Add(new Vector2Int(CurrentX, lower));

                    lower = -1;
                }
            }
        }

        //Horizontal move
        int right = CurrentX;
        int left = CurrentX;

        while (true)
        {
            right++;
            left--;

            if (right >= tileCountX && left < 0)
                break;

            if (right < tileCountX)
            {
                if (board[right, CurrentY] == null)
                {
                    result.Add(new Vector2Int(right, CurrentY));
                }
                else
                {
                    if (board[right, CurrentY].Team != Team)
                        result.Add(new Vector2Int(right, CurrentY));

                    right = tileCountX;
                }
            }

            if (left >= 0)
            {
                if (board[left, CurrentY] == null)
                {
                    result.Add(new Vector2Int(left, CurrentY));
                }
                else
                {
                    if (board[left, CurrentY].Team != Team)
                        result.Add(new Vector2Int(left, CurrentY));

                    left = -1;
                }
            }
        }

        return result;
    }
}
