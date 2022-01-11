using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 5555;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        // Set instance to this on awake if null 
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object.");
            Destroy(this);
        }
    }

    private void Start()
    {
        // Create TCP and UDP instances 
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitClientData();

        // Set isConnected to true and call TCP Connect method 
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet rcvdData;
        private byte[] rcvBuffer;

        public void Connect()
        {
            // Initnalise TcpClient socket with buffer sizes 
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            rcvBuffer = new byte[dataBufferSize];
            // Begin asynchronous request for connection at instance ip and port on this socket 
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            // End pending connection attempt 
            socket.EndConnect(_result);

            // Return out if socket not connected 
            if (!socket.Connected)
            {
                return;
            }

            // Set network steam for sending and receiving data 
            stream = socket.GetStream();

            
            // Initialise new packet for receiving data 
            rcvdData = new Packet();

            // Begin reading from stream into the receive buffer 
            stream.BeginRead(rcvBuffer, 0, dataBufferSize, RcvCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                // Write packet to stream if socket is not null 
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch(Exception _e)
            {
                Debug.Log($"Error sending data from client: {_e}");
            }
        }

        private void RcvCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLen = stream.EndRead(_result);
                // Disconnect if bad length 
                if (_byteLen <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                // Create new byte array for data 
                byte[] _data = new byte[_byteLen];
                // Copy content of receive buffer into byte array 
                Array.Copy(rcvBuffer, _data, _byteLen);

                // Reset data received 
                rcvdData.Reset(HandleData(_data));

                // Open to read again
                stream.BeginRead(rcvBuffer, 0, dataBufferSize, RcvCallback, null);
            }
            catch (Exception _e)
            {
                Console.WriteLine($"Error recieving TCP data: {_e}");
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLen = 0;

            rcvdData.SetBytes(_data);

            // Still a byte left if greater than 4 (typically integer) 
            if (rcvdData.UnreadLength() >= 4)
            {
                _packetLen = rcvdData.ReadInt();
                if (_packetLen <= 0)
                {
                    return true;
                }
            }

            // While there is still data to read 
            while (_packetLen > 0 && _packetLen <= rcvdData.UnreadLength())
            {
                // Read bytes from the received data packet into new byte array 
                byte[] _packetBytes = rcvdData.ReadBytes(_packetLen);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    // Add the packet to the packet handlers dictionary 
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLen = 0;
                if (rcvdData.UnreadLength() >= 4)
                {
                    _packetLen = rcvdData.ReadInt();
                    if (_packetLen <= 0)
                    {
                        return true;
                    }
                }
            }

            // Strange length, return out 
            if (_packetLen <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            // Disconnect instance and reset vars on disconnect
            instance.Disconnect();

            stream = null;
            rcvdData = null;
            rcvBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endpoint;

        public UDP()
        {
            // Initialise endpoint using client instance IP and port no.
            endpoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            // Create UDP client instance on local port passed into method 
            socket = new UdpClient(_localPort);

            // Connect UDP client to endpoint 
            socket.Connect(endpoint);
            // Open to receive data 
            socket.BeginReceive(RcvCallback, null);

            // Purpose of this packet is to intialise connection w server and open up local port so client can recieve messages 
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                // Add instance ID to packet 
                _packet.InsertInt(instance.myId);
                // Send packet if not null 
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _e)
            {
                Debug.Log($"Error sending data to server via UDP: {_e}");
            }
        }

        private void RcvCallback(IAsyncResult _result)
        {
            try
            {
                // Create new byte array and stop receiving data 
                byte[] _data = socket.EndReceive(_result, ref endpoint);
                // Open to receive data again 
                socket.BeginReceive(RcvCallback, null);

                // Disconnect if data length less than 4
                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }
                // Otherwise, call handle data method 
                HandleData(_data);
            }
            catch (Exception _e)
            {
                Debug.Log($"Error {_e}");
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                // Read packet length and data from packet 
                int _packetLen = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLen);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    // Add packet to packet handlers dictionary 
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            endpoint = null;
            socket = null;
        }
    }

    private void InitClientData()
    {
        // Initialise packet handlers for incoming packets from server 
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.RcvWelcome },
            { (int)ServerPackets.spawnPlr, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.plrPos, ClientHandle.PlayerPosition },
            { (int)ServerPackets.plrRot, ClientHandle.PlayerRotation },
            { (int)ServerPackets.plrDisconnect, ClientHandle.PlayerDisconnect },
            { (int)ServerPackets.createKeySpawner, ClientHandle.CreateKeySpawner },
            { (int)ServerPackets.keySpawned, ClientHandle.KeySpawned },
            { (int)ServerPackets.keyPickedUp, ClientHandle.KeyPickedUp },
            { (int)ServerPackets.plrHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.plrRespawned, ClientHandle.PlayerRespawn },
            { (int)ServerPackets.bridgePos, ClientHandle.BridgePosition },
            { (int)ServerPackets.platformPos, ClientHandle.PlatformPosition },
            { (int)ServerPackets.endOfLevel, ClientHandle.EndOfLevel }
        };
        Debug.Log($"Initialised packets.");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            // Close sockets on disconnect 
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server");
        }
    }
}
