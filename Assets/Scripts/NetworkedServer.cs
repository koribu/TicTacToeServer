using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using System.Numerics;
using System.Linq;

public class NetworkedServer : MonoBehaviour
{
    static public class ClientMessageSignifierList
    {
        public const int Login = 0;
        public const int CreateAccount = 1;
        public const int JoinRoom = 2;
        public const int GameUpdate = 3;
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

   

    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    public List<TicTacToeGame> gameRooms;

    [SerializeField]
    Dictionary<string,string> accountList = new Dictionary<string, string>();
    Dictionary<int, string> onlinePlayerList = new Dictionary<int, string>();
    Dictionary<int, int> gameRoomIDs = new Dictionary<int, int>();


    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        LoadAccountList();

    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
               
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }

    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {

        Debug.Log("Go message = " + msg);

        string[] msgs = msg.Split(',');
        int signifier = int.Parse(msgs[0]);

        if(signifier == ClientMessageSignifierList.Login)
        {
          /*  Debug.Log("username: " + msgs[1]);
            Debug.Log("Password: " + msgs[2]);*/

            CheckAccounts(msgs[1], msgs[2], id);
        }
        else if(signifier == ClientMessageSignifierList.CreateAccount)
        {
            CreateAccount(msgs[1], msgs[2],  id);
        }
        else if (signifier == ClientMessageSignifierList.JoinRoom)
        {
            JoinRoom(msgs[1], id);
        }
        else if (signifier == ClientMessageSignifierList.GameUpdate)
        {
            UpdatePlayers(msgs, id);
        }

        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
    }
    private void LoadAccountList()
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

    private void CreateAccount(string userName, string passWord, int id)
    {
       
        if (!accountList.ContainsKey(userName))
        {
            StreamWriter sw = new StreamWriter("Accounts.txt", true);

            sw.WriteLine(userName + "," +passWord);

            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountSuccess + "," + "Your account Created", id);

            sw.Close();

            accountList.Add(userName, passWord);
        }
        else
        {
            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountFailure + "," + "Account name already taken", id);
        }
        

    }
    private void CheckAccounts(string userName, string passWord, int id)
    {
        
                
        if(accountList.ContainsKey(userName))//found the account name
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
    
    private void JoinRoom(string roomName, int id)
    {

        for (int i = 0; i <gameRooms.Count; i++)
        {
            
            if (gameRooms[i].RoomName == roomName) // it is already created
            {

                gameRooms[i].Gamer2 = id;
                gameRoomIDs.Add(id, i);
                SendMessageToClient(ServerFeedBackSignifierList.JoinRoomAsPlayer2 + "," + onlinePlayerList[gameRooms[i].Gamer2].ToString() + "," + onlinePlayerList[gameRooms[i].Gamer1] + "," + roomName, gameRooms[i].Gamer2);
                SendMessageToClient(ServerFeedBackSignifierList.JoinRoomAsPlayer2 + "," + onlinePlayerList[gameRooms[i].Gamer1] + "," + onlinePlayerList[gameRooms[i].Gamer2] +  "," + roomName, gameRooms[i].Gamer1);
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

    void UpdatePlayers(string[] msgs, int id)
    {
        Debug.Log("Game Updating...");
        TicTacToeGame game = gameRooms[gameRoomIDs[id]];
        int[] gameStatus = game.Play(int.Parse(msgs[1]),id);

        string messages = game.IsGameEnded().ToString();
        foreach(int i in gameStatus)
        {
            messages += "," + i;
        }

        SendMessageToClient(ServerFeedBackSignifierList.GameUpdate + "," + game.Turn + "," + messages,game.Gamer1);//messages has= signifier + turn of player + isGameEnded + Game status array

        SendMessageToClient(ServerFeedBackSignifierList.GameUpdate + "," + (1-game.Turn) + "," + messages, game.Gamer2);



    }

    
    private void LeaveRoom()
    {

    }

    private void OnApplicationQuit()
    {
        
    }
}