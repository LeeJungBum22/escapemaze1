using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 8개의 알고리즘 목록

public class Pathfinding : MonoBehaviour
{
    [Header("Algorithm Settings")]
    public AlgorithmType selectedAlgorithm = AlgorithmType.AStar;
    public bool allowDiagonal = true;

    [Header("Visualization Settings")]
    public GameObject searchMarkerPrefab;
    public float searchDelay = 0.02f;
    
    private float tileSize;
    private Vector2 offset;
    private List<GameObject> visualMarkers = new List<GameObject>();

    // IDA* 전용 상태 저장 클래스
    private class IDAState
    {
        public Node node;
        public int gCost;
        public List<Node> neighbors;
        public int neighborIndex;
    }

    public void StartVisualSearch(int[,] maze, Vector2Int startPos, Vector2Int targetPos, float tSize, Vector2 off, Action<List<Node>> onComplete)
    {
        tileSize = tSize;
        offset = off;
        ClearMarkers(); // 🌟 수정됨
        
        foreach (var marker in visualMarkers) if(marker != null) Destroy(marker);
        visualMarkers.Clear();

        switch (selectedAlgorithm)
        {
            case AlgorithmType.AStar:
            case AlgorithmType.Dijkstra:
            case AlgorithmType.BreadthFirstSearch:
            case AlgorithmType.BestFirstSearch:
                StartCoroutine(StandardSearchCoroutine(maze, startPos, targetPos, onComplete));
                break;
            case AlgorithmType.IDAStar:
                StartCoroutine(IDAStarCoroutine(maze, startPos, targetPos, onComplete));
                break;
            case AlgorithmType.JumpPointSearch:
            case AlgorithmType.OrthogonalJumpPointSearch:
                StartCoroutine(JPSCoroutine(maze, startPos, targetPos, onComplete));
                break;
            case AlgorithmType.Trace:
                StartCoroutine(TraceCoroutine(maze, startPos, targetPos, onComplete));
                break;
        }
    }

    public void ClearMarkers()
    {
        foreach (var marker in visualMarkers) if(marker != null) Destroy(marker);
        visualMarkers.Clear();
    }
    // ==========================================
    // [1] A*, BFS, Dijkstra, Best-First 통합 탐색기
    // ==========================================
    IEnumerator StandardSearchCoroutine(int[,] maze, Vector2Int startPos, Vector2Int targetPos, Action<List<Node>> onComplete)
    {
        int sizeY = maze.GetLength(0);
        int sizeX = maze.GetLength(1);
        Node[,] nodes = CreateNodes(maze);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Queue<Node> bfsQueue = new Queue<Node>();

        Node startNode = nodes[startPos.y, startPos.x];
        Node targetNode = nodes[targetPos.y, targetPos.x];

        if (selectedAlgorithm == AlgorithmType.BreadthFirstSearch) bfsQueue.Enqueue(startNode);
        else openList.Add(startNode);

        bool pathFound = false;

        while ((selectedAlgorithm == AlgorithmType.BreadthFirstSearch ? bfsQueue.Count > 0 : openList.Count > 0))
        {
            Node currentNode;

            if (selectedAlgorithm == AlgorithmType.BreadthFirstSearch) currentNode = bfsQueue.Dequeue();
            else
            {
                currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                        currentNode = openList[i];
                }
                openList.Remove(currentNode);
            }

            closedList.Add(currentNode);

            if (currentNode != startNode && currentNode != targetNode)
            {
                CreateVisualMarker(currentNode);
                yield return new WaitForSeconds(searchDelay);
            }

            if (currentNode == targetNode) { pathFound = true; break; }

            foreach (Node neighbor in GetNeighbors(currentNode, nodes, sizeX, sizeY))
            {
                if (!neighbor.walkable || closedList.Contains(neighbor)) continue;

                if (selectedAlgorithm == AlgorithmType.BreadthFirstSearch)
                {
                    if (!bfsQueue.Contains(neighbor))
                    {
                        neighbor.parent = currentNode;
                        bfsQueue.Enqueue(neighbor);
                    }
                }
                else
                {
                    int moveCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (selectedAlgorithm == AlgorithmType.BestFirstSearch) moveCost = 0; 

                    if (moveCost < neighbor.gCost || !openList.Contains(neighbor))
                    {
                        neighbor.gCost = moveCost;
                        neighbor.parent = currentNode;

                        if (selectedAlgorithm == AlgorithmType.AStar || selectedAlgorithm == AlgorithmType.BestFirstSearch)
                            neighbor.hCost = GetDistance(neighbor, targetNode);
                        else if (selectedAlgorithm == AlgorithmType.Dijkstra) neighbor.hCost = 0;

                        if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    }
                }
            }
        }
        FinalizeSearch(pathFound, startNode, targetNode, onComplete);
    }

    // ==========================================
    // [2] Trace 알고리즘 (단순 직진 추적)
    // ==========================================
    IEnumerator TraceCoroutine(int[,] maze, Vector2Int startPos, Vector2Int targetPos, Action<List<Node>> onComplete)
    {
        Node[,] nodes = CreateNodes(maze);
        Node currentNode = nodes[startPos.y, startPos.x];
        Node targetNode = nodes[targetPos.y, targetPos.x];
        HashSet<Node> visited = new HashSet<Node>();
        bool pathFound = false;

        visited.Add(currentNode);

        while (currentNode != targetNode)
        {
            Node bestNeighbor = null;
            int minH = int.MaxValue;

            foreach (Node neighbor in GetNeighbors(currentNode, nodes, maze.GetLength(1), maze.GetLength(0)))
            {
                if (!neighbor.walkable || visited.Contains(neighbor)) continue;
                
                int h = GetDistance(neighbor, targetNode);
                if (h < minH)
                {
                    minH = h;
                    bestNeighbor = neighbor;
                }
            }

            if (bestNeighbor == null) break;

            bestNeighbor.parent = currentNode;
            currentNode = bestNeighbor;
            visited.Add(currentNode);

            if (currentNode != nodes[startPos.y, startPos.x] && currentNode != targetNode)
            {
                CreateVisualMarker(currentNode);
                yield return new WaitForSeconds(searchDelay);
            }

            if (currentNode == targetNode) pathFound = true;
        }

        FinalizeSearch(pathFound, nodes[startPos.y, startPos.x], targetNode, onComplete);
    }

    // ==========================================
    // [3] IDA* (반복적 깊이 탐색 A*) - 버그 수정판
    // ==========================================
    IEnumerator IDAStarCoroutine(int[,] maze, Vector2Int startPos, Vector2Int targetPos, Action<List<Node>> onComplete)
    {
        Node[,] nodes = CreateNodes(maze);
        Node startNode = nodes[startPos.y, startPos.x];
        Node targetNode = nodes[targetPos.y, targetPos.x];

        int bound = GetDistance(startNode, targetNode);
        bool pathFound = false;

        while (!pathFound && bound < 1000)
        {
            // 한계치 증가 시 시각적 마커 초기화 연출
            foreach (var marker in visualMarkers) if(marker != null) Destroy(marker);
            visualMarkers.Clear();
            yield return new WaitForSeconds(0.2f); 

            int minOverBound = int.MaxValue;
            Stack<IDAState> stack = new Stack<IDAState>();
            HashSet<Node> currentPath = new HashSet<Node>();

            stack.Push(new IDAState {
                node = startNode,
                gCost = 0,
                neighbors = GetNeighbors(startNode, nodes, maze.GetLength(1), maze.GetLength(0)),
                neighborIndex = 0
            });
            currentPath.Add(startNode);

            while (stack.Count > 0)
            {
                IDAState current = stack.Peek();
                int f = current.gCost + GetDistance(current.node, targetNode);

                if (f > bound)
                {
                    minOverBound = Mathf.Min(minOverBound, f);
                    currentPath.Remove(current.node);
                    stack.Pop();
                    continue;
                }

                if (current.node == targetNode) { pathFound = true; break; }

                Node nextNeighbor = null;
                while (current.neighborIndex < current.neighbors.Count)
                {
                    Node n = current.neighbors[current.neighborIndex];
                    current.neighborIndex++;
                    if (n.walkable && !currentPath.Contains(n))
                    {
                        nextNeighbor = n;
                        break;
                    }
                }

                if (nextNeighbor != null)
                {
                    nextNeighbor.parent = current.node;
                    stack.Push(new IDAState {
                        node = nextNeighbor,
                        gCost = current.gCost + GetDistance(current.node, nextNeighbor),
                        neighbors = GetNeighbors(nextNeighbor, nodes, maze.GetLength(1), maze.GetLength(0)),
                        neighborIndex = 0
                    });
                    currentPath.Add(nextNeighbor);

                    if (nextNeighbor != startNode && nextNeighbor != targetNode)
                    {
                        CreateVisualMarker(nextNeighbor);
                        yield return new WaitForSeconds(searchDelay); 
                    }
                }
                else
                {
                    currentPath.Remove(current.node);
                    stack.Pop();
                }
            }

            if (pathFound) break;
            if (minOverBound == int.MaxValue) break;
            
            bound = minOverBound;
        }

        FinalizeSearch(pathFound, startNode, targetNode, onComplete);
    }

    // ==========================================
    // [4] JPS & OJPS (Jump Point Search)
    // ==========================================
    IEnumerator JPSCoroutine(int[,] maze, Vector2Int startPos, Vector2Int targetPos, Action<List<Node>> onComplete)
    {
        bool isOrthogonal = (selectedAlgorithm == AlgorithmType.OrthogonalJumpPointSearch);
        
        Node[,] nodes = CreateNodes(maze);
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = nodes[startPos.y, startPos.x];
        Node targetNode = nodes[targetPos.y, targetPos.x];

        openList.Add(startNode);
        bool pathFound = false;

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                    currentNode = openList[i];
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode != startNode && currentNode != targetNode)
            {
                CreateVisualMarker(currentNode);
                yield return new WaitForSeconds(searchDelay);
            }

            if (currentNode == targetNode) { pathFound = true; break; }

            int[] dirX = { 0, 1, 0, -1, 1, 1, -1, -1 };
            int[] dirY = { -1, 0, 1, 0, -1, 1, 1, -1 };
            int maxDirs = isOrthogonal ? 4 : 8;

            for (int i = 0; i < maxDirs; i++)
            {
                Node jumpNode = Jump(currentNode.gridX, currentNode.gridY, dirX[i], dirY[i], maze, nodes, targetNode, isOrthogonal);
                if (jumpNode != null && !closedList.Contains(jumpNode))
                {
                    int cost = currentNode.gCost + GetDistance(currentNode, jumpNode);
                    if (cost < jumpNode.gCost || !openList.Contains(jumpNode))
                    {
                        jumpNode.gCost = cost;
                        jumpNode.hCost = GetDistance(jumpNode, targetNode);
                        jumpNode.parent = currentNode;
                        if (!openList.Contains(jumpNode)) openList.Add(jumpNode);
                        
                        CreateVisualMarker(jumpNode);
                        yield return new WaitForSeconds(searchDelay);
                    }
                }
            }
        }
        FinalizeSearch(pathFound, startNode, targetNode, onComplete);
    }

    Node Jump(int x, int y, int dx, int dy, int[,] maze, Node[,] nodes, Node target, bool isOrthogonal)
    {
        int nx = x + dx;
        int ny = y + dy;
        int sizeY = maze.GetLength(0);
        int sizeX = maze.GetLength(1);

        if (nx < 0 || nx >= sizeX || ny < 0 || ny >= sizeY || maze[ny, nx] == 1) return null;
        Node nextNode = nodes[ny, nx];

        if (nextNode == target) return nextNode;

        if (dx != 0 && dy != 0 && !isOrthogonal) 
        {
            if ((maze[y, x + dx] == 1 && maze[ny, nx] != 1) || (maze[y + dy, x] == 1 && maze[ny, nx] != 1))
                return nextNode;
            if (Jump(nx, ny, dx, 0, maze, nodes, target, isOrthogonal) != null || Jump(nx, ny, 0, dy, maze, nodes, target, isOrthogonal) != null)
                return nextNode;
        }
        else 
        {
            if (dx != 0) 
            {
                if ((ny + 1 < sizeY && maze[ny + 1, x] == 1 && maze[ny + 1, nx] != 1) ||
                    (ny - 1 >= 0 && maze[ny - 1, x] == 1 && maze[ny - 1, nx] != 1)) return nextNode;
            }
            else if (dy != 0) 
            {
                if ((nx + 1 < sizeX && maze[y, nx + 1] == 1 && maze[ny, nx + 1] != 1) ||
                    (nx - 1 >= 0 && maze[y, nx - 1] == 1 && maze[ny, nx - 1] != 1)) return nextNode;
            }
        }
        return Jump(nx, ny, dx, dy, maze, nodes, target, isOrthogonal);
    }

    // ==========================================
    // 공통 보조 함수들
    // ==========================================
    Node[,] CreateNodes(int[,] maze)
    {
        int sizeY = maze.GetLength(0), sizeX = maze.GetLength(1);
        Node[,] nodes = new Node[sizeY, sizeX];
        for (int y = 0; y < sizeY; y++)
            for (int x = 0; x < sizeX; x++)
                nodes[y, x] = new Node(maze[y, x] != 1, x, y);
        return nodes;
    }

    void FinalizeSearch(bool pathFound, Node startNode, Node targetNode, Action<List<Node>> onComplete)
    {
        if (pathFound) onComplete?.Invoke(GetFinalPath(startNode, targetNode));
        else { Debug.LogWarning("경로를 찾을 수 없습니다! (알고리즘 특성상 막혔을 수 있습니다)"); onComplete?.Invoke(null); }
    }

    List<Node> GetNeighbors(Node node, Node[,] nodes, int sizeX, int sizeY)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (!allowDiagonal && (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)) continue;
                int cx = node.gridX + x, cy = node.gridY + y;
                if (cx >= 0 && cx < sizeX && cy >= 0 && cy < sizeY) neighbors.Add(nodes[cy, cx]);
            }
        return neighbors;
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        if (allowDiagonal) return dstX > dstY ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
        else return 10 * (dstX + dstY);
    }

    List<Node> GetFinalPath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    // 🌟 핵심 수정: 마커 생성 시 월드 좌표 보정
    void CreateVisualMarker(Node node)
    {
        float targetX = (node.gridX * tileSize) - offset.x;
        float targetY = -(node.gridY * tileSize) + offset.y;
        
        // 🌟 수정: 현재 스페이스 오브젝트의 위치(transform.position)를 더해줍니다.
        Vector3 pos = transform.position + new Vector3(targetX, targetY, 0);

        GameObject marker = Instantiate(searchMarkerPrefab, pos, Quaternion.identity);
        
        // 🌟 추가: 마커를 현재 오브젝트의 자식으로 설정 (관리가 편해집니다)
        marker.transform.parent = this.transform;
        
        marker.transform.localScale = Vector3.one * (tileSize * 0.9f);
        visualMarkers.Add(marker);
    }
}