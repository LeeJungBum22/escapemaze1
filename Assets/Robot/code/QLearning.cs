using Unity.VisualScripting;
using UnityEngine;

public class QLearning
{
    const int ACTION_COUNT = 4; //가능한 행동의 수(상하좌우)

    public double[,] Q; //Q테이블

    double alpha = 0.1; //학습률
    double gamma = 0.9; //할인률

     //Q테이블을 만드는 함수
    public void makeQ(int states)
    {
        Q = new double[states, ACTION_COUNT];
    }

    //특정 위치의 최대 Q값 반환
    double GetMaxQ(int state)
    {
        double max = Q[state, 0];

        for (int action = 1; action < ACTION_COUNT; action++)
        {
            if (Q[state, action] > max)
            {
                max = Q[state, action];
            }
        }
        return max;
    }

    //특정 위치의 가장 좋은 행동 반환
    int GetBestAction(int state)
    {
        int bestAction = 0;

        for (int action = 1; action < ACTION_COUNT; action++)
        {
            if (Q[state, action] > Q[state, bestAction])
            {
                bestAction = action;
            }
        }
        return bestAction;
    }

    //Q값 학습
    void Learn(int state, int action, int nextState, double reward)
    {
        double maxNextQ = GetMaxQ(nextState);

        Q[state, action] += alpha * (reward + gamma * maxNextQ - Q[state, action]); //이 함수는 자료에 있는 걸 그대로 사용.
    }

    //e-greedy 방식 행동 선택
    int ChooseAction(int state, double epsilon)
    {
        if (Random.value < epsilon)
        {
            return Random.Range(0, 4); //랜덤 선택
        }
        return GetBestAction(state); //Q값 기준 선택
    }

    //즉각적인 보상 반환
    int getReward(int[,] maze, int x, int y)
    {
        if (maze[y, x] == 1) //벽
        {
            return -10;
        }
        else if (maze[y, x] == 4) //목적지
        {
            return 100;
        }
        else //이동 비용
        {
            return -1;
        }
    }

    //이동 함수
    public int Step(int [,] maze, int state)
    {
        int w = maze.GetLength(1); //너비
        int h = maze.GetLength(0); //높이
        int x = state % w; //x좌표
        int y = state / w; //y좌표
        int action = ChooseAction(state, 0.2); //행동 선택, 0.2는 엡실론 값
        int nextState = 0; //다음 위치
        int reward = 0; //즉각적인 보상

        //각 행동에 따른 위치 조정 및 즉각적인 보상 계산
        if (action == 0)
        {
            if (y-1 < 0) //맵 밖으로 나가는 거 방지
            {
                reward = -10;
                Q[state, 0] = -10;
                return state;
            }
            nextState = w * (y - 1) + x;
            reward = getReward(maze, x, y - 1);
        }
        else if (action == 1)
        {
            if (y+1 >= h) //맵 밖으로 나가는 거 방지
            {
                reward = -10;
                Q[state, 1] = -10;
                return state;
            }
            nextState = w * (y + 1) + x;
            reward = getReward(maze, x, y + 1);
        }
        else if (action == 2)
        {
            if (x - 1 < 0) //맵 밖으로 나가는 거 방지
            {
                reward = -10;
                Q[state, 2] = -10;
                return state;
            }
            nextState = state - 1;
            reward = getReward(maze, x - 1, y);
        }
        else if (action == 3)
        {
            if (x + 1 >= w) //맵 밖으로 나가는 거 방지
            {
                reward = -10;
                Q[state, 3] = -10;
                return state;
            }
            nextState = state + 1;
            reward = getReward(maze, x + 1, y);
        }

        Learn(state, action, nextState, reward); //q테이블 학습

        if (reward == -10) //벽에 부딪히면 출발지로
        {
            nextState = 0;
        }
        if(reward == 100) //목적지에 도착하면 출발지로
        {
            nextState = 0;
        }
        return nextState; //다음 위치를 반환.
    }

    //테스트용 출력 함수.
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
