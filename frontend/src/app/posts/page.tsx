'use client';

import { useEffect, useState, useCallback } from 'react';
import { postsApi, Post } from '@/lib/api';
import { useAuth } from '@/context/AuthContext';

export default function PostsPage() {
  const { user } = useAuth();
  const [posts, setPosts] = useState<Post[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [creating, setCreating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await postsApi.list(page);
      setPosts(res.items);
      setTotalPages(res.totalPages);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load posts');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    setCreating(true);
    try {
      await postsApi.create({ title, content });
      setTitle('');
      setContent('');
      setShowForm(false);
      setPage(1);
      load();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Create failed');
    } finally {
      setCreating(false);
    }
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Public Posts</h1>
        {user && (
          <button
            onClick={() => setShowForm(!showForm)}
            className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700"
          >
            {showForm ? 'Cancel' : '+ New Post'}
          </button>
        )}
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="bg-white border rounded-xl p-4 mb-6 shadow-sm space-y-3">
          <input
            placeholder="Post title"
            value={title}
            onChange={e => setTitle(e.target.value)}
            required
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
          />
          <textarea
            placeholder="What's on your mind?"
            value={content}
            onChange={e => setContent(e.target.value)}
            rows={3}
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none resize-none"
          />
          <div className="flex justify-end">
            <button type="submit" disabled={creating} className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 disabled:opacity-50">
              {creating ? 'Publishing...' : 'Publish'}
            </button>
          </div>
        </form>
      )}

      {error && <p className="text-red-600 mb-4">{error}</p>}

      {loading ? (
        <div className="text-center py-16 text-gray-500">Loading posts...</div>
      ) : posts.length === 0 ? (
        <div className="text-center py-16 text-gray-400">No posts yet. Be the first to publish!</div>
      ) : (
        <div className="space-y-4">
          {posts.map(post => (
            <div key={post.id} className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm">
              <h3 className="font-semibold text-gray-800 text-lg mb-1">{post.title}</h3>
              <p className="text-gray-600 mb-3">{post.content}</p>
              <div className="flex items-center gap-2 text-xs text-gray-400">
                <span>Posted {new Date(post.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-8">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
            <button
              key={p}
              onClick={() => setPage(p)}
              className={`w-9 h-9 rounded-lg text-sm ${p === page ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
            >
              {p}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
