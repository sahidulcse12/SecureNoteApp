'use client';

import { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function RegisterPage() {
  const { register } = useAuth();
  const router = useRouter();
  const [form, setForm] = useState({ username: '', email: '', password: '', fullName: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register(form.username, form.email, form.password, form.fullName);
      router.push('/notes');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  return (
    <div className="max-w-md mx-auto mt-16 bg-white p-8 rounded-xl shadow-md">
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Create Account</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        {[
          { field: 'fullName', label: 'Full Name', type: 'text' },
          { field: 'username', label: 'Username', type: 'text' },
          { field: 'email', label: 'Email', type: 'email' },
          { field: 'password', label: 'Password', type: 'password' },
        ].map(({ field, label, type }) => (
          <div key={field}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
            <input
              type={type}
              value={form[field as keyof typeof form]}
              onChange={set(field)}
              required
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
        ))}
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button
          type="submit"
          disabled={loading}
          className="w-full bg-indigo-600 text-white py-2 rounded-lg hover:bg-indigo-700 disabled:opacity-50 font-medium"
        >
          {loading ? 'Creating account...' : 'Create Account'}
        </button>
      </form>
      <p className="mt-4 text-sm text-center text-gray-600">
        Already have an account?{' '}
        <Link href="/login" className="text-indigo-600 hover:underline">Sign in</Link>
      </p>
    </div>
  );
}
