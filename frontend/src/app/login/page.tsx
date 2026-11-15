'use client';

import { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(username, password);
      router.push('/notes');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-16 bg-white p-8 rounded-xl shadow-md text-black">
      <h1 className="text-2xl font-bold text-black mb-6">Sign In</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-black mb-1">Username</label>
          <input
            type="text"
            value={username}
            onChange={e => setUsername(e.target.value)}
            required
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500 text-black bg-white"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-black mb-1">Password</label>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            required
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500 text-black bg-white"
          />
        </div>
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button
          type="submit"
          disabled={loading}
          className="w-full bg-indigo-600 text-white py-2 rounded-lg hover:bg-indigo-700 disabled:opacity-50 font-medium"
        >
          {loading ? 'Signing in...' : 'Sign In'}
        </button>
      </form>
      <p className="mt-4 text-sm text-center text-black">
        Don&apos;t have an account?{' '}
        <Link href="/register" className="text-indigo-600 hover:underline">Register</Link>
      </p>
      <div className="mt-4 p-3 bg-gray-50 rounded-lg text-xs text-gray-700">
        <strong>Demo credentials:</strong><br />
        Admin: <code>admin</code> / <code>Admin123!</code><br />
        User: <code>alice</code> / <code>User123!</code>
      </div>
    </div>
  );
}
