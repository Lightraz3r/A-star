using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class AStar
{
    public static List<Vector3> FindPath(this Tilemap tilemap, Vector3 startPos, Vector3 endPos, bool showPath)
    {
        Vector3Int startTilePos = tilemap.WorldToCell(startPos);
        Vector3Int endTilePos = tilemap.WorldToCell(endPos);

        TileInfo[,] grid = new TileInfo[tilemap.size.x, tilemap.size.y];

        //lägger varenda tile i tilemapen ett värde. Hjälper till null-checks
        for (int y = 0; y < grid.GetLength(1) ; y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Vector3Int pos = new Vector3Int(x + tilemap.origin.x, y + tilemap.origin.y);
                AStarTile tile = tilemap.GetTile<AStarTile>(pos);
                if (tile != null)
                {
                    grid[x, y] = new TileInfo(pos, null, 0, 0);
                    tilemap.SetColor(pos, tile.color);
                }
            }
        }

        TileInfo startTile = new TileInfo(startTilePos, null, 0, 0);
        grid[startTile.Position.x - tilemap.origin.x, startTile.Position.y - tilemap.origin.y] = startTile;

        List<TileInfo> toSearch = new List<TileInfo>() { startTile };
        List<TileInfo> searched = new List<TileInfo>();
        List<TileInfo> Corners = new List<TileInfo>() { startTile };
        while (toSearch.Count > 0)
        {
            TileInfo current = toSearch[0];
            foreach (var tile in toSearch)
            {
                if (tile.F < current.F)
                {
                    current = tile;
                }
            }

            searched.Add(current);
            toSearch.Remove(current);

            if (current.Position == endTilePos)
            {//Framställer vägen
                TileInfo tile = current;
                List<Vector3> path = new List<Vector3>();
                //bool start = SortCorner(tilemap, startTilePos, endTilePos);
                while (tile.Position != startTilePos)
                {
                    //if (tile.Target && start)
                    //{
                    //    if (!SortCorner(tilemap, tile.Position, endPos))
                    //    {
                    //        path.Clear();
                    //    }
                    //    path.Add(tile.Position);
                    //}
                    if (tile.Target)
                    {
                        path.Add(tile.Position);
                    }
                    SetTileColor(tile.Position, tile.Target ? Color.green : Color.red, tilemap, showPath);
                    tile = tile.Parent;
                }
                SetTileColor(startTilePos, Color.white, tilemap, showPath);
                SetTileColor(endTilePos, Color.black, tilemap, showPath);
                endPos = Offset(tilemap, endPos, endTilePos);

                path.Reverse();
                path.Add(endPos - new Vector3(0.5f, 0.5f, 0));
                return path;
            }

            //Letar efter en möjlig gåbar tile och framställer den
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    Vector3Int pos = new Vector3Int(current.Position.x + x, current.Position.y + y);
                    Vector3Int gridPos = pos - tilemap.origin;

                    //Kollar om position går över leveln
                    if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x > grid.GetLength(0) - 1 || gridPos.y > grid.GetLength(1) - 1) { continue; }

                    //Kollar om tilen vid position är inte nuvarande, null
                    if (searched.Contains(grid[gridPos.x, gridPos.y]) || grid[gridPos.x, gridPos.y] == null) { continue; }

                    //Kollar om den nuvarande tilen är ett hörn 
                    if (x != 0 && y != 0 && CheckCorner(tilemap, current, pos, endTilePos, Corners))
                    {
                        current.Target = true;
                        Corners.Add(current);
                    }

                    //Kollar om ai:n kan gå på tilen eller en hörn. Om det finns en, eller två, non-walkable tile på sidorna av hörnan
                    if (!GetWalkable(pos.x, pos.y, tilemap) || !GetWalkable(pos.x, current.Position.y, tilemap) || !GetWalkable(current.Position.x, pos.y, tilemap)) { continue; }

                    //Kollar om G värdet på en förra tile är större än den nuvarande (om den är mindre vill vi kolla den igen)
                    if ((toSearch.Contains(grid[gridPos.x, gridPos.y]) && current.G + ValueG(current.Position, pos) > grid[gridPos.x, gridPos.y].G)) { continue; }

                    SetTileColor(pos, Color.magenta, tilemap, showPath);
                    toSearch.Add(CreateTile(pos, grid, current, endTilePos, tilemap));
                }
            }
        }

        return null;
    }

    private static Vector3 Offset(Tilemap tilemap, Vector3 endPos, Vector3Int endTilePos)
    {
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x == 0 || y == 0)
                {
                    Vector3Int pos = new Vector3Int(endTilePos.x + x, endTilePos.y + y);
                    if (!GetWalkable(pos.x, pos.y, tilemap))
                    {
                        if (x != 0)
                        {
                            pos.x += x == -1 ? 1 : 0;
                            float xChange = pos.x - endPos.x;
                            if (Mathf.Abs(xChange) < 0.5)
                            {
                                endPos.x -= NegMod(xChange) * 0.5f - xChange;
                            }
                        }
                        else
                        {
                            pos.y += y == -1 ? 1 : 0;
                            float yChange = pos.y - endPos.y;
                            if (Mathf.Abs(yChange) < 0.5)
                            {
                                endPos.y -= NegMod(yChange) * 0.5f - yChange;
                            }
                        }
                    }
                }
            }
        }

        return endPos;
    }

    private static int NegMod(float xChange)
    {
        if (xChange < 0)
        {
            return -1;
        }
        return xChange == 0 ? 0 : 1;
    }

    private static bool SortCorner(Tilemap tilemap, Vector3 tile, Vector3 target)
    { // kollar om det finns något som blockerar målet
        if ((int)target.x - (int)tile.x == 0)
        {
            float y = (int)tile.y;
            while (y != (int)target.y)
            {
                y += NegMod(target.y - y) * 0.125f;

                for (int i = -1; i < 2; i++)
                {
                    if (!GetWalkable(tile.x + (float)i / 3, y, tilemap))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            float k = (target.y - tile.y) / (target.x - tile.x );

            float m = tile.y - k * tile.x;

            float x = tile.x;
            while (x != (int)target.x)
            {
                x += NegMod(target.x - x) * 0.125f;

                for (int i = -1; i < 2; i++)
                {
                    if (!GetWalkable(x, k * x + m + (float)i / 3, tilemap))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private static bool CheckCorner(Tilemap tilemap, TileInfo current, Vector3Int neighbour, Vector3Int end, List<TileInfo> corners)
    { //Kollar efter hörn
        if (!current.Target && current.Parent != null && !GetWalkable(neighbour.x, neighbour.y, tilemap) && GetWalkable(neighbour.x, current.Position.y, tilemap) && GetWalkable(current.Position.x, neighbour.y, tilemap))
        {
            //TileInfo tile = current;
            //while (tile.Parent != null)
            //{
            //    if (tile.Parent.Position.x == neighbour.x || tile.Parent.Position.y == neighbour.y)
            //    {
            //        return true;
            //    }
            //    tile = tile.Parent;
            //}
            for (int i = 0; i < corners.Count; i++)
            {
                if (SortCorner(tilemap, current.Position, corners[i].Position))
                {
                    Debug.Log("Before: " + corners.Count);
                    for (int j = i + 1; j < corners.Count; j++)
                    {
                        corners[j].Target = false;
                        corners.RemoveAt(j);
                    }
                    Debug.Log("After: " + corners.Count);
                    return true;
                }
            }
        }
        return false;
    }

    public static bool GetWalkable(float x, float y, Tilemap tilemap)
    {//Kollar om tilen är en walkable eller inte, alltså om ai kan gå på tilen
        Vector3Int tile = tilemap.WorldToCell(new Vector2(x, y));
        if(tilemap.GetTile<AStarTile>(tile) == null)
        {
            return false;
        }
        return tilemap.GetTile<AStarTile>(tile).TypeOfTile == TileType.walkable;
    }

    private static void SetTileColor(Vector3Int pos, Color color, Tilemap tilemap, bool showPath)
    {//Ändrar färgen på tiles, används till debug
        if (showPath)
        {
            tilemap.RemoveTileFlags(pos, TileFlags.LockColor);
            tilemap.SetColor(pos, color);
        }
        return;
    }

    private static TileInfo CreateTile(Vector3Int pos, TileInfo[,] grid, TileInfo parent, Vector3Int endTilePos, Tilemap tilemap)
    {//framställer en ny tile
        float g = ValueG(pos, parent.Position) + parent.G;

        float h = 10 * (Mathf.Abs(endTilePos.x - pos.x) + Mathf.Abs(endTilePos.y - pos.y));

        grid[pos.x - tilemap.origin.x, pos.y - tilemap.origin.y] = new TileInfo(pos, parent, g, h);
        return grid[pos.x - tilemap.origin.x, pos.y - tilemap.origin.y];
    }

    private static float ValueG(Vector3Int pos, Vector3Int other)
    {
        float g = 14;
        if (pos.x == other.x || pos.y == other.y)
        {
            g = 10;
        }
        return g;
    }
}
    
