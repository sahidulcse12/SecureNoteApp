'use client';

import { useEffect, useState, useCallback } from 'react';
import { usersApi, User } from '@/lib/api';
import RequireAuth from '@/components/RequireAuth';

function UserModal({ user, onClose, onSave }: { user: Partial<User> | null; onClose: () => void; onSave: () => void }) {
  const isNew = !user?.id;
  const [form, setForm] = useState({
    username: user?.username || '',
    email: user?.email || '',
    fullName: user?.fullName || '',
    password: '',
    role: user?.role || 'User',
    interests: (user?.interests || []).join(', '),
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      const interests = form.interests.split(',').map(s => s.trim()).filter(Boolean);
      if (isNew) {
        await usersApi.create({ ...form, interests });
      } else if (user?.id) {
        await usersApi.update(user.id, {
          username: form.username,
          email: form.email,
          fullName: form.fullName,
          role: form.role,
          interests,
          ...(form.password ? { password: form.password } : {}),
        });
      }
      onSave();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md">
        <div className="p-6">
          <h2 className="text-xl font-bold mb-4">{isNew ? 'Create User' : 'Edit User'}</h2>
          <div className="space-y-3">
            {isNew && (
              <input placeholder="Username" value={form.username} onChange={set('username')}
                className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none" />
            )}
            <input placeholder="Full Name" value={form.fullName} onChange={set('fullName')}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none" />
            <input placeholder="Email" type="email" value={form.email} onChange={set('email')}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none" />
            {isNew && (
              <input placeholder="Password" type="password" value={form.password} onChange={set('password')}
                className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none" />
            )}
            <select value={form.role} onChange={set('role')}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none">
              <option value="User">User</option>
              <option value="Admin">Admin</option>
            </select>
            <input placeholder="Interests (comma separated)" value={form.interests} onChange={set('interests')}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none" />
          </div>
          {error && <p className="text-red-600 text-sm mt-2">{error}</p>}
          <div className="flex justify-end gap-3 mt-4">
            <button onClick={onClose} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg">Cancel</button>
            <button onClick={handleSave} disabled={saving}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function AdminUsersContent() {
  const [users, setUsers] = useState<User[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [modalUser, setModalUser] = useState<Partial<User> | null | undefined>(undefined);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await usersApi.list(page);
      setUsers(res.items);
      setTotalPages(res.totalPages);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load users');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const handleDelete = async (id: string, username: string) => {
    if (!confirm(`Delete user "${username}"?`)) return;
    try {
      await usersApi.delete(id);
      load();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-800">User Management <span className="text-indigo-600">(Admin)</span></h1>
          <p className="text-gray-500 mt-1">{totalCount} total users</p>
        </div>
        <button onClick={() => setModalUser({})} className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700">
          + Create User
        </button>
      </div>

      {error && <p className="text-red-600 mb-4">{error}</p>}

      {loading ? (
        <div className="text-center py-16 text-gray-500">Loading...</div>
      ) : (
        <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Username</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Email</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Role</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Interests</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {users.map(user => (
                <tr key={user.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{user.fullName}</td>
                  <td className="px-4 py-3 text-gray-600 font-mono">@{user.username}</td>
                  <td className="px-4 py-3 text-gray-600">{user.email}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-1 rounded-full font-medium ${user.role === 'Admin' ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'}`}>
                      {user.role}
                    </span>
                  </td>
                  <td className="px-4 py-3 max-w-[200px]">
                    <div className="flex flex-wrap gap-1">
                      {user.interests?.slice(0, 3).map(i => (
                        <span key={i} className="bg-gray-100 text-gray-600 text-xs px-1.5 py-0.5 rounded">{i}</span>
                      ))}
                      {user.interests?.length > 3 && <span className="text-gray-400 text-xs">+{user.interests.length - 3}</span>}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex gap-3 justify-end">
                      <button onClick={() => setModalUser(user)} className="text-indigo-600 hover:underline text-xs">Edit</button>
                      <button onClick={() => handleDelete(user.id, user.username)} className="text-red-600 hover:underline text-xs">Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-6">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
            <button key={p} onClick={() => setPage(p)}
              className={`w-9 h-9 rounded-lg text-sm ${p === page ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
            >{p}</button>
          ))}
        </div>
      )}

      {modalUser !== undefined && (
        <UserModal user={modalUser} onClose={() => setModalUser(undefined)} onSave={load} />
      )}
    </div>
  );
}

export default function AdminUsersPage() {
  return <RequireAuth adminOnly><AdminUsersContent /></RequireAuth>;
}
