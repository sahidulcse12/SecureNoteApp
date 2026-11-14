'use client';

import { useEffect, useState, useCallback } from 'react';
import { aggregationsApi, usersApi, InterestGroup, UserWithPosts, User } from '@/lib/api';
import RequireAuth from '@/components/RequireAuth';

function AggregationsContent() {
  const [groups, setGroups] = useState<InterestGroup[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [userPosts, setUserPosts] = useState<UserWithPosts | null>(null);
  const [loadingGroups, setLoadingGroups] = useState(true);
  const [loadingPosts, setLoadingPosts] = useState(false);
  const [error, setError] = useState('');

  const loadGroups = useCallback(async () => {
    try {
      const data = await aggregationsApi.usersByInterests();
      setGroups(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load groups');
    } finally {
      setLoadingGroups(false);
    }
  }, []);

  const loadUsers = useCallback(async () => {
    try {
      const res = await usersApi.list(1, 100);
      setUsers(res.items);
    } catch {
      // non-admin users won't have access, ignore
    }
  }, []);

  useEffect(() => {
    loadGroups();
    loadUsers();
  }, [loadGroups, loadUsers]);

  const handleLookupPosts = async () => {
    if (!selectedUserId) return;
    setLoadingPosts(true);
    setUserPosts(null);
    try {
      const data = await aggregationsApi.userPosts(selectedUserId);
      setUserPosts(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load user posts');
    } finally {
      setLoadingPosts(false);
    }
  };

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-800 mb-2">Insights & Aggregations</h1>
        <p className="text-gray-500">MongoDB aggregation pipeline results</p>
      </div>

      {error && <p className="text-red-600">{error}</p>}

      {/* Scenario 1: Users by Interests */}
      <section>
        <div className="flex items-center gap-3 mb-4">
          <h2 className="text-xl font-semibold text-gray-700">Scenario 1: Users Grouped by Interests</h2>
          <span className="bg-indigo-100 text-indigo-700 text-xs px-2 py-1 rounded-full font-mono">$unwind → $group</span>
        </div>
        <p className="text-sm text-gray-500 mb-4 font-mono bg-gray-50 p-2 rounded">
          db.users.aggregate([&#123;$unwind: &quot;$interests&quot;&#125;, &#123;$group: &#123;_id: &quot;$interests&quot;, ...&#125;&#125;])
        </p>
        {loadingGroups ? (
          <div className="text-gray-500">Loading...</div>
        ) : groups.length === 0 ? (
          <div className="text-gray-400">No interest groups found.</div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {groups.map(group => (
              <div key={group.interest} className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm">
                <div className="flex justify-between items-start mb-3">
                  <span className="font-semibold text-gray-800 capitalize">{group.interest}</span>
                  <span className="bg-indigo-600 text-white text-xs px-2 py-1 rounded-full">{group.count} users</span>
                </div>
                <div className="space-y-1">
                  {group.users.map(u => (
                    <div key={u.id} className="text-sm text-gray-600 flex items-center gap-2">
                      <span className="w-2 h-2 rounded-full bg-indigo-400 flex-shrink-0" />
                      {u.fullName} <span className="text-gray-400">(@{u.username})</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Scenario 2: User Posts with $lookup */}
      <section>
        <div className="flex items-center gap-3 mb-4">
          <h2 className="text-xl font-semibold text-gray-700">Scenario 2: User Posts via $lookup</h2>
          <span className="bg-green-100 text-green-700 text-xs px-2 py-1 rounded-full font-mono">$match → $lookup → $project</span>
        </div>
        <p className="text-sm text-gray-500 mb-4 font-mono bg-gray-50 p-2 rounded">
          db.users.aggregate([&#123;$match&#125;, &#123;$lookup: &#123;from: &quot;posts&quot;, ...&#125;&#125;, &#123;$project&#125;])
        </p>

        <div className="flex gap-3 mb-4">
          <input
            type="text"
            placeholder="Enter User ID"
            value={selectedUserId}
            onChange={e => setSelectedUserId(e.target.value)}
            className="border rounded-lg px-3 py-2 flex-1 focus:ring-2 focus:ring-indigo-500 focus:outline-none text-sm font-mono"
          />
          {users.length > 0 && (
            <select
              onChange={e => setSelectedUserId(e.target.value)}
              value={selectedUserId}
              className="border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none text-sm"
            >
              <option value="">Pick a user...</option>
              {users.map(u => (
                <option key={u.id} value={u.id}>{u.fullName} (@{u.username})</option>
              ))}
            </select>
          )}
          <button
            onClick={handleLookupPosts}
            disabled={!selectedUserId || loadingPosts}
            className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 text-sm"
          >
            {loadingPosts ? 'Looking up...' : 'Lookup Posts'}
          </button>
        </div>

        {userPosts && (
          <div className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm">
            <div className="flex items-center gap-4 mb-4 pb-4 border-b">
              <div>
                <h3 className="font-semibold text-gray-800">{userPosts.user.fullName}</h3>
                <p className="text-sm text-gray-500">@{userPosts.user.username} · {userPosts.user.email}</p>
              </div>
              <span className="ml-auto bg-indigo-100 text-indigo-700 text-sm px-3 py-1 rounded-full font-medium">
                {userPosts.postCount} post{userPosts.postCount !== 1 ? 's' : ''}
              </span>
            </div>
            {userPosts.posts.length === 0 ? (
              <p className="text-gray-400 text-sm">This user has no posts yet.</p>
            ) : (
              <div className="space-y-3">
                {userPosts.posts.map(post => (
                  <div key={post.id} className="border border-gray-100 rounded-lg p-3">
                    <h4 className="font-medium text-gray-800 mb-1">{post.title}</h4>
                    <p className="text-sm text-gray-600">{post.content}</p>
                    <p className="text-xs text-gray-400 mt-1">{new Date(post.createdAt).toLocaleString()}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  );
}

export default function AggregationsPage() {
  return <RequireAuth><AggregationsContent /></RequireAuth>;
}
