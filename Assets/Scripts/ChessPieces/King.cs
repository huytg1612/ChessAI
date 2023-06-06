using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        //Above row
        Vector2Int aboveRowLeft = new Vector2Int(CurrentX + 1, CurrentY - 1);
        Vector2Int aboveRowMiddle = new Vector2Int(CurrentX + 1, CurrentY);
        Vector2Int aboveRowRight = new Vector2Int(CurrentX + 1, CurrentY + 1);

        //Same row
        Vector2Int sameRowLeft = new Vector2Int(CurrentX, CurrentY - 1);
        Vector2Int sameRowRight = new Vector2Int(CurrentX, CurrentY + 1);

        //Below row
        Vector2Int belowRowLeft = new Vector2Int(CurrentX - 1, CurrentY - 1);
        Vector2Int belowRowMiddle = new Vector2Int(CurrentX - 1, CurrentY);
        Vector2Int belowRowRight = new Vector2Int(CurrentX - 1, CurrentY + 1);

        if (IsTileValid(board, aboveRowLeft, tileCountX, tileCountY))
            result.Add(aboveRowLeft);
        if (IsTileValid(board, aboveRowMiddle, tileCountX, tileCountY))
            result.Add(aboveRowMiddle);
        if (IsTileValid(board, aboveRowRight, tileCountX, tileCountY))
            result.Add(aboveRowRight);
        if (IsTileValid(board, sameRowLeft, tileCountX, tileCountY))
            result.Add(sameRowLeft);
        if (IsTileValid(board, sameRowRight, tileCountX, tileCountY))
            result.Add(sameRowRight);
        if (IsTileValid(board, belowRowLeft, tileCountX, tileCountY))
            result.Add(belowRowLeft);
        if (IsTileValid(board, belowRowMiddle, tileCountX, tileCountY))
            result.Add(belowRowMiddle);
        if (IsTileValid(board, belowRowRight, tileCountX, tileCountY))
            result.Add(belowRowRight);

        return result;
    }

    public override SpecialMove GetSpecialMoves(ref Pieces[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove r = SpecialMove.None;

        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == (Team == 0 ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == (Team == 0 ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == (Team == 0 ? 0 : 7));

        if(kingMove == null && CurrentX == 4)
        {
            // White Team
            if(Team == 0)
            {
                //Left Rook
                if(leftRook == null)
                {
                    if(board[0, 0].Type == ChessPieceType.Rook)
                    {
                        if(board[0, 0].Team == 0)
                        {
                            if(board[3, 0] == null)
                            {
                                if(board[2,0] == null)
                                {
                                    if(board[1, 0] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                //Right Rook
                if (rightRook == null)
                {
                    if (board[7, 0].Type == ChessPieceType.Rook)
                    {
                        if (board[7, 0].Team == 0)
                        {
                            if (board[5, 0] == null)
                            {
                                if (board[6, 0] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Left Rook
                if (leftRook == null)
                {
                    if (board[0, 7].Type == ChessPieceType.Rook)
                    {
                        if (board[0, 7].Team == 1)
                        {
                            if (board[3, 7] == null)
                            {
                                if (board[2, 7] == null)
                                {
                                    if (board[1, 7] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                //Right Rook
                if (rightRook == null)
                {
                    if (board[7, 7].Type == ChessPieceType.Rook)
                    {
                        if (board[7, 7].Team == 1)
                        {
                            if (board[5, 7] == null)
                            {
                                if (board[6, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
        }

        return r;
    }
}
