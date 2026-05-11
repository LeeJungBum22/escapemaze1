using UnityEngine;
using UnityEngine.Tilemaps;

public class Make : MonoBehaviour
{
    public GameObject floorprefab;
    public GameObject spikeprefab;
    public int sizex = 17;
    public int sizey = 9;

    int[,] InitBoard(int[,] maze)
    {
        for(int y = 0; y< sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                if (x%2 == 0 || y%2 == 0)
                    maze[y, x] = 1;
                else
                    maze[y, x] = 0;
            }
        }
        return maze;
    }
    int[,] GeneratedByBinaryTree(int[,] maze)
    {
        for (int y = 0; y <sizey; y++)
        {
            for(int x = 0;x < sizex; x++)
            {
                if (x%2 == 0 || y % 2 == 0)
                    continue;

                if (x == sizex - 2 && y == sizey - 2)
                    continue;

                if (x == sizex - 2)
                {
                    maze[y + 1, x] = 0;
                    continue;
                }

                if (y == sizey - 2)
                {
                    maze[y, x + 1] = 0;
                    continue;
                }

                else
                {
                    int rand = Random.Range(0, 2);
                    switch(rand)
                    {
                        case 0:
                            maze[y + 1, x] = 0;
                            break;
                        case 1:
                            maze[y, x + 1] = 0;
                            break;
                    }
                }
            }
        }
        return maze;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int[,] maze = new int[sizey, sizex];
        maze = InitBoard(maze);
        maze = GeneratedByBinaryTree(maze);

        for (int y1 = 0; y1 < sizey; y1++)
        {
            for(int x1 = 0; x1 < sizex; x1++)
            {
                float x = x1 - 8.5f;
                float y = -y1 + 4.5f;
                if (maze[y1,x1] == 0)
                {
                    GameObject f = Instantiate(floorprefab);
                    f.transform.position = new Vector2(x, y);
                }
                else
                {
                    GameObject s = Instantiate(spikeprefab);
                    s.transform.position = new Vector2(x, y);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
