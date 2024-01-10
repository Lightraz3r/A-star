using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { walkable, nonWalkable}

public class AStarTile : Tile
{//s�ger om en tile �r walkable eller inte(om man kan g� p� tilen)
    [SerializeField]
    TileType typeOfTile = TileType.walkable;
    public TileType TypeOfTile { get => typeOfTile; }

}

public class TileInfo
{//Data p� tiles
    public Vector3Int Position { get; } = Vector3Int.zero;
    public TileInfo Parent { get; set; } = null;
    public float G { get; set; } = 0;
    public float H { get; set; } = 0;
    public float F { get => G + H; }
    public bool Target = false;

    public TileInfo (Vector3Int position, TileInfo parent, float g, float h)
    {
        Position = position;
        Parent = parent;
        G = g;
        H = h;
    }
}




