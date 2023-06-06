using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();
        #region Bishop Move
        //Left Diagonal
        Vector2Int leftUpper = new Vector2Int(CurrentX, CurrentY);
        Vector2Int leftLower = new Vector2Int(CurrentX, CurrentY);
        while (true)
        {
            if (leftUpper != -Vector2Int.one)
                leftUpper.y++; leftUpper.x--;

            if (leftLower != -Vector2Int.one)
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
        #endregion

        #region Rook Move
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
                if (board[CurrentX, upper] == null)
                {
                    result.Add(new Vector2Int(CurrentX, upper));
                }
                else
                {
                    if (board[CurrentX, upper].Team != Team)
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
        #endregion

        return result;
    }
}
