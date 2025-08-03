import * as signalR from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import config from '../config';

const HUB_URL = config.hubUrl;

// This function creates a connection to a SignalR hub
export const createHubConnection = (hubName: string) => {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_URL}/hubs/${hubName}`)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
};

// Custom hook for PoolHub connection
export const usePoolHub = (poolId?: string) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  
  useEffect(() => {
    // Create the connection
    const hubConnection = createHubConnection('poolHub');
    setConnection(hubConnection);
    
    // Start the connection
    const startConnection = async () => {
      try {
        await hubConnection.start();
        console.log('SignalR connection started successfully');
        setConnected(true);
        
        // Join the pool group if poolId is provided
        if (poolId) {
          await hubConnection.invoke('JoinPoolGroup', poolId);
          console.log(`Joined pool group: ${poolId}`);
        }
      } catch (error) {
        console.error('Error starting SignalR connection:', error);
        // Retry after 5 seconds
        setTimeout(startConnection, 5000);
      }
    };
    
    startConnection();
    
    // Set up event handlers for connection changes
    hubConnection.onreconnecting(() => {
      console.log('Attempting to reconnect to SignalR hub...');
      setConnected(false);
    });
    
    hubConnection.onreconnected(() => {
      console.log('SignalR connection reestablished');
      setConnected(true);
      
      // Rejoin the pool group if poolId is provided
      if (poolId) {
        hubConnection.invoke('JoinPoolGroup', poolId)
          .then(() => console.log(`Rejoined pool group: ${poolId}`))
          .catch((err: Error) => console.error('Error rejoining pool group:', err));
      }
    });
    
    hubConnection.onclose(() => {
      console.log('SignalR connection closed');
      setConnected(false);
    });
    
    // Clean up when component unmounts
    return () => {
      // Leave the pool group if poolId is provided
      if (poolId && hubConnection.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke('LeavePoolGroup', poolId)
          .then(() => console.log(`Left pool group: ${poolId}`))
          .catch((err: Error) => console.error('Error leaving pool group:', err));
      }
      
      hubConnection.stop()
        .then(() => console.log('SignalR connection stopped'))
        .catch((err: Error) => console.error('Error stopping SignalR connection:', err));
    };
  }, [poolId]);
  
  return { connection, connected };
};
