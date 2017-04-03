using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitMaskGrid : MonoBehaviour {
    public Sprite[] eightBitTiles;    
    public GameObject advancedTile;

    // 1 Unique bitmask value to 1 tile index
    // Many tiles may share a bitmask value
    // i.e tile 23 may share bm value 1, 7, 9.
    Dictionary<int, int> BtMskToTile = new Dictionary<int, int>()
    {
        { 0, 47 },
        // N, E, S, W   
        { 1, 1 },
        { 4, 5 },
        { 16, 13 },
        { 64, 2 },
        // N, E, +90ds
        { 5, 6 },
        { 20, 18 },
        { 80, 15 },
        { 65, 3 },
        // N, NE, E, +90ds
        { 7, 7 },
        { 28, 34 },
        { 112, 26 },
        { 193, 4 },
        // N, S, E, W
        { 17, 14 },
        { 68, 8 },
        // N, E, S, +90ds
        { 21, 19 },
        { 84, 21 },
        { 81, 16 },
        { 69, 9 },
        // N,S,E,NE +90ds
        { 23, 20 },
        { 92, 37 },
        { 113, 27 },
        { 197, 10 },
        // N,S,E,SE +90ds
        { 29, 35 },
        { 116, 29 },
        { 209, 17 },
        { 71, 11 },
        // N,S,NE,SE,E +90ds
        { 31, 36 },
        { 124, 42 },
        { 241, 28 },
        { 199, 12 },
        // N&E&S&W
        { 85, 22 },
        // N,NE,E,W,S,SE
        { 87, 24 },
        { 93, 38 },
        { 117, 0 },
        { 213, 32 },
        //
        { 95, 40 },
        { 125, 43 },
        { 245, 31 },
        { 215, 25 },
        // N,NE,E,W,SW,S
        { 119, 30 },
        { 221, 39 },
        // N,NE,E,SE,S,SW,W +90ds
        { 127, 45 },
        { 253, 44 },
        { 247, 33 },
        { 223, 41 },
        // Surrounded all sides
        { 255, 46 }
    };

    int[,] test_map =
    {
        {0,0,1,1,0 },
        {0,1,1,1,0 },
        {0,0,1,0,0 },
        {0,0,0,0,0 },
        {0,0,0,0,0 }
    };

    public CaveGenerator CaveGen;
    
    public enum TileDirection
    {
        BASE_TILE = 0,
        NORTH_WEST_TILE = 128,
        NORTH_TILE = 1,
        NORTH_EAST_TILE = 2,
        EAST_TILE = 4,
        SOUTH_EAST_TILE = 8,
        SOUTH_TILE = 16,
        SOUTH_WEST_TILE = 32,
        WEST_TILE = 64
    };

    // Use this for initialization
    void Start()
    {
        //int width = test_map.GetLength(0);
        //int height = test_map.GetLength(1);
        //createMap(width, height);
        
        CaveGen.GenerateMap(CaveGen.mapWidth, CaveGen.mapHeight);
        int[,] random_map = CaveGen.map;
        createMap(random_map);
        //createMap(test_map);
        //ResizeCamera(test_map.GetLength(0), test_map.GetLength(1));
        ResizeCamera(random_map.GetLength(0), random_map.GetLength(1));
    }

    private void ResizeCamera(int width, int height)
    {
        Camera.main.orthographicSize = (height + 2) / 2;
    }

    private void SpawnWaterTile(int width, int height, int x, int y)
    {
        Vector3 pos = new Vector3(-width / 2 + x + 0.5f,
                height / 2 - y + 0.5f,
                0);

        GameObject tile = Instantiate(advancedTile, pos, Quaternion.identity) as
            GameObject;

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.sprite = eightBitTiles[eightBitTiles.Length - 1];

        tile.name = "Tile(" + x + "," + y + ")";
    }

    private void SpawnTile(int[,] _map, int width, int height, int x, int y)
    {
        Vector3 pos = new Vector3(-width / 2 + x + 0.5f,
                height / 2 - y + 0.5f,
                0);

        GameObject tile = Instantiate(advancedTile, pos, Quaternion.identity) as
            GameObject;
        tile.name = "Tile(" + x + "," + y + ")";
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        int index = getBitMask(_map, x, y, width, height);
        if (BtMskToTile.ContainsKey(index))
        {
            sr.sprite = eightBitTiles[BtMskToTile[index]];
            //Bounds bounds = sr.sprite.bounds;
            //float size = bounds.size.x;
            //tile.transform.localScale = 

        }
        else
        {
            Debug.LogError("Key(" + index +") not found for tile: " + tile.name);
        }
    }

    public void createMap(int width, int height)
    {
        for (int i = -1; i < width + 1; i++)
        {
            for (int j = -1; j < height + 1; j++)
            {
                if (i < 0 || j < 0 || j == height || i == width)
                {
                    SpawnWaterTile(width, height, i, j);
                }
                else
                {
                    if (test_map[i, j] > 0)
                        SpawnTile(test_map, width, height, i, j);
                    else
                    {
                        SpawnWaterTile(width, height, i, j);
                    }
                }

            }
        }
    }


    public void createMap(int[,] _map)
    {
        int width = _map.GetLength(0);
        int height = _map.GetLength(1);

        for (int i = -1; i < width + 1; i++)
        {
            for (int j = -1; j < height + 1; j++)
            {
                if (i < 0 || j < 0 || j == height || i == width)
                {
                    SpawnWaterTile(width, height, i, j);
                }
                else
                {
                    if (_map[i, j] > 0)
                        SpawnTile(_map, width, height, i, j);
                    else
                    {
                        SpawnWaterTile(width, height, i, j);
                    }
                }

            }
        }
    }


    private TileDirection getDirection(int x, int y, int dx, int dy)
    {
        // if x is 0
        // and dx is -1
        // then -1 - 0 = -1 // North
        // checking: if x is 4, and dx 3; then 3 - 4 is -1, perfect 

        int tx = dx - x;
        int ty = dy - y;
        switch (tx)
        {
            case -1:
                switch (ty)
                {
                    case -1:
                        return TileDirection.NORTH_WEST_TILE;
                    case 0:
                        return TileDirection.WEST_TILE;
                    case 1:
                        return TileDirection.SOUTH_WEST_TILE;
                    default:
                        return TileDirection.BASE_TILE;
                }
            case 0:
                switch (ty)
                {
                    case -1:
                        return TileDirection.NORTH_TILE;
                    case 1:
                        return TileDirection.SOUTH_TILE;
                    default:
                        return TileDirection.BASE_TILE;
                }
            case 1:
                switch (ty)
                {
                    case -1:
                        return TileDirection.NORTH_EAST_TILE;
                    case 0:
                        return TileDirection.EAST_TILE;
                    case 1:
                        return TileDirection.SOUTH_EAST_TILE;
                    default:
                        return TileDirection.BASE_TILE;
                }
            default:
                return TileDirection.BASE_TILE;
        }
    }

    /// <summary>
    /// Returns the bitmask value of a given tile.
    /// </summary>
    /// <param name="map">The 2D grid representing the map.</param>
    /// <param name="x">x coordinate.</param>
    /// <param name="y">y coordinate.</param>
    /// <param name="width">width bounds of the map.</param>
    /// <param name="height">height bounds of the map.</param>
    /// <returns>The bitmask value of a given tile coordinate (x,y)</returns>
    private int getBitMask(int[,] _map, int x, int y, int width, int height)
    {
        int value = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i != x || j != y)
                {
                    if (isInBounds(i, j, width, height) &&
                        isWallTile(_map, i,j))
                    {
                        TileDirection dir = getDirection(x, y, i, j);

                        if (dir == TileDirection.NORTH_EAST_TILE)
                        {
                            if (_map[i - 1, j] > 0 && _map[i, j + 1] > 0)
                            {
                                value += 1 *
                                    (int)dir;
                            }
                        }
                        else if (dir == TileDirection.NORTH_WEST_TILE)
                        {
                            if (_map[i + 1, j] > 0 && _map[i, j + 1] > 0)
                            {
                                value += 1 *
                                    (int)dir;
                            }
                        }
                        else if (dir == TileDirection.SOUTH_EAST_TILE)
                        {
                            if (_map[i - 1, j] > 0 && _map[i, j - 1] > 0)
                            {
                                value += 1 *
                                    (int)dir;
                            }
                        }
                        else if (dir == TileDirection.SOUTH_WEST_TILE)
                        {
                            if (_map[i + 1, j] > 0 && _map[i, j - 1] > 0)
                            {
                                value += 1 *
                                    (int)dir;
                            }
                        }
                        else
                        {
                            value += 1 *
                                (int)dir;
                        }

                    }
                }
            }
        }

        return value;
    }

    private bool isInBounds(int x, int y, int width, int height)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return true;
        else
            return false;
    }

    private bool isWallTile(int[,] _map, int x, int y)
    {
        if (_map[x, y] == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
