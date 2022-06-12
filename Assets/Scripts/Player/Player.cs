using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id { get; private set; }
    public string username { get; private set; }


    private Player player;


    private void OnDestroy()
    {
        list.Remove(id);
    }
    public static void Spawn(ushort id, string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            username = "Guest";
        }

        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.SendSpawned(id);
        }

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>(); 
        player.name = $"Player {id} ({username})";
        player.id = id;
        player.username = username;

        player.SendSpawned();

        list.Add(id, player);
        

    }

    private void FixedUpdate()
    {
       sendPlayerPositions();
    }

    #region health and death


    [Header("Health and stats")]
    [SerializeField] private float health;

    [MessageHandler((ushort)ClientToServerId.damage)]
    private static void dealDamage(Message message)
    {
        Player player = list[message.GetUShort()];

        player.health -= message.GetFloat();
    }

    private void sendPlayerHealth()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerHealth);

        message.AddUShort(id);

        message.AddFloat(health);

        NetworkManager.Singleton.Server.SendToAll(message);

    }


    #endregion

    #region Messages

    private void SendSpawned()
    {

        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerSpawned)), toClientId);

    }

    private Message AddSpawnData(Message message)
    {
        
        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(transform.position);
        return message;
    }

    private void sendPlayerPositions()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerPositions);
        message.AddUShort(id);
        message.AddVector3(transform.position);

        //Debug.Log("Sending positions to players" + transform.position);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void changePlayerPosition(Vector3 position)
    {
        transform.position = position;
    }


    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }



    [MessageHandler((ushort)ClientToServerId.playerPosition)]
    private static void getPosition(ushort fromClientId, Message message)
    {

        Debug.Log("Recieved player position");

        Player player = list[message.GetUShort()];
        player.changePlayerPosition(message.GetVector3());

    }




    #endregion

    
}
