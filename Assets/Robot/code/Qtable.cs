using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//테스트용(어떤 식으로 쓰면 되는지 보여주는 용도입니다.)
public class Qtable : MonoBehaviour
{
    private QLearning q;
    int[,] maze = { { 3, 0, 0, 0 }, { 0, 1, 0, 1 }, { 0, 0, 0, 1 }, { 1, 0, 0, 0 }, { 1, 0, 0, 4 } };
    int state = 0;

    private void Start()
    {
        q = new QLearning();
        q.makeQ(20/*=미로의 칸 수*/);
        //원하는 학습 횟수만큼 반복
        for (int i = 0; i < 200; i++)
        {
            do
            {
                state = q.Step(maze, state); //이때 state는 다음 위치를 반환받게 됨.
            } while (state != 0); //만약 벽에 부딪히거나 목적지에 도착하여 출발지로 돌아왔다면, 학습 횟수 1회 소진한 것으로 간주.
        }
        q.PrintQ(); //테스트용 출력
    }
}
