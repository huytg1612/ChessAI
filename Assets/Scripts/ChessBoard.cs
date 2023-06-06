using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!CurrentCamera)
        {
            CurrentCamera = Camera.main;
            return;
        }

        RaycastHit hitInfo; //Get the gameobject's info when it is hit
        Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition); //Whenever mouse moved, get the Ray

        //IF the Ray hit the game object with Layer Mask named "Tile, Hover, Highlight", get the gameobject info 
        if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = LookupTileIndex(hitInfo.transform.gameObject);

            //If the previous hover is nothing, we set the currentHover to hitPosition
            if(CurrentHover == -Vector2Int.one)
            {
                CurrentHover = hitPosition;
                Tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //If the hitPosition is different from the previous hover, set the previous hover to Tile and the hitPosition to current 
            if(CurrentHover != hitPosition)
            {
                Tiles[CurrentHover.x, CurrentHover.y].layer = IsTileAvailableMove(CurrentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                CurrentHover = hitPosition;
                Tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //If pick the piece up
            if (Input.GetMouseButtonDown(0))
            {
                if(Pieces[hitPosition.x, hitPosition.y] != null)
                {
                    if (CurrentTurn == Pieces[hitPosition.x, hitPosition.y].Team)
                    {
                        CurrentlyDragging = Pieces[hitPosition.x, hitPosition.y];
                        AvailableMoves = CurrentlyDragging.GetAvailableMoves(ref Pieces, TILE_COUNT_X, TILE_COUNT_Y);
                        //Get a list of special moves
                        SpecialMove = CurrentlyDragging.GetSpecialMoves(ref Pieces, ref MoveList, ref AvailableMoves);
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
            if(CurrentHover != -Vector2Int.one)
            {
                Tiles[CurrentHover.x, CurrentHover.y].layer = IsTileAvailableMove(CurrentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                CurrentHover = -Vector2Int.one;
            }

            //Prevent the case, move chess out of board.
            if(CurrentlyDragging && Input.GetMouseButtonUp(0))
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

    private bool MoveTo(Pieces piece, int x, int y)
    {
        if(!IsTileAvailableMove(new Vector2Int(x, y)))
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
        PositionSinglePiece(x, y);

        if (CurrentTurn == TEAM_WHITE)
            CurrentTurn = TEAM_BLACK;
        else
            CurrentTurn = TEAM_WHITE;

        MoveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();
        return true;
    }

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

    private void Awake()
    {
        CurrentTurn = TEAM_WHITE;
        //Generate the chess board
        GenerateAllTiles(TileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

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

    //Get the gameobject's position which is hit
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

    //Spawning of the Pieces
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

    private Pieces SpawnSinglePiece(ChessPieceType type, int team)
    {
        //Spawn object and get object's component
        Pieces piece = Instantiate(Prefabs[(int)type - 1], transform).GetComponent<Pieces>();
        piece.Type = type;
        piece.Team = team;
        piece.GetComponent<MeshRenderer>().material = TeamMaterials[team];

        return piece;
    }

    //Positioning
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

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * TileSize, YOffset, y * TileSize) - Bound + new Vector3(TileSize / 2, 0, TileSize / 2);
    }

    private Vector3 GetTileCenterVer2(int x, int y)
    {
        return new Vector3(x * TileSize, YOffset - 0.2f, y * TileSize);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        //Store the piece's current position
        Pieces[x, y].CurrentX = x;
        Pieces[x, y].CurrentY = y;
        //Calculate position for each piece
        Pieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    //Hightlight available moves
    private void HightlightAvailableMoves()
    {
        foreach(var move in AvailableMoves)
        {
            Tiles[move.x, move.y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHightlight()
    {
        foreach(var move in AvailableMoves)
        {
            Tiles[move.x, move.y].layer = LayerMask.NameToLayer("Tile");
        }
        AvailableMoves.Clear();
    }

    private bool IsTileAvailableMove(Vector2Int currentHover)
    {
        foreach(var move in AvailableMoves)
        {
            if (move.x == currentHover.x && move.y == currentHover.y)
                return true;
        }

        return false;
    }

    private void CheckMate(int winningTeam)
    {
        DisplayVictoryTeam(winningTeam);
    }

    private void DisplayVictoryTeam(int winningTeam)
    {
        VictoryScreen.SetActive(true);
        VictoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnResetButton()
    {
        ////Disable canvas
        //VictoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        //VictoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        //VictoryScreen.SetActive(false);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}
