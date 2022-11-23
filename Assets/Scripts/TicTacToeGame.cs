using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeGame : MonoBehaviour
{
    static public class GamersTurnSignifier
    {
        public const int Gamer2sTurn = 0;
        public const int Gamer1sTurn = 1;
       
    }

    static public class IsSpotEmptySignifier
    {
        public const int SpotIsEmpty = 0;
        public const int SpotIsTaken = 1;

    }

    static public class TicTacToeMoveSignifier
    {
        public const int EmptySpot = 0;
        public const int X = 1;
        public const int O = 2;
    }

    string roomName;
   
    int gamer1 = 0, gamer2 = 0;
    int[] gameSpots = { 0, 0, 0, 0, 0, 0, 0, 0, 0};

    int turn = 1; // 1 for gamer1, 0 for gamer2


    public string RoomName { get => roomName; set => roomName = value; }
    public int Gamer1 { get => gamer1; set => gamer1 = value; }
    public int Gamer2 { get => gamer2; set => gamer2 = value; }
    public int Turn { get => turn; }
    // Start is called before the first frame update

    public int[] Play(int spot, int gamer)
    {
        if(turn == GamersTurnSignifier.Gamer1sTurn) // gamer1's turn to player
        {
            gameSpots[spot] = TicTacToeMoveSignifier.X;
            turn = GamersTurnSignifier.Gamer2sTurn;
        }
            
        else if(turn == GamersTurnSignifier.Gamer2sTurn)// gamer2's turn to player
        {
            gameSpots[spot] = TicTacToeMoveSignifier.O;
            turn = GamersTurnSignifier.Gamer1sTurn;
        }
            
        return gameSpots;
    }

    public void LeavingPlayer(int playerID)
    {
        if(gamer1== playerID)
        {
            gamer1 = IsSpotEmptySignifier.SpotIsEmpty;
        }
        else if (gamer2 == playerID)
        {
            gamer2 = IsSpotEmptySignifier.SpotIsEmpty;
        }
    }

    public bool IsRoomEmpty()
    {
        if(gamer1 == IsSpotEmptySignifier.SpotIsEmpty && gamer2 == IsSpotEmptySignifier.SpotIsEmpty)
        {
            return true;
        }
        return false;
    }
    public int IsGameEnded() //0 for game not ended, 1 for game ended
    {
        if ((gameSpots[0] != 0 && gameSpots[0] == gameSpots[1] && gameSpots[1] == gameSpots[2]) ||
            (gameSpots[0] != 0 && gameSpots[0] == gameSpots[3] && gameSpots[4] == gameSpots[6]) ||
            (gameSpots[2] != 0 && gameSpots[2] == gameSpots[5] && gameSpots[5] == gameSpots[8]) ||
            (gameSpots[6] != 0 && gameSpots[6] == gameSpots[7] && gameSpots[7] == gameSpots[8]) ||
            (gameSpots[0] != 0 && gameSpots[0] == gameSpots[4] && gameSpots[4] == gameSpots[8]) ||
            (gameSpots[2] != 0 && gameSpots[2] == gameSpots[4] && gameSpots[4] == gameSpots[6]))
        {
            return 1;
        }
        return 0;
    }

}
