using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Các nước đi đặc biệt
public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}

//Điểm cho từng loại chess
public enum ChessPiecePoint
{
    Pawn = 10,
    Knight = 30,
    Bishop = 30,
    Rook = 50,
    Queen = 90,
    King = 900
}

public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    //Define material (texture) for tile
    [SerializeField] private Material TileMaterial;
    [SerializeField] private float TileSize = 1.0f;
    [SerializeField] private float YOffset = 0.2f;
    [SerializeField] private Vector3 BoardCenter = Vector3.zero;
    [SerializeField] private float DragOffset = 1f;
    [SerializeField] private GameObject VictoryScreen;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] Prefabs;
    [SerializeField] private Material[] TeamMaterials;

    //Chess Board Size
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;

    private const int TEAM_WHITE = 0;
    private const int TEAM_BLACK = 1;
    private const float DeathScale = 0.5f;
    private const float DeathSpacing = 0.5f;

    private Pieces[,] Pieces;
    private GameObject[,] Tiles;
    private Camera CurrentCamera;
    private Vector2Int CurrentHover;
    private Vector3 Bound;
    private Pieces CurrentlyDragging;
    private List<Pieces> DeadWhite = new List<Pieces>();
    private List<Pieces> DeadBlack = new List<Pieces>();
    private List<Vector2Int> AvailableMoves = new List<Vector2Int>();
    private int CurrentTurn;
    private SpecialMove SpecialMove;
    private List<Vector2Int[]> MoveList = new List<Vector2Int[]>();

    private Pieces PieckedPiece;
    private Vector2Int BestMove = -Vector2Int.one;
    private int MaxDepth = 4;
    private int BestValue = int.MinValue;

    void Start()
    {
        
    }

    //Tính điểm trên bàn cờ cho AI
    private int Evaluate(Pieces[,] pieces, int teamColor)
    {
        int whitePoint = 0;
        int blackPoint = 0;

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                var piece = pieces[x, y];
                int point;
                if(piece != null && !piece.IsDead)
                {
                    switch (piece.Type)
                    {
                        case ChessPieceType.King:
                            point = (int)ChessPiecePoint.King;
                            break;
                        case ChessPieceType.Bishop:
                            point = (int)ChessPiecePoint.Bishop;
                            break;
                        case ChessPieceType.Pawn:
                            point = (int)ChessPiecePoint.Pawn;
                            break;
                        case ChessPieceType.Knight:
                            point = (int)ChessPiecePoint.Knight;
                            break;
                        case ChessPieceType.Queen:
                            point = (int)ChessPiecePoint.Queen;
                            break;
                        case ChessPieceType.Rook:
                            point = (int)ChessPiecePoint.Rook;
                            break;
                        default:
                            point = 0;
                            break;
                    }

                    if (piece.Team == TEAM_WHITE)
                        whitePoint += point;
                    else
                        blackPoint += point;
                }
            }
        }

        if (teamColor == TEAM_WHITE)
            return whitePoint - blackPoint;
        else
            return blackPoint - whitePoint;
    }

    //Lẩy các quân cờ theo team
    private List<Pieces> GetPiecesByTeam(ref Pieces[,] pieces, int teamColor)
    {
        var result = new List<Pieces>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                var piece = pieces[x, y];
                if (piece != null && piece.Team == teamColor && !piece.IsDead)
                    result.Add(piece);
            }
        }

        return result;
    }

    //Random các lựa chọn quân cờ cho AI
    public void Shuffle(List<Pieces> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Pieces value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //Lượt chơi của AI
    private void AIMove()
    {
        Minimax(Pieces.Clone() as Pieces[,], MaxDepth, true, int.MinValue, int.MaxValue);   

        if(PieckedPiece != null)
        {
            Debug.Log("Picked piece: " + PieckedPiece);
            Debug.Log("From: " + new Vector2Int(PieckedPiece.CurrentX, PieckedPiece.CurrentY));
            Debug.Log("Move: " + BestMove);
            Debug.Log("Value: " + BestValue);
            Debug.Log("--------------------------------------------");
            MoveTo(PieckedPiece, BestMove.x, BestMove.y);
            RemoveHightlight();
        }
    }

    //Kiểm tra game đã kết thúc chưa (chỉ dùng cho AI)
    private bool IsGameOver(Pieces[,] pieces)
    {
        Pieces whiteKing = null;
        Pieces blackKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                var piece = pieces[x, y];
                if(piece != null && !piece.IsDead && piece.Type == ChessPieceType.King)
                {
                    if (piece.Team == TEAM_BLACK)
                        blackKing = piece;
                    else
                        whiteKing = piece;
                }
            }
        }

        return whiteKing == null || blackKing == null;
    }

    //Tính nước đi tốt nhất cho AI
    private int Minimax(Pieces[,] pieces, int depth, bool isMaximizingPlayer, int alpha, int beta)
    {
        if (depth == 0 || IsGameOver(pieces))
            return Evaluate(pieces, TEAM_BLACK);

        if (isMaximizingPlayer)
        {
            int maxVal = int.MinValue;
            List<Pieces> blackPieces = GetPiecesByTeam(ref pieces, TEAM_BLACK);
            Shuffle(blackPieces);
            foreach (var piece in blackPieces)
            {
                var availableMoves = piece.GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
                foreach(var move in availableMoves)
                {
                    #region Move
                    var currenPosition = new Vector2Int(piece.CurrentX, piece.CurrentY);
                    Pieces targetPiece = null;
                    //If the move is enemy
                    if (pieces[move.x, move.y] != null)
                        targetPiece = pieces[move.x, move.y].Clone();
                    pieces[move.x, move.y] = piece;
                    piece.CurrentX = move.x;
                    piece.CurrentY = move.y;
                    pieces[currenPosition.x, currenPosition.y] = null;
                    #endregion
                    var val = Minimax(pieces.Clone() as Pieces[,], depth - 1, false, alpha, beta);
                    #region Undo
                    pieces[currenPosition.x, currenPosition.y] = piece;
                    piece.CurrentX = currenPosition.x;
                    piece.CurrentY = currenPosition.y;
                    pieces[move.x, move.y] = targetPiece;
                    #endregion
                    if (val > maxVal)
                    {
                        maxVal = val;
                        if(depth == MaxDepth)
                        {
                            PieckedPiece = piece;
                            BestMove = move;
                            AvailableMoves = availableMoves;
                            BestValue = maxVal;
                        }
                    }
                    if (maxVal > alpha)
                        alpha = maxVal;

                    if (beta <= alpha)
                        break;
                }

                if (beta <= alpha)
                    break;
            }

            return maxVal;
        }
        else
        {
            int minVal = int.MaxValue;
            List<Pieces> whitePieces = GetPiecesByTeam(ref pieces, TEAM_WHITE);
            Shuffle(whitePieces);
            foreach (var piece in whitePieces)
            {
                var availableMoves = piece.GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
                foreach (var move in availableMoves)
                {
                    #region Move
                    var currenPosition = new Vector2Int(piece.CurrentX, piece.CurrentY);
                    Pieces targetPiece = null;
                    if (pieces[move.x, move.y] != null)
                        targetPiece = pieces[move.x, move.y].Clone();
                    pieces[move.x, move.y] = piece;
                    piece.CurrentX = move.x;
                    piece.CurrentY = move.y;
                    pieces[currenPosition.x, currenPosition.y] = null;
                    #endregion
                    var val = Minimax(pieces.Clone() as Pieces[,], depth - 1, true, alpha, beta);
                    #region Undo
                    pieces[currenPosition.x, currenPosition.y] = piece;
                    piece.CurrentX = currenPosition.x;
                    piece.CurrentY = currenPosition.y;
                    pieces[move.x, move.y] = targetPiece;
                    #endregion
                    if (val < minVal)
                    {
                        minVal = val;
                    }

                    if (minVal < beta)
                        beta = minVal;

                    if (beta <= alpha)
                        break;
                }

                if (beta <= alpha)
                    break;
            }

            return minVal;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!CurrentCamera)
        {
            CurrentCamera = Camera.main;
            return;
        }

        if (!VictoryScreen.activeSelf)
        {
            if (CurrentTurn == TEAM_BLACK)
            {
                AIMove();
            }
            else
            {
                RaycastHit hitInfo; //Get the gameobject's info when it is hit
                Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition); //Whenever mouse moved, get the Ray

                //IF the Ray hit the game object with Layer Mask named "Tile, Hover, Highlight", get the gameobject info 
                if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
                {
                    Vector2Int hitPosition = LookupTileIndex(hitInfo.transform.gameObject);

                    //If the previous hover is nothing, we set the currentHover to hitPosition
                    if (CurrentHover == -Vector2Int.one)
                    {
                        CurrentHover = hitPosition;
                        Tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                    }

                    //If the hitPosition is different from the previous hover, set the previous hover to Tile and the hitPosition to current 
                    if (CurrentHover != hitPosition)
                    {
                        Tiles[CurrentHover.x, CurrentHover.y].layer = IsTileAvailableMove(ref AvailableMoves, CurrentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                        CurrentHover = hitPosition;
                        Tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                    }

                    //If pick the piece up
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Pieces[hitPosition.x, hitPosition.y] != null)
                        {
                            if (CurrentTurn == Pieces[hitPosition.x, hitPosition.y].Team && CurrentTurn == TEAM_WHITE)
                            {
                                CurrentlyDragging = Pieces[hitPosition.x, hitPosition.y];
                                AvailableMoves = CurrentlyDragging.GetAvailableMoves(ref Pieces, TILE_COUNT_X, TILE_COUNT_Y);
                                //Get a list of special moves
                                SpecialMove = CurrentlyDragging.GetSpecialMoves(ref Pieces, ref MoveList, ref AvailableMoves);
                                PreventCheck();
                                HightlightAvailableMoves();
                            }
                        }
                    }

                    //If put the piece down
                    if (Input.GetMouseButtonUp(0) && CurrentlyDragging != null)
                    {
                        Vector2Int previousPosition = new Vector2Int(CurrentlyDragging.CurrentX, CurrentlyDragging.CurrentY);

                        bool validMove = MoveTo(CurrentlyDragging, hitPosition.x, hitPosition.y);
                        if (!validMove)
                        {
                            CurrentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                        }

                        CurrentlyDragging = null;
                        RemoveHightlight();
                    }

                }
                //If the mouse move out side of the ChessBoard, reset the previous Tile's Layer Mask
                else
                {
                    if (CurrentHover != -Vector2Int.one)
                    {
                        Tiles[CurrentHover.x, CurrentHover.y].layer = IsTileAvailableMove(ref AvailableMoves, CurrentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                        CurrentHover = -Vector2Int.one;
                    }

                    //Prevent the case, move chess out of board.
                    if (CurrentlyDragging && Input.GetMouseButtonUp(0))
                    {
                        CurrentlyDragging.SetPosition(GetTileCenter(CurrentlyDragging.CurrentX, CurrentlyDragging.CurrentY));
                        CurrentlyDragging = null;
                        RemoveHightlight();
                    }
                }

                if (CurrentlyDragging)
                {
                    //Create a plane which is above the chess board
                    Plane plane = new Plane(Vector3.up, Vector3.up * YOffset);
                    float distance = 0.0f;

                    //Stick the Piece to the Plane
                    if (plane.Raycast(ray, out distance))
                    {
                        CurrentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * DragOffset);
                    }
                }
            }
        }
    }

    //Di chuyển quân cờ tới vị trí chỉ định
    private bool MoveTo(Pieces piece, int x, int y)
    {
        if(!IsTileAvailableMove(ref AvailableMoves, new Vector2Int(x, y)))
        {
            return false;
        }

        var otherPiece = Pieces[x, y];
        if(otherPiece != null)
        {
            if(otherPiece.Team == piece.Team)
                return false;

            //Kill the opponent piece
            if(otherPiece.Team != piece.Team)
            {
                otherPiece.IsDead = true;
                otherPiece.SetScale(Vector3.one * DeathScale);
                if (otherPiece.Team == 0)
                {
                    //If White King is killed, Black Team win
                    if (otherPiece.Type == ChessPieceType.King)
                        CheckMate(1);

                    DeadWhite.Add(otherPiece);
                    otherPiece.SetPosition(
                            new Vector3(8 * TileSize, YOffset, -1 * TileSize)
                            - Bound
                            + new Vector3(TileSize / 2, 0, TileSize / 2)
                            + (Vector3.forward * DeathSpacing) * DeadWhite.Count
                        );
                }
                else
                {
                    //If Black King is killed, White Team win
                    if (otherPiece.Type == ChessPieceType.King)
                        CheckMate(0);

                    DeadBlack.Add(otherPiece);
                    otherPiece.SetPosition(
                            new Vector3(-1 * TileSize, YOffset, 8 * TileSize)
                            - Bound
                            + new Vector3(TileSize / 2, 0, TileSize / 2)
                            + (Vector3.back * DeathSpacing) * DeadBlack.Count
                        );
                }
            }
        }

        Vector2Int previousPosition = new Vector2Int(piece.CurrentX, piece.CurrentY);
        Pieces[x, y] = piece;
        Pieces[previousPosition.x, previousPosition.y] = null;

        //Move the piece to the right place
        PositionSinglePiece(x, y, true);

        if (CurrentTurn == TEAM_WHITE)
            CurrentTurn = TEAM_BLACK;
        else
            CurrentTurn = TEAM_WHITE;

        MoveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        if (CheckForCheckmate())
        {
            CheckMate(piece.Team);
        }
        return true;
    }

    //Hậu xử lý các nước đi đặc biệt
    private void ProcessSpecialMove()
    {
        if(SpecialMove == SpecialMove.EnPassant)
        {
            var newMove = MoveList[MoveList.Count - 1];
            Pieces myPawn = Pieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = MoveList[MoveList.Count - 2];
            Pieces enemyPawn = Pieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.CurrentX == enemyPawn.CurrentX)
            {
                if(myPawn.CurrentY == enemyPawn.CurrentY - 1 || myPawn.CurrentY == enemyPawn.CurrentY + 1)
                {
                    enemyPawn.SetScale(Vector3.one * DeathScale);
                    if (enemyPawn.Team == 0)
                    {
                        DeadWhite.Add(enemyPawn);
                        enemyPawn.SetPosition(
                                new Vector3(8 * TileSize, YOffset, -1 * TileSize)
                                - Bound
                                + new Vector3(TileSize / 2, 0, TileSize / 2)
                                + (Vector3.forward * DeathSpacing) * DeadWhite.Count
                            );
                    }
                    else
                    {
                        DeadBlack.Add(enemyPawn);
                        enemyPawn.SetPosition(
                                new Vector3(8 * TileSize, YOffset, -1 * TileSize)
                                - Bound
                                + new Vector3(TileSize / 2, 0, TileSize / 2)
                                + (Vector3.forward * DeathSpacing) * DeadBlack.Count
                            );
                    }
                    Pieces[enemyPawn.CurrentX, enemyPawn.CurrentY] = null;
                }
            }
        }

        if(SpecialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = MoveList[MoveList.Count - 1];
            Pieces targetPawn = Pieces[lastMove[1].x, lastMove[1].y];

            if(targetPawn.Type == ChessPieceType.Pawn)
            {
                if(targetPawn.Team == 0 && lastMove[1].y == 7)
                {
                    Pieces newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = Pieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(Pieces[lastMove[1].x, lastMove[1].y].gameObject);
                    Pieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                if (targetPawn.Team == 1 && lastMove[1].y == 0)
                {
                    Pieces newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = Pieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(Pieces[lastMove[1].x, lastMove[1].y].gameObject);
                    Pieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if(SpecialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = MoveList[MoveList.Count - 1];
            
            //Left Rook
            if(lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) //White side
                {
                    Pieces rook = Pieces[0, 0];
                    Pieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    Pieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) //Black side
                {
                    Pieces rook = Pieces[0, 7];
                    Pieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    Pieces[0, 7] = null;
                } 
            }
            //Right Rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) //White side
                {
                    Pieces rook = Pieces[7, 0];
                    Pieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    Pieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) //Black side
                {
                    Pieces rook = Pieces[7, 7];
                    Pieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    Pieces[7, 7] = null;
                }
            }

        }
    }

    //Kiểm tra checkmate
    private bool CheckForCheckmate()
    {
        var lastMove = MoveList[MoveList.Count - 1];
        int targetTeam = Pieces[lastMove[1].x, lastMove[1].y].Team == 0 ? 1 : 0;

        List<Pieces> attackingPieces = new List<Pieces>();
        List<Pieces> defendingPieces = new List<Pieces>();
        Pieces targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (Pieces[x, y] != null)
                {
                    if (Pieces[x, y].Team == targetTeam)
                    {
                        defendingPieces.Add(Pieces[x, y]);
                        if(Pieces[x, y].Type == ChessPieceType.King)
                        {
                            targetKing = Pieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(Pieces[x, y]);
                    }
                }
            }
        }

        //Is the king attacked right now
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for(int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref Pieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int b = 0; b < pieceMoves.Count; b++)
            {
                currentAvailableMoves.Add(pieceMoves[b]);
            }
        }

        // Are we in check right now?
        //King is under attack, can we move something to help him?
        for (int i = 0; i < defendingPieces.Count; i++)
        {
            List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref Pieces, TILE_COUNT_X, TILE_COUNT_Y);
            SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

            if (defendingMoves.Count != 0)
                return false;
        }

        return true; // Checkmate exist
    }

    private void Awake()
    {
        CurrentTurn = TEAM_WHITE;
        //Generate the chess board
        GenerateAllTiles(TileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

    //Tạo bàn cờ ảo
    public void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        YOffset += transform.position.y;
        Bound = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + BoardCenter;

        Tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for(int y = 0; y < tileCountY; y++)
            {
                Tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    //Tạo các ô cờ ảo
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject($"X: {x}, Y: {y}");
        tileObject.transform.parent = this.transform;

        //Add mesh component to render the object
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = TileMaterial;

        //Generate 4 corners of tile
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, YOffset, y * tileSize) - Bound;
        vertices[1] = new Vector3(x * tileSize, YOffset, (y+1) * tileSize) - Bound;
        vertices[2] = new Vector3((x+1) * tileSize, YOffset, y * tileSize) - Bound;
        vertices[3] = new Vector3((x+1) * tileSize, YOffset, (y+1) * tileSize) - Bound;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        //Recalculate light on mesh
        mesh.RecalculateNormals();

        //Set the layer named "Tile"
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        return tileObject;
    }

    //Trả về ví trí của quân cờ khi nó được chọn
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (Tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
            }
        }

        //Return the outbound index
        return -Vector2Int.one;
    }

    //Tạo các quân cờ
    private void SpawnAllPieces()
    {
        Pieces = new Pieces[TILE_COUNT_X, TILE_COUNT_Y];

        Pieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, TEAM_WHITE);
        Pieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, TEAM_WHITE);
        Pieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, TEAM_WHITE);
        Pieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, TEAM_WHITE);
        Pieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, TEAM_WHITE);
        Pieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, TEAM_WHITE);
        Pieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, TEAM_WHITE);
        Pieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, TEAM_WHITE);
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            Pieces[x, 1] = SpawnSinglePiece(ChessPieceType.Pawn, TEAM_WHITE);
        }

        Pieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, TEAM_BLACK);
        Pieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, TEAM_BLACK);
        Pieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, TEAM_BLACK);
        Pieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, TEAM_BLACK);
        Pieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, TEAM_BLACK);
        Pieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, TEAM_BLACK);
        Pieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, TEAM_BLACK);
        Pieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, TEAM_BLACK);
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            Pieces[x, 6] = SpawnSinglePiece(ChessPieceType.Pawn, TEAM_BLACK);
        }
    }

    //Tạo từng quân cờ
    private Pieces SpawnSinglePiece(ChessPieceType type, int team)
    {
        //Spawn object and get object's component
        Pieces piece = Instantiate(Prefabs[(int)type - 1], transform).GetComponent<Pieces>();
        piece.Type = type;
        piece.Team = team;
        piece.GetComponent<MeshRenderer>().material = TeamMaterials[team];

        return piece;
    }

    //Sắp xếp vị trí các quân cờ
    private void PositionAllPieces()
    {
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(Pieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    //Xếp quân cờ vào chính giữa ô cờ
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * TileSize, YOffset, y * TileSize) - Bound + new Vector3(TileSize / 2, 0, TileSize / 2);
    }

    private Vector3 GetTileCenterVer2(int x, int y)
    {
        return new Vector3(x * TileSize, YOffset - 0.2f, y * TileSize);
    }

    //Sắp xếp vị trí cho quân cờ
    private void PositionSinglePiece(int x, int y, bool force = true)
    {
        //Store the piece's current position
        Pieces[x, y].CurrentX = x;
        Pieces[x, y].CurrentY = y;
        //Calculate position for each piece
        Pieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    //Highlight các nước đi khả thi của quân cờ
    private void HightlightAvailableMoves()
    {
        foreach(var move in AvailableMoves)
        {
            Tiles[move.x, move.y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    //Remove highlght
    private void RemoveHightlight()
    {
        foreach(var move in AvailableMoves)
        {
            Tiles[move.x, move.y].layer = LayerMask.NameToLayer("Tile");
        }
        AvailableMoves.Clear();
    }

    //Kiểm tra nước đi có hợp lệ không
    private bool IsTileAvailableMove(ref List<Vector2Int> availableMoves, Vector2Int currentHover)
    {
        foreach(var move in availableMoves)
        {
            if (move.x == currentHover.x && move.y == currentHover.y)
                return true;
        }

        return false;
    }

    //Xử lý checkmate
    private void CheckMate(int winningTeam)
    {
        DisplayVictoryTeam(winningTeam);
    }

    //Hiển thị menu checkmate
    private void DisplayVictoryTeam(int winningTeam)
    {
        VictoryScreen.SetActive(true);
        VictoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    //Loại bỏ các nước đi gây nguy hiểm cho King
    private void PreventCheck()
    {
        Pieces targetKing = null;
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(Pieces[x,y] != null)
                {
                    if (Pieces[x, y].Type == ChessPieceType.King)
                    {
                        if (Pieces[x, y].Team == CurrentlyDragging.Team)
                        {
                            targetKing = Pieces[x, y];
                        }
                    }
                }
            }
        }

        //Since we are sending ref availableMoves, we will be deleting moves that are puttting us in check
        SimulateMoveForSinglePiece(CurrentlyDragging, ref AvailableMoves, targetKing);
    }

    //Loại bỏ các nước đi gây nguy hiểm cho King
    private void SimulateMoveForSinglePiece(Pieces piece, ref List<Vector2Int> moves, Pieces targetKing)
    {
        //Save the current value, to reset after the function call
        int actualX = piece.CurrentX;
        int actualY = piece.CurrentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //Going through all the moves, simulate them and check if we are in check
        for(int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.CurrentX, targetKing.CurrentY);
            //Did we simulate the king's move
            if(piece.Type == ChessPieceType.King)
            {
                kingPositionThisSim = new Vector2Int(simX, simY);
            }

            //Copy the [,] and not a reference
            Pieces[,] simulation = new Pieces[TILE_COUNT_X, TILE_COUNT_Y];
            List<Pieces> simAttackingPieces = new List<Pieces>();
            for(int x = 0; x < TILE_COUNT_X; x++)
            {
                for(int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if(Pieces[x,y] != null)
                    {
                        simulation[x, y] = Pieces[x, y];
                        if(simulation[x,y].Team != piece.Team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            //Simulate that move
            simulation[actualX, actualY] = null;
            piece.CurrentX = simX;
            piece.CurrentY = simY;
            simulation[simX, simY] = piece;

            // Did one of the piece got taken down during our simulation
            var deadPiece = simAttackingPieces.Find(p => p.CurrentX == simX && p.CurrentY == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);

            //Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for(int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for(int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            //Is the king in trouble, if so remove the move
            if(IsTileAvailableMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            //Restore the actual piece data
            piece.CurrentX = actualX;
            piece.CurrentY = actualY;
        }

        //Remove from the current available move list
        for(int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }

    //Action khi click vào nút Reset
    public void OnResetButton()
    {
        ////Disable canvas
        //VictoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        //VictoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        //VictoryScreen.SetActive(false);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    //Action khi click vào nút Exit
    public void OnExitButton()
    {
        Application.Quit();
    }
}
