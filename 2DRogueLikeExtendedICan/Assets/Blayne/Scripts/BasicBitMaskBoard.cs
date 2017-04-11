using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBitMaskBoard : MonoBehaviour {
    public Sprite[] BasicTiles;

    public GameObject basicTile;

    int[,] test_map =
    {
        {1,1,1,1 },
        {1,0,1,0 },
        {0,1,1,1 },
        {1,1,1,1 }
    };

    // Use this for initialization
    void Start () {
        int width = test_map.GetLength(0);
        int height = test_map.GetLength(1);
        createMap(width, height);
        ResizeCamera(width, height);
    }

    private void ResizeCamera(int width, int height)
    {
        Camera.main.orthographicSize = (height + 2) / 2;
    }

    private void SpawnWaterTile(int width, int height, int x, int y)
    {
        Vector3 pos = new Vector3(-width / 2 + x,
                height / 2 - y,
                0);

        GameObject tile = Instantiate(basicTile, pos, Quaternion.identity) as
            GameObject;

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.sprite = BasicTiles[BasicTiles.Length - 1];

        tile.name = "Tile(" + x + "," + y + ")";
    }

    private void SpawnTile(int width, int height, int x, int y)
    {
        Vector3 pos = new Vector3(-width / 2 + x,
                height / 2 - y,
                0);

        GameObject tile = Instantiate(basicTile, pos, Quaternion.identity) as
            GameObject;

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.sprite = BasicTiles[getBitMask(test_map, x, y, width, height)];

        tile.name = "Tile(" + x + "," + y + ")";
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
                        SpawnTile(width, height, i, j);
                    else
                    {
                        SpawnWaterTile(width, height, i, j);
                    }
                }

            }
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
    private int getBitMask(int[,] map, int x, int y, int width, int height)
    {
        // Clockwise order, North, East, South, West
        // 1, 2, 4, 8
        // Assume "Island" layout, so "map edges" are to be treated as adjacent
        // to "water"/floors.
        int value = 0;

        int north = 0;
        int west = 0;
        int south = 0;
        int east = 0;

        // north
        if (y - 1 >= 0)
        {
            north = map[x, y - 1] * 1;
        }

        // east
        if (x + 1 < width)
            east = map[x + 1, y] * 2;

        // south
        if (y + 1 < height)
            south = map[x, y + 1] * 4;

        // west
        if (x - 1 >= 0)
            west = map[x - 1, y] * 8;

        // Basic Bitmasking operations      
        value = north + east + south + west;         

        return value;
    }
	
	private bool isWallTile(int[,] map, int x, int y)
    {
        if (map[x,y] == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
