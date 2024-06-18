using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public NextPiece nextPiece { get; private set; }
    public TetronimoData[] tetronimoes;
    public Vector3Int spawnPosition;
    public Vector3Int nextPiecePosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Tile clearingTile;
    public MusicController musicController;
    public Text scoreText;

    public float stepDelay = 1f;
    public int pieceCount = 0;

    private List<int> tetrominoBag;
    private int level = 0;
    private int score = 0;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.nextPiece = GetComponentInChildren<NextPiece>();

        this.scoreText.text = score.ToString();

        for(int i = 0; i < this.tetronimoes.Length; i++)
        {
            this.tetronimoes[i].Initialize();
        }

        ResetTetrominoBag();
    }

    private void Start()
    {
        SpawnStartingPieces();
    }

    private void ResetTetrominoBag()
    {
        tetrominoBag = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    }

    public void SpawnStartingPieces()
    {
        // starting piece
        int random = Random.Range(0, tetrominoBag.Count);
        TetronimoData data = this.tetronimoes[tetrominoBag[random]];
        tetrominoBag.RemoveAt(random);

        this.activePiece.Initialize(this, this.spawnPosition, data);
        Set(this.activePiece);

        // starting next piece

        int randomNext = Random.Range(0, tetrominoBag.Count);
        TetronimoData dataNext = this.tetronimoes[tetrominoBag[randomNext]];
        tetrominoBag.RemoveAt(randomNext);

        this.nextPiece.Initialize(this, this.nextPiecePosition, dataNext);
        SetAsNext(this.nextPiece);
    }

    public void SpawnPiece()
    {
        if(tetrominoBag.Count == 0)
        {
            ResetTetrominoBag();
        }

        this.activePiece.Initialize(this, this.spawnPosition, nextPiece.data);

        if(IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }

        Clear(this.nextPiece);

        int random = Random.Range(0, tetrominoBag.Count);
        TetronimoData data = this.tetronimoes[tetrominoBag[random]];
        tetrominoBag.RemoveAt(random);

        this.nextPiece.Initialize(this, nextPiecePosition, data);
        SetAsNext(this.nextPiece);
    }

    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void SetAsNext(NextPiece nextPiece)
    {
        for (int i = 0; i < nextPiece.cells.Length; i++)
        {
            Vector3Int tilePosition = nextPiece.cells[i] + nextPiece.position;
            this.tilemap.SetTile(tilePosition, nextPiece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public void Clear(NextPiece nextPiece)
    {
        for (int i = 0; i < nextPiece.cells.Length; i++)
        {
            Vector3Int tilePosition = nextPiece.cells[i] + nextPiece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if(this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        IncreaseScore(10);

        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        int linesClearedCount = 0;

        while(row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesClearedCount++;
            }
            else
            {
                row++;
            }
        }

        switch (linesClearedCount)
        {
            case 0:
                break;
            case 1:
                IncreaseScore(100);
                break;
            case 2:
                IncreaseScore(250);
                break;
            case 3:
                IncreaseScore(600);
                break;
            case 4:
                IncreaseScore(1500);
                break;
            default:
                break;
        }
    }

    private void IncreaseScore(int increase)
    {
        score += increase;
        scoreText.text = score.ToString();
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            this.tilemap.SetTile(position, clearingTile);
        }

        MoveRowsAboveDownwards(row);
    }

    private void MoveRowsAboveDownwards(int row)
    {
        RectInt bounds = this.Bounds;

        for (int rowAbove = row + 1; rowAbove < bounds.height; rowAbove++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, rowAbove, 0);
                TileBase tileAbove = this.tilemap.GetTile(position);

                Vector3Int positionBelow = new Vector3Int(col, rowAbove - 1, 0);

                this.tilemap.SetTile(positionBelow, tileAbove);
            }
        }
    }

    public void CheckForNextLevel()
    {
        if(pieceCount % 20 == 0)
        {
            level++;
            stepDelay = stepDelay * 0.9f;
        }

        if(pieceCount == 80)
        {
            musicController.StartPart2();
        }
    }

    private void GameOver()
    {
        musicController.StopMusic();

        this.tilemap.ClearAllTiles();
        pieceCount = 0;
        score = 0;
        scoreText.text = score.ToString();

        musicController.StartMusic();
    }
}
