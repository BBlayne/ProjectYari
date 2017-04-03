using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour {
    public int[,] map;
    int[,] tempMap;

    public int mapHeight = 15;
    public int mapWidth = 15;

    [Range(0, 7)]
    public int TilesThreshold1 = 4;

    [Range(0, 4)]
    public int TilesThreshold2 = 2;

    [Range(0, 5)]
    public int smoothIterations = 3;

    [Range(0, 5)]
    public int smoothIterations2 = 1;

    public string randomSeed = "Blayne";
    public bool useRandomSeed = false;

    public MapTypes mapMode = MapTypes.CAVE;

    public enum MapTypes
    {
        CAVE = 0,
        MAZE
    };

    [Range(0, 100)]
    public int fillPercentage = 32;

    private void ResizeCamera()
    {
        Camera.main.orthographicSize = mapHeight / 2;
    }

    private void Start()
    {
        GenerateMap(mapWidth, mapHeight);
        ResizeCamera();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {            
            GenerateMap(mapWidth, mapHeight);
            ResizeCamera();
        }
    }

    public void GenerateMap(int width, int height)
    {
        mapWidth = width;
        mapHeight = height;
        map = new int[width, height];
        tempMap = new int[width, height];
        RandomFillMap(width, height);

        switch (mapMode)
        {
            case MapTypes.CAVE:
                CaveSmoothing();
                break;
            case MapTypes.MAZE:
                MazeSmoothing();
                break;
            default:
                CaveSmoothing();
                break;
        }
    }

    private void MazeSmoothing()
    {
        for (int i = 0; i < smoothIterations; i++)
        {
            MazeSmoothMap();
        }
    }

    private void CaveSmoothing()
    {
        for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap();
            //MazeSmoothMap();
        }

        for (int i = 0; i < smoothIterations2; i++)
        {
            SmoothMapTweaked();
        }
    }

    private void RandomFillMap(int width, int height)
    {
        if (useRandomSeed)
        {
            randomSeed = Time.time.ToString();
        }

        System.Random pseudoRandom = 
            new System.Random(randomSeed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // '=' is assignment. aka x = 1; means the value of x is 1.
                // '==' is comparison. Are these two things equivilent?
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1; // is a wall
                }
                //else if (x == 1 || y == 1 || y == height - 2 || x == width - 2)
                //{
                //    map[x, y] = 0;
                //}
                else
                {
                    //map[x, y] = (pseudoRandom.Next(0, 100))
                    int myRandomDiceRoll = pseudoRandom.Next(0, 100);
                    if (myRandomDiceRoll < fillPercentage)
                    {
                        map[x, y] = 1; // is a wall
                    }
                    else
                    {
                        map[x, y] = 0; // is a floor
                    }
                }
            }
        }
    }

    private void MazeSmoothMap()
    {
        for (int x = 2; x < mapWidth - 2; x++)
        {
            for (int y = 2; y < mapHeight - 2; y++)
            {
                int neighbourWallTiles =
                    GetSurroundingWallCount(1, x, y);

                if (neighbourWallTiles >= TilesThreshold1)
                    map[x, y] = 0;
                else
                    map[x, y] = 1;
            }
        }
    }


    private void SmoothMap()
    {
        //tempMap = map;
        tempMap = map.Clone() as int[,];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                ///int neighbourWallTilesCount =
                //    GetSurroundingWallCount(x, y);

                int WallTileCountWithin1Steps =
                    GetSurroundingWallCount(1, x, y);

                int WallTileCountWithin2Steps =
                    GetSurroundingWallCount(2, x, y);

                if (WallTileCountWithin1Steps >= TilesThreshold1
                    || WallTileCountWithin2Steps <= TilesThreshold2)
                {
                    tempMap[x, y] = 1;
                }
                else
                {
                    tempMap[x, y] = 0;
                }
            }
        }
        map = tempMap.Clone() as int[,];
    }

    private void SmoothMapTweaked()
    {        
        tempMap = map.Clone() as int[,];        
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int neighbourWallTilesCount =
                    GetSurroundingWallCount(1, x, y);

                if (neighbourWallTilesCount >= TilesThreshold1)
                {
                    tempMap[x, y] = 1;
                }
                else
                {
                    tempMap[x, y] = 0;
                }
            }
        }
        map = tempMap.Clone() as int[,];
    }

    private int GetSurroundingWallCount(int steps, int x, int y)
    {
        int wallCount = 0;
        for (int neighbourX = x - steps; neighbourX <= x + steps; neighbourX++)
        {
            for (int neighbourY = y - steps; neighbourY <= y + steps; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapWidth && 
                    neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (neighbourX != x || neighbourY != y)
                    {
                        // walls are 1, floors are 0
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {

                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;

                    Vector3 pos = new Vector3(-mapWidth / 2 + x + 0.5f,
                        mapHeight / 2 - y + 0.5f,
                        0);

                    //Gizmos.DrawCube(pos, Vector3.one);


                }
            }
        }
    }
}
