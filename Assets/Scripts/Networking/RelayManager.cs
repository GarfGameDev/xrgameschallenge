using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using System;

public class RelayManager : NetworkBehaviour
{
    private NetworkVariable<int> PlayerConnected = new NetworkVariable<int>();

    public int Player2Connect
    {
        get
        {
            return PlayerConnected.Value;
        }
    }

    [SerializeField]
    private string _environment = "production";

    [SerializeField]
    private int _maxConnects = 1;

    public int MaxConnects
    {
        get
        {
            return _maxConnects;
        }
    }
    private string _joinCode;

    public UnityTransport relayTransport;
    
    public bool IsRelayEnabled => relayTransport != null && relayTransport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;


    private static RelayManager _instance;
    public static RelayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("The RelayManager is null");
            }
            return _instance;
        }
    }

    public string JoinCode
    {
        get
        {
            return _joinCode;
        }
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);


    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.IsServer)
            {
                PlayerConnected.Value++;
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback+= (id) =>
        {
            if (NetworkManager.IsServer)
            {
                PlayerConnected.Value--;
            }
        };
    }
    public async Task<RelayHostData> HostGame()
    {
        InitializationOptions options = new InitializationOptions().SetEnvironmentName(_environment);
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync(options);
        // Authenticates the user beforehand
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Ask Unity Services to allocate a Relay server
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(_maxConnects);

        // Populate the hosting data
        RelayHostData data = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        // Retrieve the Relay join code for our clients to join our party
        data.JoinCode = await Relay.Instance.GetJoinCodeAsync(data.AllocationID);
        _joinCode = data.JoinCode;
       

        relayTransport.SetRelayServerData(data.IPv4Address, data.Port, data.AllocationIDBytes, data.Key, data.ConnectionData);

        return data;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        InitializationOptions options = new InitializationOptions().SetEnvironmentName(_environment);
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync(options);
        //Always autheticate your users beforehand
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        JoinAllocation joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        RelayJoinData joinData = new RelayJoinData
        {
            Key = joinAllocation.Key,
            Port = (ushort)joinAllocation.RelayServer.Port,
            AllocationID = joinAllocation.AllocationId,
            AllocationIDBytes = joinAllocation.AllocationIdBytes,
            ConnectionData = joinAllocation.ConnectionData,
            HostConnectionData = joinAllocation.HostConnectionData,
            IPv4Address = joinAllocation.RelayServer.IpV4,
            JoinCode = joinCode
        };

        relayTransport.SetRelayServerData(joinData.IPv4Address, joinData.Port, joinData.AllocationIDBytes, joinData.Key, joinData.ConnectionData, joinData.HostConnectionData);

        return joinData;
    }
}
