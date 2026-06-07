using System.Collections.Generic;
using UnityEngine;

public class QLearning
{
    const int ACTION_COUNT = 4; // 상(0) 하(1) 좌(2) 우(3)

    public double[,] Q; // Q테이블
    public int mazeWidth;
    public int mazeHeight;

    double alpha = 0.1;  // 학습률
    double gamma = 0.9;  // 할인률

    // Q테이블 초기화
    public void MakeQ(int[,] maze)
    {
        mazeHeight = maze.GetLength(0);
        mazeWidth = maze.GetLength(1);
        Q = new double[mazeHeight * mazeWidth, ACTION_COUNT];
    }

    // 특정 state의 최대 Q값
    double GetMaxQ(int state)
    {
        double max = Q[state, 0];
        for (int a = 1; a < ACTION_COUNT; a++)
        {
            if (Q[state, a] > max) max = Q[state, a];
        }
        return max;
    }

    // 특정 state의 최적 행동
    int GetBestAction(int state)
    {
        int best = 0;
        for (int a = 1; a < ACTION_COUNT; a++)
        {
            if (Q[state, a] > Q[state, best]) best = a;
        }
        return best;
    }

    // Q값 업데이트 (벨만 방정식)
    void Learn(int state, int action, int nextState, double reward)
    {
        double maxNextQ = GetMaxQ(nextState);
        Q[state, action] += alpha * (reward + gamma * maxNextQ - Q[state, action]);
    }

    // ε-greedy 행동 선택
    int ChooseAction(int state, double epsilon)
    {
        if (Random.value < epsilon)
            return Random.Range(0, ACTION_COUNT);
        return GetBestAction(state);
    }

    // 좌표로 state 인덱스 계산
    int ToState(int x, int y) => y * mazeWidth + x;

    // state에서 좌표 추출
    void ToXY(int state, out int x, out int y)
    {
        x = state % mazeWidth;
        y = state / mazeWidth;
    }

    // 행동에 따른 다음 좌표 계산 (경계 클램핑)
    void GetNextXY(int x, int y, int action, out int nx, out int ny)
    {
        nx = x; ny = y;
        switch (action)
        {
            case 0: ny = Mathf.Max(0, y - 1); break;       // 상
            case 1: ny = Mathf.Min(mazeHeight - 1, y + 1); break; // 하
            case 2: nx = Mathf.Max(0, x - 1); break;       // 좌
            case 3: nx = Mathf.Min(mazeWidth - 1, x + 1); break;  // 우
        }
    }

    // 보상 함수
    double GetReward(int[,] maze, int x, int y, int prevX, int prevY)
    {
        if (maze[y, x] == 1) return -10;  // 벽
        if (maze[y, x] == 4) return 100;  // 목적지
        if (x == prevX && y == prevY) return -5; // 경계 밖 시도 (제자리)
        return -1; // 일반 이동
    }

    // ★ 한 에피소드 학습 (시작~목적지 도달 또는 최대 스텝)
    public bool TrainEpisode(int[,] maze, int startState, int goalState, double epsilon, int maxSteps = 500)
    {
        int state = startState;

        for (int step = 0; step < maxSteps; step++)
        {
            ToXY(state, out int x, out int y);
            int action = ChooseAction(state, epsilon);

            GetNextXY(x, y, action, out int nx, out int ny);
            int nextState = ToState(nx, ny);

            // 벽이면 제자리 유지
            if (maze[ny, nx] == 1)
            {
                nextState = state;
            }

            double reward = GetReward(maze, nx, ny, x, y);
            Learn(state, action, nextState, reward);

            // 목적지 도달 시 에피소드 종료
            if (maze[ny, nx] == 4)
                return true;

            state = nextState;
        }
        return false; // 최대 스텝 초과
    }

    // ★ N 에피소드 일괄 학습
    public void Train(int[,] maze, int startState, int goalState, int episodes = 500)
    {
        MakeQ(maze);

        for (int ep = 0; ep < episodes; ep++)
        {
            // 초반엔 탐험 많이, 후반엔 활용 위주
            double epsilon = Mathf.Max(0.01f, 1.0f - (float)ep / episodes);
            TrainEpisode(maze, startState, goalState, epsilon);
        }
    }

    // ★ 누적 학습 (기존 Q테이블을 초기화하지 않고 이어서 학습)
    //    Q가 비어있으면 새로 만들고, 있으면 그 위에 추가 학습.
    public void TrainIncremental(int[,] maze, int startState, int goalState, int episodes, double epsilon)
    {
        if (Q == null) MakeQ(maze);

        for (int ep = 0; ep < episodes; ep++)
        {
            TrainEpisode(maze, startState, goalState, epsilon);
        }
    }

    // ★ 학습된 Q테이블에서 최적 경로 추출 → Node 리스트로 반환
    public List<Node> GetPath(int[,] maze, int startState, int goalState, int maxSteps = 300)
    {
        List<Node> path = new List<Node>();
        HashSet<int> visited = new HashSet<int>();
        int state = startState;

        for (int step = 0; step < maxSteps; step++)
        {
            ToXY(state, out int x, out int y);
            path.Add(new Node(true, x, y));

            if (state == goalState) break;

            // 루프 방지
            if (visited.Contains(state))
            {
                Debug.LogWarning("[QLearning] 경로 루프 감지! 학습이 부족할 수 있습니다.");
                break;
            }
            visited.Add(state);

            int action = GetBestAction(state);
            GetNextXY(x, y, action, out int nx, out int ny);
            int nextState = ToState(nx, ny);

            // 벽이면 경로 종료 (비정상)
            if (maze[ny, nx] == 1)
            {
                Debug.LogWarning("[QLearning] 최적 경로가 벽으로 향함! 학습이 부족합니다.");
                break;
            }

            state = nextState;
        }

        return path;
    }

    // Q테이블을 직렬화용 1차원 배열로 변환
    public double[] SerializeQ()
    {
        int rows = Q.GetLength(0);
        int cols = Q.GetLength(1);
        double[] flat = new double[rows * cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                flat[r * cols + c] = Q[r, c];
        return flat;
    }

    // 1차원 배열에서 Q테이블 복원
    public void DeserializeQ(double[] flat, int states)
    {
        Q = new double[states, ACTION_COUNT];
        for (int r = 0; r < states; r++)
            for (int c = 0; c < ACTION_COUNT; c++)
                Q[r, c] = flat[r * ACTION_COUNT + c];
    }

    // 디버그 출력
    public void PrintQ()
    {
        string result = "";
        for (int row = 0; row < Q.GetLength(0); row++)
        {
            for (int col = 0; col < Q.GetLength(1); col++)
            {
                result += $"{Q[row, col]:F2}\t";
            }
            result += "\n";
        }
        Debug.Log(result);
    }
}