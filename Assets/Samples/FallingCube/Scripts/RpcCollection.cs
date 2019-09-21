using System;
using Unity.Entities;
using Unity.Networking.Transport;

public struct RpcCollection : IRpcCollection
{
    static Type[] s_RpcTypes = new Type[]
    {

    };
    public void ExecuteRpc(int type, DataStreamReader reader, ref DataStreamReader.Context ctx, Entity connection, EntityCommandBuffer.Concurrent commandBuffer, int jobIndex)
    {
        switch (type)
        {

        }
    }

    public int GetRpcFromType<T>() where T : struct, IRpcCommand
    {
        for (int i = 0; i < s_RpcTypes.Length; ++i)
        {
            if (s_RpcTypes[i] == typeof(T))
                return i;
        }

        return -1;
    }
}

public class clientserverRpcSystem : RpcSystem<RpcCollection>
{
}
