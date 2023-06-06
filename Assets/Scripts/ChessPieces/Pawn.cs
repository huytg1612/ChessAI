using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        //If Pawn stand at last piece of board (vertically)
        if (CurrentY == 0 || CurrentY == tileCountY - 1)
            return result;

        //If Pawn is white team, go up. Black team, go down
        int direction = Team == 0 ? 1 : -1;
        //One front
        if (board[CurrentX, CurrentY + direction] == null)
        {
            result.Add(new Vector2Int(CurrentX, CurrentY + direction));

            //Two front
            if ((CurrentY == 1 && Team == 0 || CurrentY == 6 && Team == 1) && board[CurrentX, CurrentY + (direction * 2)] == null)
            {
                result.Add(new Vector2Int(CurrentX, CurrentY + (direction * 2)));
            }
        }

        //Kill opponent
        //Left side check
        if (CurrentX > 0 && board[CurrentX - 1, CurrentY + direction] != null && board[CurrentX - 1, CurrentY + direction].Team != Team)
            result.Add(new Vector2Int(CurrentX - 1, CurrentY + direction));
        //Right side check
        if (CurrentX < tileCountX - 1 && board[CurrentX + 1, CurrentY + direction] != null && board[CurrentX + 1, CurrentY + direction].Team != Team)
            result.Add(new Vector2Int(CurrentX + 1, CurrentY + direction));

        return result;
    }

    public override SpecialMove GetSpecialMoves(ref Pieces[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = Team == 0 ? 1 : -1;
        if((Team == 0 && CurrentY == 6) || (Team == 1 && CurrentY == 1))
        {
            return SpecialMove.Promotion;
        }
        // En Passant
        if(moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            //If the last piece moved was a pawn
            if(board[lastMove[1].x, lastMove[1].y].Type == ChessPieceType.Pawn)
            {
                //If the last move was a +2 in either direction
                if(Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    if(board[lastMove[1].x, lastMove[1].y].Team != Team)
                    {
                        //If both pawn are on the same Y
                        if(lastMove[1].y == CurrentY)
                        {
                            if(lastMove[1].x == CurrentX - 1)//Landed left
                            {
                                availableMoves.Add(new Vector2Int(CurrentX - 1, CurrentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == CurrentX + 1)//Landed right
                            {
                                availableMoves.Add(new Vector2Int(CurrentX + 1, CurrentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
