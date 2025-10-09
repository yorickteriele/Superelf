import apiClient from './api';

export interface Pool {
  id: string;
  name: string;
  code: string;
  createTime: string;
  ownerName: string;
  participants: PoolParticipant[];
  allowSelectionEditing: boolean;
}

export interface PoolParticipant {
  userId: string;
  userName: string;
  selectionComplete: boolean;
  selectedPlayers: number;
  hasJoker: boolean;
}

export interface UserScore {
  userId: string;
  userName: string;
  score: number;
  playerScores: PlayerScore[];
  hasUsedJoker: boolean;
}

export interface PlayerScore {
  playerId: string;
  playerName: string;
  position: string;
  club: string;
  score: number;
  isJoker: boolean;
}

export interface CreatePoolRequest {
  name: string;
}

export interface JoinPoolRequest {
  poolCode: string;
}

export interface EditPoolNameRequest {
  newName: string;
}

export interface ToggleSelectionEditingRequest {
  allowSelectionEditing: boolean;
}

export interface ApiResponse {
  success: boolean;
  message?: string;
}

// Pool services
export const poolService = {
  getUserPools: async (): Promise<Pool[]> => {
    try {
      const response = await apiClient.get('/pools');
      // Ensure we always return an array
      if (!Array.isArray(response.data)) {
        console.warn('getUserPools: API response is not an array:', response.data);
        return [];
      }
      return response.data;
    } catch (error: any) {
      console.error('Failed to fetch pools:', error);
      return [];
    }
  },
  
  getPool: async (poolId: string): Promise<Pool | null> => {
    try {
      const response = await apiClient.get(`/pools/${poolId}`);
      return response.data;
    } catch (error: any) {
      console.error('Failed to fetch pool details:', error);
      return null;
    }
  },
  
  createPool: async (poolData: CreatePoolRequest): Promise<Pool | null> => {
    try {
      const response = await apiClient.post('/pools', poolData);
      return response.data;
    } catch (error: any) {
      console.error('Failed to create pool:', error);
      return null;
    }
  },
  
  joinPool: async (joinData: JoinPoolRequest): Promise<ApiResponse> => {
    try {
      const response = await apiClient.post('/pools/join', joinData);
      return {
        success: true,
        message: response.data
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data || 'Failed to join pool'
      };
    }
  },
  
  getParticipants: async (poolId: string): Promise<PoolParticipant[]> => {
    try {
      const response = await apiClient.get(`/pools/${poolId}/participants`);
      return response.data;
    } catch (error: any) {
      console.error('Failed to fetch participants:', error);
      return [];
    }
  },
  
  editPoolName: async (poolId: string, editData: EditPoolNameRequest): Promise<ApiResponse> => {
    try {
      const response = await apiClient.put(`/pools/${poolId}/name`, editData);
      return {
        success: true,
        message: response.data
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data || 'Failed to update pool name'
      };
    }
  },
  
  removeParticipant: async (poolId: string, userId: string): Promise<ApiResponse> => {
    try {
      const response = await apiClient.delete(`/pools/${poolId}/participants/${userId}`);
      return {
        success: true,
        message: response.data
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data || 'Failed to remove participant'
      };
    }
  },

  getScoreboard: async (poolId: string): Promise<UserScore[]> => {
    try {
      const response = await apiClient.get(`/pools/${poolId}/scoreboard`);
      return response.data;
    } catch (error: any) {
      console.error('Failed to fetch scoreboard:', error);
      return [];
    }
  },

  toggleSelectionEditing: async (poolId: string, toggleData: ToggleSelectionEditingRequest): Promise<ApiResponse> => {
    try {
      const response = await apiClient.put(`/pools/${poolId}/toggle-selection-editing`, toggleData);
      return {
        success: true,
        message: response.data
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data || 'Failed to toggle selection editing'
      };
    }
  }
};
