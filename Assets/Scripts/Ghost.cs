using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board mainboard;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int newPosition = this.trackingPiece.position;

        int currentRow = newPosition.y;
        int bottom = (-this.mainboard.boardSize.y / 2) - 1;

        this.mainboard.Clear(this.trackingPiece);

        for (int row = currentRow; row >= bottom; row--)
        {
            newPosition.y = row;

            if(this.mainboard.IsValidPosition(this.trackingPiece, newPosition))
            {
                this.position = newPosition;
            }
            else
            {
                break;
            }
        }

        this.mainboard.Set(this.trackingPiece);
    }

    private void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }
}
