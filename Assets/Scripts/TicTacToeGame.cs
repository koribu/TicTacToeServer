using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeGame : MonoBehaviour
{
    string roomName;
   
    int gamer1, gamer2;
    int[] gameSpots = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    bool turn = false; // false for gamer1, true for gamer2


    public string RoomName { get => roomName; set => roomName = value; }
    public int Gamer1 { get => gamer1; set => gamer1 = value; }
    public int Gamer2 { get => gamer2; set => gamer2 = value; }
    public bool Turn { get => turn; set => turn = !turn; }
    // Start is called before the first frame update

    public int[] Play(int spot, int gamer)
    {
        if(turn) // gamer1's turn to player
            gameSpots[spot] = 0;
        else// gamer2's turn to player
            gameSpots[spot] = 1;

        turn = !turn;

        return gameSpots;
    }


    public int IsGameEnded() //0 for game not ended, 1 for game ended
    {
        if ((gameSpots[0] == 0 && gameSpots[0] == gameSpots[1] && gameSpots[1] == gameSpots[2]) ||
            (gameSpots[0] == 0 && gameSpots[0] == gameSpots[3] && gameSpots[4] == gameSpots[6]) ||
            (gameSpots[0] == 2 && gameSpots[2] == gameSpots[5] && gameSpots[5] == gameSpots[8]) ||
            (gameSpots[0] == 6 && gameSpots[6] == gameSpots[7] && gameSpots[7] == gameSpots[8]) ||
            (gameSpots[0] == 0 && gameSpots[0] == gameSpots[4] && gameSpots[4] == gameSpots[8]) ||
            (gameSpots[0] == 2 && gameSpots[2] == gameSpots[4] && gameSpots[4] == gameSpots[6]))
        {
            return 1;
        }
        return 0;
    }

}
