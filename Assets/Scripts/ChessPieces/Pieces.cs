using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public class Pieces : MonoBehaviour
{
    //0 is White, 1 is Black
    public int Team;
    public int CurrentX;
    public int CurrentY;
    public ChessPieceType Type;
    public bool IsDead = false;

    //The next desired position
    private Vector3 desiredPosition;
    //Scale when dead
    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        transform.rotation = Quaternion.Euler((Team == 0) ? Vector3.zero : new Vector3(0, 180, 0));
    }

    private void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        //transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual void SetPosition(Vector3 position, bool force = true)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY)
    {
        var result = new List<Vector2Int>();

        //result.Add(new Vector2Int(3, 3));
        //result.Add(new Vector2Int(3, 4));
        //result.Add(new Vector2Int(4, 3));
        //result.Add(new Vector2Int(4, 4));

        return result;
    }

    public virtual void SetScale(Vector3 scale, bool force = true)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

    protected bool IsTileInBound(Vector2Int tile, int tileCountX, int tileCountY)
    {
        //Check if tile is in bound
        if (tile.x < 0 || tile.x >= tileCountX || tile.y < 0 || tile.y >= tileCountY)
            return false;

        return true;
    }

    protected bool IsTileValid(Pieces[,] board, Vector2Int tile, int tileCountX, int tileCountY)
    {
        //Check if tile is in bound
        if (tile.x < 0 || tile.x >= tileCountX || tile.y < 0 || tile.y >= tileCountY)
            return false;

        //Check if there is no teammate
        if (board[tile.x, tile.y] != null && board[tile.x, tile.y].Team == Team)
            return false;

        return true;
    }

    public virtual SpecialMove GetSpecialMoves(ref Pieces[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }

    public Pieces Clone()
    {
        switch (Type)
        {
            case ChessPieceType.King:
                return this.MemberwiseClone() as King;
            case ChessPieceType.Bishop:
                return this.MemberwiseClone() as Bishop;
            case ChessPieceType.Pawn:
                return this.MemberwiseClone() as Pawn;
            case ChessPieceType.Knight:
                return this.MemberwiseClone() as Knight;
            case ChessPieceType.Queen:
                return this.MemberwiseClone() as Queen;
            case ChessPieceType.Rook:
                return this.MemberwiseClone() as Rook;
            default:
                return this.MemberwiseClone() as Pieces;
        }
    }
}
