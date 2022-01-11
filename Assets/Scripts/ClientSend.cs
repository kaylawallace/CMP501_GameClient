using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /*
     * Method to send packets through TCP
     */
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /*
     * Method to send packets through UDP 
     */
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /*
     * Method sends the WelcomeRcvd packet to the server 
     * Packet includes the clients ID and username input at the starting menu 
     * Sends packet through TCP
     */
    public static void WelcomeRcvd()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    /*
     * Method sends the PlayerMovement packet to the server 
     * Packet includes the boolean array of inputs - indicating which input buttons are triggered by the user 
     * Sends packet through UDP 
     */
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.plrMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            SendUDPData(_packet);
        }
    }
    #endregion
}
