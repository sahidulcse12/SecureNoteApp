'use client';

import { useEffect, useState, useCallback } from 'react';
import { notesApi, Note } from '@/lib/api';
import RequireAuth from '@/components/RequireAuth';

function NoteModal({
  note,
  onClose,
  onSave,
}: {
  note: Partial<Note> | null;
  onClose: () => void;
  onSave: () => void;
}) {
  const [title, setTitle] = useState(note?.title || '');
  const [content, setContent] = useState(note?.content || '');
  const [tags, setTags] = useState((note?.tags || []).join(', '));
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    if (!title.trim()) return setError('Title is required');
    setSaving(true);
    setError('');
    try {
      const tagList = tags.split(',').map(t => t.trim()).filter(Boolean);
      if (note?.id) {
        await notesApi.update(note.id, { title, content, tags: tagList });
      } else {
        await notesApi.create({ title, content, tags: tagList });
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
      <div className="bg-white rounded-xl shadow-xl w-full max-w-lg">
        <div className="p-6">
          <h2 className="text-xl font-bold mb-4">{note?.id ? 'Edit Note' : 'New Note'}</h2>
          <div className="space-y-3">
            <input
              placeholder="Title"
              value={title}
              onChange={e => setTitle(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
            />
            <textarea
              placeholder="Content"
              value={content}
              onChange={e => setContent(e.target.value)}
              rows={5}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none resize-none"
            />
            <input
              placeholder="Tags (comma separated)"
              value={tags}
              onChange={e => setTags(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
            />
          </div>
          {error && <p className="text-red-600 text-sm mt-2">{error}</p>}
          <div className="flex justify-end gap-3 mt-4">
            <button onClick={onClose} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg">Cancel</button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50"
            >
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function NotesContent() {
  const [notes, setNotes] = useState<Note[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [modalNote, setModalNote] = useState<Partial<Note> | null | undefined>(undefined);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await notesApi.list(page);
      setNotes(res.items);
      setTotalPages(res.totalPages);
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
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">My Notes</h1>
        <button
          onClick={() => setModalNote({})}
          className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700"
        >
          + New Note
        </button>
      </div>
      {error && <p className="text-red-600 mb-4">{error}</p>}
      {loading ? (
        <div className="text-center py-16 text-gray-500">Loading notes...</div>
      ) : notes.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <p className="text-xl mb-2">No notes yet</p>
          <p>Click &quot;+ New Note&quot; to create your first note.</p>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {notes.map(note => (
            <div key={note.id} className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-gray-800 mb-2 truncate">{note.title}</h3>
              <p className="text-gray-600 text-sm line-clamp-3 mb-3">{note.content}</p>
              {note.tags?.length > 0 && (
                <div className="flex flex-wrap gap-1 mb-3">
                  {note.tags.map(tag => (
                    <span key={tag} className="bg-indigo-100 text-indigo-700 text-xs px-2 py-0.5 rounded-full">{tag}</span>
                  ))}
                </div>
              )}
              <div className="flex justify-between items-center">
                <span className="text-xs text-gray-400">{new Date(note.updatedAt).toLocaleDateString()}</span>
                <div className="flex gap-2">
                  <button onClick={() => setModalNote(note)} className="text-xs text-indigo-600 hover:underline">Edit</button>
                  <button onClick={() => handleDelete(note.id)} className="text-xs text-red-600 hover:underline">Delete</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
      {/* Pagination */}
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
      {modalNote !== undefined && (
        <NoteModal note={modalNote} onClose={() => setModalNote(undefined)} onSave={load} />
      )}
    </div>
  );
}

export default function NotesPage() {
  return <RequireAuth><NotesContent /></RequireAuth>;
}
