using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;


public class ClientBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;
    public int bufferSize = 0;
    private Serializer<CustomType> cts;
    void Start()
    {
        m_Driver = new UdpNetworkDriver(new NetworkDataStreamParameter { size = bufferSize });
        m_Connection = default(NetworkConnection);

        NetworkEndPoint client_endpoint = NetworkEndPoint.LoopbackIpv4;
        client_endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(client_endpoint);
        cts = new Serializer<CustomType>();
        cts.Initialize();
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
               NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                CustomType customType = new CustomType() { val = 1 };
                var buff = cts.Serialize(customType);
                using (var writer = new DataStreamWriter(buff.Length, Allocator.Temp))
                {   
                    writer.Write(buff);
                    m_Connection.Send(m_Driver, writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                var value = stream.ReadBytesAsArray(ref readerCtx,stream.Length);
                CustomType ct = cts.DeSerialize(value);
                Debug.Log("Got the value = " + ct.val + " back from the server");
                m_Done = true;
                m_Connection.Disconnect(m_Driver);
                m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }
}
