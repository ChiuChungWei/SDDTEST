import { create } from 'zustand';
import { UserResponse } from '../types/auth';

interface AuthStore {
  user: UserResponse | null;
  token: string | null;
  isAuthenticated: boolean;
  setUser: (user: UserResponse) => void;
  setToken: (token: string) => void;
  logout: () => void;
  loadFromStorage: () => void;
}

export const useAuthStore = create<AuthStore>((set) => ({
  user: null,
  token: null,
  isAuthenticated: false,

  setUser: (user: UserResponse) => {
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, isAuthenticated: true });
  },

  setToken: (token: string) => {
    localStorage.setItem('authToken', token);
    set({ token });
  },

  logout: () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    set({ user: null, token: null, isAuthenticated: false });
  },

  loadFromStorage: () => {
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');

    if (token && user) {
      set({
        token,
        user: JSON.parse(user),
        isAuthenticated: true,
      });
    }
  },
}));
