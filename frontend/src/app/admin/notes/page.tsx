'use client';

import { useEffect, useState, useCallback } from 'react';
import { notesApi, Note } from '@/lib/api';
import RequireAuth from '@/components/RequireAuth';

function AdminNotesContent() {
  const [notes, setNotes] = useState<Note[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await notesApi.listAll(page);
      setNotes(res.items);
      setTotalPages(res.totalPages);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load notes');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this note?')) return;
    try {
      await notesApi.delete(id);
      load();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-800">All Notes <span className="text-indigo-600">(Admin)</span></h1>
        <p className="text-gray-500 mt-1">{totalCount} total notes across all users</p>
      </div>
      {error && <p className="text-red-600 mb-4">{error}</p>}
      {loading ? (
        <div className="text-center py-16 text-gray-500">Loading...</div>
      ) : (
        <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Title</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Content Preview</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Tags</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Updated</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {notes.map(note => (
                <tr key={note.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800 max-w-[180px] truncate">{note.title}</td>
                  <td className="px-4 py-3 text-gray-600 max-w-[220px] truncate">{note.content}</td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap gap-1">
                      {note.tags?.slice(0, 3).map(tag => (
                        <span key={tag} className="bg-indigo-100 text-indigo-700 text-xs px-1.5 py-0.5 rounded">{tag}</span>
                      ))}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-gray-400 whitespace-nowrap">{new Date(note.updatedAt).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    <button onClick={() => handleDelete(note.id)} className="text-red-600 hover:underline text-xs">Delete</button>
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
    </div>
  );
}

export default function AdminNotesPage() {
  return <RequireAuth adminOnly><AdminNotesContent /></RequireAuth>;
}
