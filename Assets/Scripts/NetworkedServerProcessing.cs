using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static NetworkedServer;

static public class NetworkedServerProcessing
{
    static public class ClientMessageSignifierList
    {
        public const int Login = 0;
        public const int CreateAccount = 1;
        public const int JoinRoom = 2;
        public const int GameUpdate = 3;
        public const int LeaveRoom = 4;
    }
    static public class ServerFeedBackSignifierList
    {
        public const int LoginSuccess = 0;
        public const int LoginFailure = 1;
        public const int CreateAccountSuccess = 2;
        public const int CreateAccountFailure = 3;
        public const int JoinRoomAsPlayer1 = 4;
        public const int JoinRoomAsPlayer2 = 5;
        public const int GameUpdate = 6;
    }

    static public List<TicTacToeGame> gameRooms;

    [SerializeField]
    static Dictionary<string, string> accountList = new Dictionary<string, string>(); // account lists that load at the very beginning
    static Dictionary<int, string> onlinePlayerList = new Dictionary<int, string>(); //Onlice accounts with names
    static Dictionary<int, int> gameRoomIDs = new Dictionary<int, int>(); // dictionary that hold which account in which gameRoom



    #region Setup
    static NetworkedServer networkedServer;

    static public void SetNetworkedServer(NetworkedServer NetworkedServer)
    {
        networkedServer = NetworkedServer;
    }
    static public NetworkedServer GetNetworkedServer()
    {
        return networkedServer;
    }

    #endregion

    static public void ProcessRecievedMsg(string msg, int id)
    {

        Debug.Log("Go message = " + msg);

        string[] msgs = msg.Split(',');
        int signifier = int.Parse(msgs[0]);

        if (signifier == ClientMessageSignifierList.Login)
        {
            /*  Debug.Log("username: " + msgs[1]);
              Debug.Log("Password: " + msgs[2]);*/

            CheckAccounts(msgs[1], msgs[2], id);
        }
        else if (signifier == ClientMessageSignifierList.CreateAccount)
        {
            CreateAccount(msgs[1], msgs[2], id);
        }
        else if (signifier == ClientMessageSignifierList.JoinRoom)
        {
            JoinRoom(msgs[1], id);
        }
        else if (signifier == ClientMessageSignifierList.GameUpdate)
        {
            UpdatePlayers(msgs, id);
        }
        else if (signifier == ClientMessageSignifierList.LeaveRoom)
        {
            LeaveRoom(id);
        }

        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
    }

    static public void SendMessageToClient(string msg, int clientConnectionID)
    {
        networkedServer.SendMessageToClient(msg, clientConnectionID);
    }

    static public void LoadAccountList()
    {
        using (StreamReader sr = new StreamReader("Accounts.txt", true))
        {

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] temp = line.Split(',');
                accountList.Add(temp[0], temp[1]);


            }
        }
    }

    static private void CreateAccount(string userName, string passWord, int id)
    {

        if (!accountList.ContainsKey(userName))
        {
            StreamWriter sw = new StreamWriter("Accounts.txt", true);

            sw.WriteLine(userName + "," + passWord);

            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountSuccess + "," + "Your account Created", id);

            sw.Close();

            accountList.Add(userName, passWord);
        }
        else
        {
            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountFailure + "," + "Account name already taken", id);
        }


    }
    static private void CheckAccounts(string userName, string passWord, int id)
    {


        if (accountList.ContainsKey(userName))//found the account name
        {
            if (accountList[userName] == passWord) // password matchs, login now
            {
                SendMessageToClient(ServerFeedBackSignifierList.LoginSuccess + "," + "Login is success", id);
                onlinePlayerList.Add(id, userName);
                return;
            }
            else // wrong password
            {
                SendMessageToClient(ServerFeedBackSignifierList.LoginFailure + "," + "Wrong password", id);
                return;
            }

            SendMessageToClient(ServerFeedBackSignifierList.LoginFailure + "," + "Wrong username", id);
        }
    }

    static private void JoinRoom(string roomName, int id)
    {

        for (int i = 0; i < gameRooms.Count; i++)
        {

            if (gameRooms[i].RoomName == roomName) // it is already created
            {

                gameRooms[i].Gamer2 = id;
                gameRoomIDs.Add(id, i);
                SendMessageToClient(ServerFeedBackSignifierList.JoinRoomAsPlayer2 + "," + onlinePlayerList[gameRooms[i].Gamer2].ToString() + "," + onlinePlayerList[gameRooms[i].Gamer1] + "," + roomName, gameRooms[i].Gamer2);
                SendMessageToClient(ServerFeedBackSignifierList.JoinRoomAsPlayer2 + "," + onlinePlayerList[gameRooms[i].Gamer1] + "," + onlinePlayerList[gameRooms[i].Gamer2] + "," + roomName, gameRooms[i].Gamer1);
                return;
            }
        }

        TicTacToeGame newRoom = new TicTacToeGame();
        newRoom.RoomName = roomName;
        newRoom.Gamer1 = id;
        gameRooms.Add(newRoom);
        gameRoomIDs.Add(id, gameRooms.Count - 1);

        SendMessageToClient(ServerFeedBackSignifierList.JoinRoomAsPlayer1 + "," + onlinePlayerList[id] + "," + roomName, id);

    }

    static void UpdatePlayers(string[] msgs, int id)
    {
        Debug.Log("Game Updating...");
        TicTacToeGame game = gameRooms[gameRoomIDs[id]];
        int[] gameStatus = game.Play(int.Parse(msgs[1]), id);

        string messages = game.IsGameEnded().ToString();
        foreach (int i in gameStatus)
        {
            messages += "," + i;
        }

        SendMessageToClient(ServerFeedBackSignifierList.GameUpdate + "," + game.Turn + "," + messages, game.Gamer1);//messages has= signifier + turn of player + isGameEnded + Game status array

        SendMessageToClient(ServerFeedBackSignifierList.GameUpdate + "," + (1 - game.Turn) + "," + messages, game.Gamer2);



    }


    static private void LeaveRoom(int id)
    {
        int roomNum = gameRoomIDs[id];
        gameRoomIDs.Remove(id);

        gameRooms[roomNum].LeavingPlayer(id);

        if (gameRooms[roomNum].IsRoomEmpty())
        {

            gameRooms.RemoveAt(roomNum);
        }
    }

    static private void OnApplicationQuit()
    {


    }
}
