using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    static public class ClientMessageSignifierList
    {
        public const int Login = 0;
        public const int CreateAccount = 1;
        public const int JoinRoom = 2;
    }
    static public class ServerFeedBackSignifierList
    {
        public const int LoginSuccess = 0;
        public const int LoginFailure = 1;
        public const int CreateAccountSuccess = 2;
        public const int CreateAccountFailure = 3;

    }

    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;



    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

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
     
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
    }
    private void CreateAccount(string userName, string passWord, int id)
    {
        bool isAccountNameTaken = false;
        using (StreamReader sr = new StreamReader("Accounts.txt", true))
        {

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                if (string.Equals(data[0],userName)) //account name already taken?
                {
                    isAccountNameTaken = true;
                }
            }
        }

        if(!isAccountNameTaken)
        {
            StreamWriter sw = new StreamWriter("Accounts.txt", true);

            sw.WriteLine(userName + "," +passWord);

            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountSuccess + "," + "Your account Created", id);

            sw.Close();
        }
        else
        {
            SendMessageToClient(ServerFeedBackSignifierList.CreateAccountFailure + "," + "Account name already taken", id);
        }
        

    }
    private void CheckAccounts(string userName, string passWord, int id)
    {
        using (StreamReader sr = new StreamReader("Accounts.txt", true))
        {

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                
                if(data[0] == userName) //found the account name
                {
                    if(string.Equals(data[1], passWord)) // password matchs, login now
                    {
                        SendMessageToClient(ServerFeedBackSignifierList.LoginSuccess + "," + "Login is success", id);
                    }
                    else // wrong password
                    {
                        SendMessageToClient(ServerFeedBackSignifierList.LoginFailure + "," + "Wrong password", id);
                    }
                }
                else
                {
                    SendMessageToClient(ServerFeedBackSignifierList.LoginFailure + "," + "Wrong username", id);
                }
            }


        }
    }
    
    private void JoinRoom(string roomName, int id)
    {
      
    }
}