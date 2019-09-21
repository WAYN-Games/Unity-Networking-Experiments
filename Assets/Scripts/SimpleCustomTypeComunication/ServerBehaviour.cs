using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;


public class ServerBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    private int bufferSize = 0;
    private Serializer<CustomType> cts;

    void Start()
    {
        m_Driver = new UdpNetworkDriver(new NetworkDataStreamParameter { size = bufferSize });
        NetworkEndPoint server_endpoint = NetworkEndPoint.AnyIpv4;
        server_endpoint.Port = 9000;

        if (m_Driver.Bind(server_endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        cts = new Serializer<CustomType>(); 
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }


        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }


        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
                NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var readerCtx = default(DataStreamReader.Context);
                    byte[] value = stream.ReadBytesAsArray(ref readerCtx, stream.Length);
                   
                    CustomType ct = cts.DeSerialize(value);
                    Debug.Log("Got " + ct.val + " from the Client adding + 2 to it.");
                    ct.val += 2;


                    var buff = cts.Serialize(ct);
                    using (var writer = new DataStreamWriter(buff.Length, Allocator.Temp))
                    {
                        writer.Write(buff);
                        m_Connections[i].Send(m_Driver, writer);
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }


}
