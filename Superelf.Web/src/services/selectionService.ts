import apiClient from './api';

export interface FootballPlayerDto {
  id: string;
  name: string;
  position: string;
  nationality: string;
  club?: string;
  clubId?: string;
  photoUrl?: string;
  createdAt: string;
  specificPosition?: number; // New field for specific position
  isJoker?: boolean; // New field to indicate if player is joker
}

export interface SelectedPlayerDto {
  playerId: string;
  player: FootballPlayerDto;
  specificPosition: number;
  isJoker: boolean;
  positionName: string; // Human readable position name
}

export interface SelectionDto {
  poolId: string;
  complete: boolean;
  formation: string;
  totalPlayers: number;
  uniqueNationalities: number;
  hasJoker: boolean;
  jokerPlayerId?: string;
  
  selectedBasisGoalkeeper?: SelectedPlayerDto;
  selectedBasisDefenders: (SelectedPlayerDto | null)[];
  selectedBasisMidfielders: (SelectedPlayerDto | null)[];
  selectedBasisForwards: (SelectedPlayerDto | null)[];
  
  selectedReserveGoalkeeper?: SelectedPlayerDto;
  selectedReserveDefender?: SelectedPlayerDto;
  selectedReserveMidfielder?: SelectedPlayerDto;
  selectedReserveForward?: SelectedPlayerDto;
  
  goalkeepers: FootballPlayerDto[];
  defenders: FootballPlayerDto[];
  midfielders: FootballPlayerDto[];
  forwards: FootballPlayerDto[];
}

export interface SelectionTableDto {
  poolId: string;
  position: string;
  isReserve: boolean;
  maxSelection: number;
  players: FootballPlayerDto[];
  uniqueNationalities: number;
  totalSelectedPlayers: number;
  remainingPlayers: number;
  alreadySelectedPlayers: string[];
  selectedNationalities: string[];
}

export interface SubmitSelectionDto {
  position: string;
  isReserve: boolean;
  selectedPlayers: string[];
  isJoker: boolean;
  slotIndex?: number; // New field to track which slot was clicked
}

export interface SetJokerDto {
  playerId: string;
}

const selectionService = {
  getSelection: async (poolId: string): Promise<SelectionDto> => {
    try {
      const response = await apiClient.get(`/selection/${poolId}`);
      return response.data;
    } catch (error: any) {
      console.error('Error fetching selection:', error);
      throw error;
    }
  },

  getPlayersForSelection: async (
    poolId: string,
    position: string,
    isReserve: boolean = false,
    maxSelection: number = 1
  ): Promise<SelectionTableDto> => {
    try {
      const response = await apiClient.get(
        `/selection/${poolId}/players/${position}?isReserve=${isReserve}&maxSelection=${maxSelection}`
      );
      return response.data;
    } catch (error: any) {
      console.error('Error fetching players for selection:', error);
      throw error;
    }
  },

  submitSelection: async (
    poolId: string,
    selectionData: SubmitSelectionDto
  ): Promise<string> => {
    try {
      const response = await apiClient.post(
        `/selection/${poolId}/submit`,
        selectionData
      );
      return response.data;
    } catch (error: any) {
      if (error.response && error.response.data) {
        throw new Error(error.response.data);
      }
      throw new Error('Failed to submit selection');
    }
  },

  setJoker: async (poolId: string, playerId: string): Promise<string> => {
    try {
      const response = await apiClient.post(
        `/selection/${poolId}/joker`,
        { playerId } as SetJokerDto
      );
      return response.data;
    } catch (error: any) {
      if (error.response && error.response.data) {
        throw new Error(error.response.data);
      }
      throw new Error('Failed to set joker');
    }
  },

  finalizeSelection: async (poolId: string): Promise<string> => {
    try {
      const response = await apiClient.post(`/selection/${poolId}/finalize`);
      return response.data;
    } catch (error: any) {
      if (error.response && error.response.data) {
        throw new Error(error.response.data);
      }
      throw new Error('Failed to finalize selection');
    }
  }
};

export default selectionService;
