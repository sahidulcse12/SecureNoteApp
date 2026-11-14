const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('token');
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };
  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (res.status === 204) return undefined as T;
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data?.message || data?.title || `HTTP ${res.status}`);
  return data as T;
}

// Auth
export const authApi = {
  login: (body: { username: string; password: string }) =>
    request<{ token: string; user: User }>('/auth/login', { method: 'POST', body: JSON.stringify(body) }),
  register: (body: { username: string; email: string; password: string; fullName: string }) =>
    request<{ token: string; user: User }>('/auth/register', { method: 'POST', body: JSON.stringify(body) }),
  me: () => request<User>('/auth/me'),
};

// Notes
export const notesApi = {
  list: (page = 1, pageSize = 10) =>
    request<PagedResult<Note>>(`/notes?page=${page}&pageSize=${pageSize}`),
  listAll: (page = 1, pageSize = 10) =>
    request<PagedResult<Note>>(`/notes/all?page=${page}&pageSize=${pageSize}`),
  get: (id: string) => request<Note>(`/notes/${id}`),
  create: (body: { title: string; content: string; tags?: string[] }) =>
    request<Note>('/notes', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: string, body: { title: string; content: string; tags?: string[] }) =>
    request<Note>(`/notes/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: string) => request<void>(`/notes/${id}`, { method: 'DELETE' }),
};

// Posts
export const postsApi = {
  list: (page = 1, pageSize = 10) =>
    request<PagedResult<Post>>(`/posts?page=${page}&pageSize=${pageSize}`),
  create: (body: { title: string; content: string }) =>
    request<Post>('/posts', { method: 'POST', body: JSON.stringify(body) }),
};

// Users (admin)
export const usersApi = {
  list: (page = 1, pageSize = 10) =>
    request<PagedResult<User>>(`/users?page=${page}&pageSize=${pageSize}`),
  get: (id: string) => request<User>(`/users/${id}`),
  create: (body: { username: string; email: string; password: string; fullName: string; role?: string; interests?: string[] }) =>
    request<User>('/users', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: string, body: { email?: string; fullName?: string; role?: string; interests?: string[] }) =>
    request<User>(`/users/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: string) => request<void>(`/users/${id}`, { method: 'DELETE' }),
};

// Aggregations
export const aggregationsApi = {
  usersByInterests: () =>
    request<InterestGroup[]>('/aggregations/users-by-interests'),
  userPosts: (userId: string) =>
    request<UserWithPosts>(`/aggregations/users/${userId}/posts`),
};

// Types
export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: string;
  interests: string[];
  createdAt: string;
}

export interface Note {
  id: string;
  userId: string;
  title: string;
  content: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Post {
  id: string;
  userId: string;
  title: string;
  content: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface InterestGroup {
  interest: string;
  count: number;
  users: { id: string; username: string; fullName: string }[];
}

export interface UserWithPosts {
  user: User;
  posts: Post[];
  postCount: number;
}
