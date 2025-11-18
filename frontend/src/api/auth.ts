import client from './client';
import { LoginRequest, LoginResponse, UserResponse } from '../types/auth';

export const authApi = {
  login: async (adAccount: string, password: string): Promise<LoginResponse> => {
    const response = await client.post<LoginResponse>('/auth/login', {
      adAccount,
      password,
    });
    return response.data;
  },

  logout: async (): Promise<void> => {
    await client.post('/auth/logout');
  },

  getCurrentUser: async (): Promise<UserResponse> => {
    const response = await client.get<UserResponse>('/auth/me');
    return response.data;
  },

  verifyToken: async (token: string): Promise<boolean> => {
    try {
      const response = await client.post('/auth/verify-token', { token });
      return response.data.valid;
    } catch {
      return false;
    }
  },
};
