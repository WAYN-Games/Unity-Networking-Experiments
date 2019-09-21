using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

internal struct FixedTimeLoop
{
    public float accumulatedTime;
    public const float fixedTimeStep = 1f / 60f;
    public const int maxTimeSteps = 4;
    public int timeSteps;

    public void BeginUpdate()
    {
        accumulatedTime += Time.deltaTime;
        timeSteps = 0;
    }
    public bool ShouldUpdate()
    {
        if (accumulatedTime < fixedTimeStep)
            return false;
        ++timeSteps;
        if (timeSteps > maxTimeSteps)
        {
            accumulatedTime = accumulatedTime % fixedTimeStep;
            return false;
        }
        accumulatedTime -= fixedTimeStep;
        return true;
    }
}
// Update loop for client and server worlds
[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ServerSimulationSystemGroup : ComponentSystemGroup
{
    private BeginSimulationEntityCommandBufferSystem m_beginBarrier;
    private EndSimulationEntityCommandBufferSystem m_endBarrier;
    private uint m_ServerTick;
    public uint ServerTick => m_ServerTick;
    private FixedTimeLoop m_fixedTimeLoop;
    public float UpdateTime => Time.time - m_fixedTimeLoop.accumulatedTime;
    public float UpdateDeltaTime => FixedTimeLoop.fixedTimeStep;

    protected override void OnCreateManager()
    {
        m_beginBarrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_endBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_ServerTick = 1;
    }

    protected List<ComponentSystemBase> m_systemsInGroup = new List<ComponentSystemBase>();

    public override IEnumerable<ComponentSystemBase> Systems => m_systemsInGroup;

    protected override void OnUpdate()
    {
        var defaultWorld = World.Active;
        World.Active = World;
        m_fixedTimeLoop.BeginUpdate();
        while (m_fixedTimeLoop.ShouldUpdate())
        {
            m_beginBarrier.Update();
            
            base.OnUpdate();
            m_endBarrier.Update();
            ++m_ServerTick;
            if (m_ServerTick == 0)
                ++m_ServerTick;
        }

        World.Active = defaultWorld;
    }

    public override void SortSystemUpdateList()
    {
        base.SortSystemUpdateList();
        m_systemsInGroup = new List<ComponentSystemBase>(1 + m_systemsToUpdate.Count + 1);
        m_systemsInGroup.Add(m_beginBarrier);
        m_systemsInGroup.AddRange(m_systemsToUpdate);
        m_systemsInGroup.Add(m_endBarrier);
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ClientSimulationSystemGroup : ComponentSystemGroup
{
    private BeginSimulationEntityCommandBufferSystem m_beginBarrier;
    private EndSimulationEntityCommandBufferSystem m_endBarrier;
    private GhostSpawnSystemGroup m_ghostSpawnGroup;
#if UNITY_EDITOR
    public int ClientWorldIndex { get; internal set; }
#endif
    private FixedTimeLoop m_fixedTimeLoop;
    public float UpdateTime => Time.time - m_fixedTimeLoop.accumulatedTime;
    public float UpdateDeltaTime => FixedTimeLoop.fixedTimeStep;

    protected override void OnCreateManager()
    {
        m_beginBarrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_endBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_ghostSpawnGroup = World.GetOrCreateSystem<GhostSpawnSystemGroup>();
    }

    protected List<ComponentSystemBase> m_systemsInGroup = new List<ComponentSystemBase>();

    public override IEnumerable<ComponentSystemBase> Systems => m_systemsInGroup;

    protected override void OnUpdate()
    {
        var defaultWorld = World.Active;
        World.Active = World;
        m_fixedTimeLoop.BeginUpdate();
        while (m_fixedTimeLoop.ShouldUpdate())
        {
            m_beginBarrier.Update();
            m_ghostSpawnGroup.Update();
            base.OnUpdate();
            m_endBarrier.Update();
        }

        World.Active = defaultWorld;
    }

    public override void SortSystemUpdateList()
    {
        base.SortSystemUpdateList();
        m_systemsInGroup = new List<ComponentSystemBase>(1 + m_systemsToUpdate.Count + 1);
        m_systemsInGroup.Add(m_beginBarrier);
        m_systemsInGroup.Add(m_ghostSpawnGroup);
        m_systemsInGroup.AddRange(m_systemsToUpdate);
        m_systemsInGroup.Add(m_endBarrier);
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ClientPresentationSystemGroup : ComponentSystemGroup
{
    private BeginPresentationEntityCommandBufferSystem m_beginBarrier;
    private EndPresentationEntityCommandBufferSystem m_endBarrier;

    protected override void OnCreateManager()
    {
        m_beginBarrier = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        m_endBarrier = World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();
    }

    protected List<ComponentSystemBase> m_systemsInGroup = new List<ComponentSystemBase>();

    public override IEnumerable<ComponentSystemBase> Systems => m_systemsInGroup;

    protected override void OnUpdate()
    {
        var defaultWorld = World.Active;
        World.Active = World;
        m_beginBarrier.Update();
        base.OnUpdate();
        m_endBarrier.Update();
        World.Active = defaultWorld;
    }

    public override void SortSystemUpdateList()
    {
        base.SortSystemUpdateList();
        m_systemsInGroup = new List<ComponentSystemBase>(1 + m_systemsToUpdate.Count + 1);
        m_systemsInGroup.Add(m_beginBarrier);
        m_systemsInGroup.AddRange(m_systemsToUpdate);
        m_systemsInGroup.Add(m_endBarrier);
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ClientInitializationSystemGroup : ComponentSystemGroup
{
    private BeginInitializationEntityCommandBufferSystem m_beginBarrier;
    private EndInitializationEntityCommandBufferSystem m_endBarrier;

    protected override void OnCreateManager()
    {
        m_beginBarrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        m_endBarrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected List<ComponentSystemBase> m_systemsInGroup = new List<ComponentSystemBase>();

    public override IEnumerable<ComponentSystemBase> Systems => m_systemsInGroup;

    protected override void OnUpdate()
    {
        var defaultWorld = World.Active;
        World.Active = World;
        m_beginBarrier.Update();
        base.OnUpdate();
        m_endBarrier.Update();
        World.Active = defaultWorld;
    }

    public override void SortSystemUpdateList()
    {
        base.SortSystemUpdateList();
        m_systemsInGroup = new List<ComponentSystemBase>(1 + m_systemsToUpdate.Count + 1);
        m_systemsInGroup.Add(m_beginBarrier);
        m_systemsInGroup.AddRange(m_systemsToUpdate);
        m_systemsInGroup.Add(m_endBarrier);
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ServerInitializationSystemGroup : ComponentSystemGroup
{
    private BeginInitializationEntityCommandBufferSystem m_beginBarrier;
    private EndInitializationEntityCommandBufferSystem m_endBarrier;

    protected override void OnCreateManager()
    {
        m_beginBarrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        m_endBarrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected List<ComponentSystemBase> m_systemsInGroup = new List<ComponentSystemBase>();

    public override IEnumerable<ComponentSystemBase> Systems => m_systemsInGroup;

    protected override void OnUpdate()
    {
        var defaultWorld = World.Active;
        World.Active = World;
        m_beginBarrier.Update();
        base.OnUpdate();
        m_endBarrier.Update();
        World.Active = defaultWorld;
    }

    public override void SortSystemUpdateList()
    {
        base.SortSystemUpdateList();
        m_systemsInGroup = new List<ComponentSystemBase>(1 + m_systemsToUpdate.Count + 1);
        m_systemsInGroup.Add(m_beginBarrier);
        m_systemsInGroup.AddRange(m_systemsToUpdate);
        m_systemsInGroup.Add(m_endBarrier);
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ClientAndServerSimulationSystemGroup : ComponentSystemGroup
{
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class ClientAndServerInitializationSystemGroup : ComponentSystemGroup
{
}

// Ticking of client and server worlds from the main world
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
[AlwaysUpdateSystem]
[NotClientServerSystem]
public class TickServerSimulationSystem : ComponentSystemGroup
{
    public override void SortSystemUpdateList()
    {
    }
}
[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
[NotClientServerSystem]
public class TickServerInitializationSystem : ComponentSystemGroup
{
    public override void SortSystemUpdateList()
    {
    }
}
#endif
#if !UNITY_SERVER
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
[UpdateAfter(typeof(TickServerSimulationSystem))]
#endif
[AlwaysUpdateSystem]
[NotClientServerSystem]
public class TickClientSimulationSystem : ComponentSystemGroup
{
    public override void SortSystemUpdateList()
    {
    }
}
[UpdateInGroup(typeof(PresentationSystemGroup))]
[AlwaysUpdateSystem]
[NotClientServerSystem]
public class TickClientPresentationSystem : ComponentSystemGroup
{
    public override void SortSystemUpdateList()
    {
    }
}
[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
[NotClientServerSystem]
public class TickClientInitializationSystem : ComponentSystemGroup
{
    public override void SortSystemUpdateList()
    {
    }
}
#endif

public class NotClientServerSystemAttribute : Attribute
{
}

// Bootstrap of client and server worlds
public class ClientServerBootstrap : ICustomBootstrap
{
    public List<Type> Initialize(List<Type> systems)
    {
        // Workaround for initialization being called multiple times when using game object conversion
#if !UNITY_SERVER
        if (clientWorld != null)
            return systems;
#endif
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
        if (serverWorld != null)
            return systems;
#endif

#if !UNITY_SERVER
#if UNITY_EDITOR
        int numClientWorlds = UnityEditor.EditorPrefs.GetInt("MultiplayerPlayMode_" + UnityEngine.Application.productName + "_NumClients");
        if (numClientWorlds < 1)
            numClientWorlds = 1;
        if (numClientWorlds > 8)
            numClientWorlds = 8;
        int playModeType = UnityEditor.EditorPrefs.GetInt("MultiplayerPlayMode_" + UnityEngine.Application.productName + "_Type");
#else
        int numClientWorlds = 1;
#endif
#endif

        var defaultBootstrap = new List<Type>();
#if !UNITY_SERVER
        clientWorld = null;
        ClientInitializationSystemGroup[] clientInitializationSystemGroup = null;
        ClientSimulationSystemGroup[] clientSimulationSystemGroup = null;
        ClientPresentationSystemGroup[] clientPresentationSystemGroup = null;
#if UNITY_EDITOR
        if (playModeType != 2)
#endif
        {
            clientWorld = new World[numClientWorlds];
            clientInitializationSystemGroup = new ClientInitializationSystemGroup[clientWorld.Length];
            clientSimulationSystemGroup = new ClientSimulationSystemGroup[clientWorld.Length];
            clientPresentationSystemGroup = new ClientPresentationSystemGroup[clientWorld.Length];
            for (int i = 0; i < clientWorld.Length; ++i)
            {
                clientWorld[i] = new World("ClientWorld" + i);
                clientInitializationSystemGroup[i] = clientWorld[i].GetOrCreateSystem<ClientInitializationSystemGroup>();
                clientSimulationSystemGroup[i] = clientWorld[i].GetOrCreateSystem<ClientSimulationSystemGroup>();
#if UNITY_EDITOR
                clientSimulationSystemGroup[i].ClientWorldIndex = i;
#endif
                clientPresentationSystemGroup[i] = clientWorld[i].GetOrCreateSystem<ClientPresentationSystemGroup>();
            }
        }
#endif
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
        serverWorld = null;
        ServerInitializationSystemGroup serverInitializationSystemGroup = null;
        ServerSimulationSystemGroup serverSimulationSystemGroup = null;
#if UNITY_EDITOR
        if (playModeType != 1)
#endif
        {
            serverWorld = new World("ServerWorld");
            serverInitializationSystemGroup = serverWorld.GetOrCreateSystem<ServerInitializationSystemGroup>();
            serverSimulationSystemGroup = serverWorld.GetOrCreateSystem<ServerSimulationSystemGroup>();
        }
#endif
        foreach (var type in systems)
        {
            if (type.GetCustomAttributes(typeof(NotClientServerSystemAttribute), true).Length > 0 ||
                type == typeof(InitializationSystemGroup) ||
                type == typeof(SimulationSystemGroup) ||
                type == typeof(PresentationSystemGroup))
            {
                defaultBootstrap.Add(type);
                continue;
            }
            var groups = type.GetCustomAttributes(typeof(UpdateInGroupAttribute), true);
            if (groups.Length == 0)
            {
                defaultBootstrap.Add(type);
                // Treat the default group as both client and server
                groups = new object[]{new UpdateInGroupAttribute(typeof(ClientAndServerSimulationSystemGroup)) };
            }

            foreach (var grp in groups)
            {
                var group = grp as UpdateInGroupAttribute;
                if (group.GroupType == typeof(ClientAndServerSimulationSystemGroup) || group.GroupType == typeof(SimulationSystemGroup))
                {
                    if (group.GroupType == typeof(SimulationSystemGroup))
                        defaultBootstrap.Add(type);
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
                    if (serverWorld != null)
                    {
                        var defaultWorld = World.Active;
                        World.Active = serverWorld;
                        serverSimulationSystemGroup.AddSystemToUpdateList(
                            serverWorld.GetOrCreateSystem(type));
                        World.Active = defaultWorld;
                    }
#endif
#if !UNITY_SERVER
                    if (clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientSimulationSystemGroup.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            clientSimulationSystemGroup[i]
                                .AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ClientAndServerInitializationSystemGroup) || group.GroupType == typeof(InitializationSystemGroup))
                {
                    if (group.GroupType == typeof(InitializationSystemGroup))
                        defaultBootstrap.Add(type);
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
                    if (serverWorld != null)
                    {
                        var defaultWorld = World.Active;
                        World.Active = serverWorld;
                        serverInitializationSystemGroup.AddSystemToUpdateList(
                            serverWorld.GetOrCreateSystem(type));
                        World.Active = defaultWorld;
                    }
#endif
#if !UNITY_SERVER
                    if (clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientInitializationSystemGroup.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            clientInitializationSystemGroup[i]
                                .AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ServerInitializationSystemGroup))
                {
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
                    if (serverWorld != null)
                    {
                        var defaultWorld = World.Active;
                        World.Active = serverWorld;
                        serverInitializationSystemGroup.AddSystemToUpdateList(
                            serverWorld.GetOrCreateSystem(type));
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ClientInitializationSystemGroup))
                {
#if !UNITY_SERVER
                    if (clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientInitializationSystemGroup.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            clientInitializationSystemGroup[i]
                                .AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ServerSimulationSystemGroup))
                {
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
                    if (serverWorld != null)
                    {
                        var defaultWorld = World.Active;
                        World.Active = serverWorld;
                        serverSimulationSystemGroup.AddSystemToUpdateList(
                            serverWorld.GetOrCreateSystem(type));
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ClientSimulationSystemGroup))
                {
#if !UNITY_SERVER
                    if (clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientSimulationSystemGroup.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            clientSimulationSystemGroup[i]
                                .AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
                }
                else if (group.GroupType == typeof(ClientPresentationSystemGroup) || group.GroupType == typeof(PresentationSystemGroup))
                {
                    if (group.GroupType == typeof(PresentationSystemGroup))
                        defaultBootstrap.Add(type);
#if !UNITY_SERVER
                    if (clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientPresentationSystemGroup.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            clientPresentationSystemGroup[i]
                                .AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
                }
                else
                {
                    var mask = GetTopLevelWorldMask(group.GroupType);
                    if ((mask & WorldType.DefaultWorld) != 0)
                        defaultBootstrap.Add(type);
#if !UNITY_SERVER
                    if ((mask & WorldType.ClientWorld) != 0 && clientWorld != null)
                    {
                        var defaultWorld = World.Active;
                        for (int i = 0; i < clientWorld.Length; ++i)
                        {
                            World.Active = clientWorld[i];
                            var groupSys = clientWorld[i].GetOrCreateSystem(group.GroupType) as ComponentSystemGroup;
                            groupSys.AddSystemToUpdateList(clientWorld[i].GetOrCreateSystem(type));
                        }
                        World.Active = defaultWorld;
                    }
#endif
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
                    if ((mask & WorldType.ServerWorld) != 0 && serverWorld != null)
                    {
                        var defaultWorld = World.Active;
                        World.Active = serverWorld;
                        var groupSys = serverWorld.GetOrCreateSystem(group.GroupType) as ComponentSystemGroup;
                        groupSys.AddSystemToUpdateList(serverWorld.GetOrCreateSystem(type));
                        World.Active = defaultWorld;
                    }
#endif
                }
            }
        }
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
        if (serverWorld != null)
        {
            serverInitializationSystemGroup.SortSystemUpdateList();
            serverSimulationSystemGroup.SortSystemUpdateList();
            World.Active.GetOrCreateSystem<TickServerInitializationSystem>().AddSystemToUpdateList(serverInitializationSystemGroup);
            World.Active.GetOrCreateSystem<TickServerSimulationSystem>().AddSystemToUpdateList(serverSimulationSystemGroup);
        }
#endif
#if !UNITY_SERVER
        if (clientWorld != null)
        {
            for (int i = 0; i < clientWorld.Length; ++i)
            {
                clientInitializationSystemGroup[i].SortSystemUpdateList();
                clientSimulationSystemGroup[i].SortSystemUpdateList();
                clientPresentationSystemGroup[i].SortSystemUpdateList();
                World.Active.GetOrCreateSystem<TickClientInitializationSystem>().AddSystemToUpdateList(clientInitializationSystemGroup[i]);
                World.Active.GetOrCreateSystem<TickClientSimulationSystem>().AddSystemToUpdateList(clientSimulationSystemGroup[i]);
                World.Active.GetOrCreateSystem<TickClientPresentationSystem>().AddSystemToUpdateList(clientPresentationSystemGroup[i]);
            }
        }
#endif
        return defaultBootstrap;
    }

    [Flags]
    enum WorldType
    {
        NoWorld = 0,
        DefaultWorld = 1,
        ClientWorld = 2,
        ServerWorld = 4
    }
    WorldType GetTopLevelWorldMask(Type type)
    {
        var groups = type.GetCustomAttributes(typeof(UpdateInGroupAttribute), true);
        if (groups.Length == 0)
        {
            if (type == typeof(ClientAndServerSimulationSystemGroup) || type == typeof(ClientAndServerInitializationSystemGroup))
                return WorldType.ClientWorld | WorldType.ServerWorld;
            if (type == typeof(SimulationSystemGroup) || type == typeof(InitializationSystemGroup))
                return WorldType.DefaultWorld | WorldType.ClientWorld | WorldType.ServerWorld;
            if (type == typeof(ServerSimulationSystemGroup) || type == typeof(ServerInitializationSystemGroup))
                return WorldType.ServerWorld;
            if (type == typeof(ClientSimulationSystemGroup) ||
                type == typeof(ClientInitializationSystemGroup) ||
                type == typeof(ClientPresentationSystemGroup))
                return WorldType.ClientWorld;
            if (type == typeof(PresentationSystemGroup))
                return WorldType.DefaultWorld | WorldType.ClientWorld;
            // Empty means the same thing as SimulationSystemGroup
            return WorldType.DefaultWorld | WorldType.ClientWorld | WorldType.ServerWorld;
        }

        WorldType mask = WorldType.NoWorld;
        foreach (var grp in groups)
        {
            var group = grp as UpdateInGroupAttribute;
            mask |= GetTopLevelWorldMask(group.GroupType);
        }

        return mask;
    }

#if !UNITY_SERVER
    public static World[] clientWorld;
#endif
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
    public static World serverWorld;
#endif
}

