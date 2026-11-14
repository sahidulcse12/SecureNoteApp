'use client';

import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { authApi, User } from '@/lib/api';

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string, fullName: string) => Promise<void>;
  logout: () => void;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setUser(null);
    setToken(null);
  }, []);

  useEffect(() => {
    const stored = localStorage.getItem('token');
    if (stored) {
      setToken(stored);
      authApi.me()
        .then(setUser)
        .catch(() => logout())
        .finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, [logout]);

  const login = async (username: string, password: string) => {
    const res = await authApi.login({ username, password });
    localStorage.setItem('token', res.token);
    setToken(res.token);
    setUser(res.user);
  };

  const register = async (username: string, email: string, password: string, fullName: string) => {
    const res = await authApi.register({ username, email, password, fullName });
    localStorage.setItem('token', res.token);
    setToken(res.token);
    setUser(res.user);
  };

  return (
    <AuthContext.Provider value={{
      user, token, loading, login, register, logout,
      isAdmin: user?.role === 'Admin',
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
