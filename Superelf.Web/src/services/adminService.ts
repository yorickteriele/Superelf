import apiClient from './api';

// Admin Interfaces
export interface AdminUser {
  id: string;
  username: string;
  email: string;
  phoneNumber?: string;
  emailConfirmed: boolean;
  lockoutEnd?: string;
  accessFailedCount: number;
  roles: string[];
}

export interface FootballPlayer {
  id: string;
  name: string;
  position: string;
  nationality: string;
  club?: string;
  clubId?: string;
  photoUrl?: string;
  createdAt: string;
}

export interface CreatePlayerRequest {
  name: string;
  position: string;
  nationality: string;
  club?: string;
  clubId?: string;
  photoUrl?: string;
}

export interface UpdatePlayerRequest {
  name: string;
  position: string;
  nationality: string;
  club?: string;
  clubId?: string;
  photoUrl?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PlayerFilter {
  search?: string;
  position?: string;
  nationality?: string;
  clubId?: string;
  sortBy?: string;
  sortDirection?: string;
  page?: number;
  pageSize?: number;
}

export interface BulkCreatePlayersRequest {
  players: CreatePlayerRequest[];
  skipDuplicates: boolean;
}

export interface BulkCreateResult {
  created: number;
  skipped: number;
  failed: number;
  skippedPlayers: string[];
  failedPlayers: string[];
}

export interface Club {
  id: string;
  name: string;
  shortName?: string;
  logoUrl?: string;
  country?: string;
  playerCount: number;
  createdAt: string;
}

export interface CreateClubRequest {
  name: string;
  shortName?: string;
  logoUrl?: string;
  country?: string;
}

export interface UpdateClubRequest {
  name: string;
  shortName?: string;
  logoUrl?: string;
  country?: string;
}

export interface League {
  id: string;
  name: string;
  shortName?: string;
  country?: string;
  logoUrl?: string;
  googleCalendarId?: string;
  lastSyncAt?: string;
  isActive: boolean;
  clubCount: number;
  matchCount: number;
  createdAt: string;
}

export interface CreateLeagueRequest {
  name: string;
  shortName?: string;
  country?: string;
  logoUrl?: string;
  googleCalendarId?: string;
  isActive?: boolean;
}

export interface UpdateLeagueRequest {
  name: string;
  shortName?: string;
  country?: string;
  logoUrl?: string;
  googleCalendarId?: string;
  isActive?: boolean;
}

export interface GoogleCalendarSyncRequest {
  leagueId: string;
  googleCalendarId: string;
  forceSync?: boolean;
  startDate?: string; // ISO date string
  endDate?: string;   // ISO date string
}

export interface Match {
  id: string;
  homeTeam: string;
  awayTeam: string;
  homeScore?: number;
  awayScore?: number;
  matchDate: string;
  competition?: string;
  round: number;
  isCompleted: boolean;
  createdAt: string;
  playerPerformances: PlayerPerformance[];
}

export interface CreateMatchRequest {
  homeTeam: string;
  awayTeam: string;
  matchDate: string;
  competition?: string;
  round: number;
}

export interface UpdateMatchRequest {
  homeTeam: string;
  awayTeam: string;
  homeScore?: number;
  awayScore?: number;
  matchDate: string;
  competition?: string;
  round: number;
  isCompleted: boolean;
}

export interface BulkUpdateClubsRequest {
  playerIds: string[];
  newClub: string;
}

export interface AdminStats {
  totalUsers: number;
  totalAdmins: number;
  totalPlayers: number;
  totalClubs: number;
  totalMatches: number;
  completedMatches: number;
  totalPools: number;
  totalLineups: number;
  playersByPosition: { position: string; count: number }[];
  playersByNationality: { nationality: string; count: number }[];
  playersByClub: { club: string; count: number }[];
}

export interface ProcessMatchesRequest {
  matchResults: MatchResult[];
}

export interface MatchResult {
  matchId: string;
  homeTeam: string;
  awayTeam: string;
  homeScore: number;
  awayScore: number;
  playerPerformances: PlayerPerformance[];
}

export interface PlayerPerformance {
  id: string;
  matchId: string;
  playerId: string;
  playerName: string;
  playerPosition: string;
  playerClub: string;
  goals: number;
  penaltyGoals: number;
  penaltiesMissed: number;
  ownGoals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  played: boolean;
  points: number;
}

export interface CreatePlayerPerformanceRequest {
  playerId: string;
  goals: number;
  penaltyGoals: number;
  penaltiesMissed: number;
  ownGoals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  played: boolean;
}

export interface UpdatePlayerPerformanceRequest {
  goals: number;
  penaltyGoals: number;
  penaltiesMissed: number;
  ownGoals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  played: boolean;
}

export const adminService = {
  // Users Management
  getAllUsers: async (): Promise<AdminUser[]> => {
    const response = await apiClient.get('/admin/users');
    return response.data;
  },

  makeUserAdmin: async (userId: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/make-admin`);
  },

  removeUserAdmin: async (userId: string): Promise<void> => {
    await apiClient.post(`/admin/users/${userId}/remove-admin`);
  },

  deleteUser: async (userId: string): Promise<void> => {
    await apiClient.delete(`/admin/users/${userId}`);
  },

  // Players Management
  getAllPlayers: async (filter?: PlayerFilter): Promise<PagedResult<FootballPlayer>> => {
    const params = new URLSearchParams();
    if (filter?.search) params.append('search', filter.search);
    if (filter?.position) params.append('position', filter.position);
    if (filter?.nationality) params.append('nationality', filter.nationality);
    if (filter?.clubId) params.append('clubId', filter.clubId);
    if (filter?.sortBy) params.append('sortBy', filter.sortBy);
    if (filter?.sortDirection) params.append('sortDirection', filter.sortDirection);
    if (filter?.page) params.append('page', filter.page.toString());
    if (filter?.pageSize) params.append('pageSize', filter.pageSize.toString());

    const response = await apiClient.get(`/admin/players?${params.toString()}`);
    return response.data;
  },

  createPlayer: async (player: CreatePlayerRequest): Promise<FootballPlayer> => {
    const response = await apiClient.post('/admin/players', player);
    return response.data;
  },

  bulkCreatePlayers: async (request: BulkCreatePlayersRequest): Promise<BulkCreateResult> => {
    const response = await apiClient.post('/admin/players/bulk', request);
    return response.data;
  },

  updatePlayer: async (playerId: string, player: UpdatePlayerRequest): Promise<void> => {
    await apiClient.put(`/admin/players/${playerId}`, player);
  },

  deletePlayer: async (playerId: string): Promise<void> => {
    await apiClient.delete(`/admin/players/${playerId}`);
  },

  // Clubs Management
  getAllClubs: async (): Promise<Club[]> => {
    const response = await apiClient.get('/admin/clubs');
    return response.data;
  },

  createClub: async (club: CreateClubRequest): Promise<Club> => {
    const response = await apiClient.post('/admin/clubs', club);
    return response.data;
  },

  updateClub: async (clubId: string, club: UpdateClubRequest): Promise<void> => {
    await apiClient.put(`/admin/clubs/${clubId}`, club);
  },

  deleteClub: async (clubId: string): Promise<void> => {
    await apiClient.delete(`/admin/clubs/${clubId}`);
  },

  bulkUpdatePlayerClubs: async (request: BulkUpdateClubsRequest): Promise<void> => {
    await apiClient.post('/admin/clubs/bulk-update', request);
  },

  // Match Management
  getAllMatches: async (onlyCompleted: boolean = false): Promise<Match[]> => {
    const response = await apiClient.get(`/admin/matches?onlyCompleted=${onlyCompleted}`);
    return response.data;
  },

  getMatch: async (matchId: string): Promise<Match> => {
    const response = await apiClient.get(`/admin/matches/${matchId}`);
    return response.data;
  },

  createMatch: async (match: CreateMatchRequest): Promise<Match> => {
    const response = await apiClient.post('/admin/matches', match);
    return response.data;
  },

  updateMatch: async (matchId: string, match: UpdateMatchRequest): Promise<void> => {
    await apiClient.put(`/admin/matches/${matchId}`, match);
  },

  deleteMatch: async (matchId: string): Promise<void> => {
    await apiClient.delete(`/admin/matches/${matchId}`);
  },

  // Player Performance Management
  getMatchPerformances: async (matchId: string): Promise<PlayerPerformance[]> => {
    const response = await apiClient.get(`/admin/matches/${matchId}/performances`);
    return response.data;
  },

  addPlayerPerformance: async (matchId: string, performance: CreatePlayerPerformanceRequest): Promise<PlayerPerformance> => {
    const response = await apiClient.post(`/admin/matches/${matchId}/performances`, performance);
    return response.data;
  },

  addBulkPlayerPerformances: async (matchId: string, performances: CreatePlayerPerformanceRequest[]): Promise<string> => {
    const response = await apiClient.post(`/admin/matches/${matchId}/performances/bulk`, {
      matchId,
      performances
    });
    return response.data;
  },

  updatePlayerPerformance: async (performanceId: string, performance: UpdatePlayerPerformanceRequest): Promise<void> => {
    await apiClient.put(`/admin/performances/${performanceId}`, performance);
  },

  deletePlayerPerformance: async (performanceId: string): Promise<void> => {
    await apiClient.delete(`/admin/performances/${performanceId}`);
  },

  // Statistics
  getStats: async (): Promise<AdminStats> => {
    const response = await apiClient.get('/admin/stats');
    return response.data;
  },

  // Match Processing
  processMatches: async (request: ProcessMatchesRequest): Promise<void> => {
    await apiClient.post('/admin/matches/process', request);
  },

  // League Management
  getAllLeagues: async (): Promise<League[]> => {
    const response = await apiClient.get('/admin/leagues');
    return response.data;
  },

  createLeague: async (league: CreateLeagueRequest): Promise<League> => {
    const response = await apiClient.post('/admin/leagues', league);
    return response.data;
  },

  updateLeague: async (leagueId: string, league: UpdateLeagueRequest): Promise<void> => {
    await apiClient.put(`/admin/leagues/${leagueId}`, league);
  },

  deleteLeague: async (leagueId: string): Promise<void> => {
    await apiClient.delete(`/admin/leagues/${leagueId}`);
  },

  syncGoogleCalendar: async (request: GoogleCalendarSyncRequest): Promise<void> => {
    await apiClient.post(`/admin/leagues/${request.leagueId}/sync-calendar`, request);
  },

  testCalendarAccess: async (calendarId: string): Promise<boolean> => {
    const response = await apiClient.post('/admin/test-calendar-access', { calendarId });
    return response.data;
  },

  debugCalendarData: async (leagueId: string, request: GoogleCalendarSyncRequest): Promise<any> => {
    const response = await apiClient.post(`/admin/debug-calendar/${leagueId}`, request);
    return response.data;
  },

  // Maintenance
  cleanupDatabase: async (): Promise<string> => {
    const response = await apiClient.post('/admin/maintenance/cleanup');
    return response.data;
  }
};
