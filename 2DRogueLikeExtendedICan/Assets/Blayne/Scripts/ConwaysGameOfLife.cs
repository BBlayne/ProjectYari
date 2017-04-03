using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Life
{
    ManualResetEvent _doneEvent;
    int[,] map;
    int[,] new_map;
    int mapWidth;
    int mapHeight;
    int ReproductionThreshold = 3;
    int OverPopThreshold = 3;
    int UnderPopThreshold = 2;
    int LivesThresholdRangeA = 2;
    int LivesThresholdRangeB = 3;

    int StartX;
    int StartY;
    int EndX;
    int EndY;

    public int[,] GetMap() { return new_map; }

    public Life(int mapWidth, int mapHeight, 
        int[,] map,
        int[,] new_map,
        int StartX, int StartY,
        int EndX, int EndY,
        ManualResetEvent doneEvent)
    {
        this._doneEvent = doneEvent;
        this.map = map;
        this.new_map = new_map;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.StartX = StartX;
        this.StartY = StartY;
        this.EndX = EndX;
        this.EndY = EndY;
    }

    // Wrapper method for use with thread pool.
    public void ThreadPoolCallback(System.Object threadContext)
    {
        int threadIndex = (int)threadContext;
        //_fibOfN = Calculate(_n);  
        Debug.Log("thread " + threadIndex + " started.");
        SimulationIterationKernel(StartX, StartY, EndX, EndY);
        _doneEvent.Set();
    }

    private int GetSurroundingLiveCellCount(int x, int y)
    {
        int liveCellCount = 0;
        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
        {
            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapWidth &&
                    neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (neighbourX != x || neighbourY != y)
                    {
                        // walls are 1, floors are 0
                        liveCellCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    int _y = neighbourY;
                    int _x = neighbourX;
                    if (neighbourX < 0)
                    {
                        _x = (neighbourX % mapWidth + mapWidth) % mapWidth;
                    }
                    else if (neighbourX >= mapWidth)
                    {
                        _x = (neighbourX % mapWidth);
                    }

                    if (neighbourY < 0)
                    {
                        _y = (neighbourY % mapHeight + mapHeight) % mapHeight;
                    }
                    else if (neighbourY >= mapHeight)
                    {
                        _y = (neighbourY % mapHeight);
                    }

                    liveCellCount += map[_x, _y];

                }
            }
        }

        return liveCellCount;
    }


    private void SimulationIterationKernel(int offsetX,
        int offsetY, int _width, int _height)
    {

        for (int x = offsetX; x < _width; x++)
        {
            for (int y = offsetY; y < _height; y++)
            {
                int neighbourLiveCellCount =
                    GetSurroundingLiveCellCount(x, y);

                if (map[x, y] == 0)
                {
                    if (neighbourLiveCellCount == ReproductionThreshold)
                    {
                        new_map[x, y] = 1;
                    }
                }
                else
                {
                    if (neighbourLiveCellCount < UnderPopThreshold)
                    {
                        new_map[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount > OverPopThreshold)
                    {
                        new_map[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount >= LivesThresholdRangeA &&
                        neighbourLiveCellCount <= LivesThresholdRangeB)
                    {
                        new_map[x, y] = 1;
                    }
                }
            }
        }
    }
}

public class ConwaysGameOfLife : MonoBehaviour {
    static int[,] map_copy;
    static int[,] map;

    public static int mapHeight = 100;
    public static int mapWidth = 100;

    public string randomSeed = "Blayne";
    public bool useRandomSeed = false;

    private static int threadCount = 0;

    [Range(0, 100)]
    public int fillPercentage = 10;

    public float IterationInSeconds = 1;
    private float timer;

    public static int UnderPopThreshold = 2;
    public static int LivesThresholdRangeA = 2;
    public static int LivesThresholdRangeB = 3;
    public static int OverPopThreshold = 3;
    public static int ReproductionThreshold = 3;

    public bool isRunning = false;
    private bool isFinishedWork = true;

    private List<Thread> threads = new List<Thread>();
    //private List <ManualResetEvent> doneEvents = new List<ManualResetEvent>();
    private ManualResetEvent[] doneEvents;
    private List<Life> lives = new List<Life>();

    private void ResizeCamera()
    {
        Camera.main.orthographicSize = mapHeight / 2;
    }

    private void Start()
    {
        GenerateMap(mapWidth, mapHeight);
        ResizeCamera();
        //SetupThreads(2, 2);
        //Tick();
    }

    private void Update()
    {
        if (isRunning && isFinishedWork)
        {
            timer -= Time.deltaTime;
            if (timer <= Time.time)
            {
                Tick();
                timer = Time.time + IterationInSeconds;
            }

            //print(timer);
        }
    }

    public void ToggleRunningSimulation(bool flag)
    {
        isRunning = flag;
        if (isRunning)
            timer = Time.time + IterationInSeconds;
        else
            timer = Time.time;
    }

    public void ResetSim()
    {
        GenerateMap(mapWidth, mapHeight);
        ResizeCamera();
        timer = Time.time + IterationInSeconds;
        Tick();
    }

    public void Tick()
    {
        isFinishedWork = false;
        StartCoroutine(TickRoutine());
    }

    private void SetupThreads(int _x, int _y)
    {
        doneEvents = new ManualResetEvent[_x * _y];
        int count = 0;
        for (int i = 0; i < _x; i++)
        {
            for (int j = 0; j < _y; j++)
            {
                /*
                Thread thread = new Thread(
                    () => SimulationIterationKernel(
                        i * (mapWidth / _x),
                    j * (mapHeight / _y),
                    (i + 1) * (mapWidth / _x),
                    (j + 1) * (mapHeight / _y)));

                threads.Add(thread);
                */
                doneEvents[count] = new ManualResetEvent(false);
                lives.Add(new Life(mapWidth, mapHeight, map, map_copy,
                   i * (mapWidth / _x),
                   j * (mapHeight / _y),
                   (i + 1) * (mapWidth / _x),
                   (j + 1) * (mapHeight / _y), doneEvents[count]));
                count++;
                // (0, 0, (mapWidth / 2), (mapHeight / 2)
                // (0, (mapHeight / 2), (mapWidth / 2), mapHeight)
                // ((mapWidth / 2), 0, mapWidth, (mapHeight / 2))
                // (1, 1, mapWidth, mapHeight
            }
        }

        foreach (Thread thread in threads)
        {
            //thread.Start();
            //print("Thread " + thread.ManagedThreadId + " is started");
        }
    }

    private void TickThreaded()
    {
        map_copy = map.Clone() as int[,];
        int _x = (mapWidth / 50);
        int _y = (mapHeight / 50);
        threads.Clear();
        for (int i = 0; i < _x; i++)
        {
            for (int j = 0; j < _y; j++)
            {
                /*
                Thread thread = new Thread(
                    () => SimulationIterationKernel(
                        i * (mapWidth / _x),
                    j * (mapHeight / _y),
                    (i + 1) * (mapWidth / _x),
                    (j + 1) * (mapHeight / _y)));

                threads.Add(thread);
                */
            }
        }

        Thread thread1 = new Thread(
                    () => SimulationIterationKernel(
                        0 * (mapWidth / _x),
                    0 * (mapHeight / _y),
                    (0 + 1) * (mapWidth / _x),
                    (0 + 1) * (mapHeight / _y)));
        Thread thread2 = new Thread(
                  () => SimulationIterationKernel(
                      1 * (mapWidth / _x),
                  0 * (mapHeight / _y),
                  (1 + 1) * (mapWidth / _x),
                  (0 + 1) * (mapHeight / _y)));
        Thread thread3 = new Thread(
                  () => SimulationIterationKernel(
                      0 * (mapWidth / _x),
                  1 * (mapHeight / _y),
                  (0 + 1) * (mapWidth / _x),
                  (1 + 1) * (mapHeight / _y)));

        Thread thread4 = new Thread(
                    () => SimulationIterationKernel(
                        1 * (mapWidth / _x),
                    1 * (mapHeight / _y),
                    (1 + 1) * (mapWidth / _x),
                    (1 + 1) * (mapHeight / _y)));

        thread1.Start();
        thread2.Start();
        thread3.Start();
        thread4.Start();

        thread1.Join();
        thread2.Join();
        thread3.Join();
        thread4.Join();

        map = map_copy.Clone() as int[,];
        isFinishedWork = true;
    }

    IEnumerator TickRoutine()
    {
        TickThreaded();
        yield return null;
    }

    public void GenerateMap(int width, int height)
    {
        mapWidth = width;
        mapHeight = height;

        map = new int[width, height];
        map_copy = new int[width, height];
        RandomFillMap(width, height);
        map_copy = map.Clone() as int[,];
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

    private static void SimulationIterationKernel(int offsetX,
        int offsetY, int _width, int _height)
    {
        for (int x = offsetX; x < _width; x++)
        {
            for (int y = offsetY; y < _height; y++)
            {
                int neighbourLiveCellCount =
                    GetSurroundingLiveCellCount(x, y);

                if (map[x, y] == 0)
                {
                    if (neighbourLiveCellCount == ReproductionThreshold)
                    {
                        map_copy[x, y] = 1;
                    }
                }
                else
                {
                    if (neighbourLiveCellCount < UnderPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount > OverPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount >= LivesThresholdRangeA &&
                        neighbourLiveCellCount <= LivesThresholdRangeB)
                    {
                        map_copy[x, y] = 1;
                    }
                }
            }
        }
    }

    private void SimulationIteration()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int neighbourLiveCellCount =
                    GetSurroundingLiveCellCount(x, y);

                if (map[x, y] == 0)
                {
                    if (neighbourLiveCellCount == ReproductionThreshold)
                    {
                        map_copy[x, y] = 1;
                    }
                }
                else
                {
                    if (neighbourLiveCellCount < UnderPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount > OverPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount >= LivesThresholdRangeA &&
                        neighbourLiveCellCount <= LivesThresholdRangeB)
                    {
                        map_copy[x, y] = 1;
                    }
                }
            }
        }
    }

    private static int GetSurroundingLiveCellCount(int x, int y)
    {
        int liveCellCount = 0;
        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
        {
            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapWidth &&
                    neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (neighbourX != x || neighbourY != y)
                    {
                        // walls are 1, floors are 0
                        liveCellCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    int _y = neighbourY;
                    int _x = neighbourX;
                    if (neighbourX < 0)
                    {
                        _x = (neighbourX % mapWidth + mapWidth) % mapWidth;
                    }
                    else if (neighbourX >= mapWidth)
                    {
                        _x = (neighbourX % mapWidth);
                    }

                    if (neighbourY < 0)
                    {
                        _y = (neighbourY % mapHeight + mapHeight) % mapHeight;
                    }
                    else if (neighbourY >= mapHeight)
                    {
                        _y = (neighbourY % mapHeight);
                    }

                    liveCellCount += map[_x, _y];

                }
            }
        }

        return liveCellCount;
    }

    IEnumerator SimulationSubroutine(
        int offsetX, int offsetY,
        int sectorWidth, int sectorHeight)
    {
        for (int x = offsetX; x < sectorWidth; x++)
        {
            for (int y = offsetY; y < sectorHeight; y++)
            {
                int neighbourLiveCellCount =
                    GetSurroundingLiveCellCount(x, y);

                if (map[x, y] == 0)
                {
                    if (neighbourLiveCellCount == ReproductionThreshold)
                    {
                        map_copy[x, y] = 1;
                    }
                }
                else
                {
                    if (neighbourLiveCellCount < UnderPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount > OverPopThreshold)
                    {
                        map_copy[x, y] = 0;
                    }
                    else if (neighbourLiveCellCount >= LivesThresholdRangeA &&
                        neighbourLiveCellCount <= LivesThresholdRangeB)
                    {
                        map_copy[x, y] = 1;
                    }
                }
            }
        }


        yield return null;
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
                        -mapHeight / 2 + y + 0.5f,
                        0);

                    Gizmos.DrawCube(pos, Vector3.one);

                }
            }
        }
    }
}
