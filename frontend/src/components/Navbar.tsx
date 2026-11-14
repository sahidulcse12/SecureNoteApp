'use client';

import Link from 'next/link';
import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';

export default function Navbar() {
  const { user, logout, isAdmin } = useAuth();
  const router = useRouter();

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <nav className="bg-indigo-700 text-white px-6 py-3 flex items-center justify-between shadow-md">
      <div className="flex items-center gap-6">
        <Link href="/" className="font-bold text-xl tracking-tight">SecureNote</Link>
        {user && (
          <>
            <Link href="/notes" className="hover:text-indigo-200 text-sm">Notes</Link>
            <Link href="/posts" className="hover:text-indigo-200 text-sm">Posts</Link>
            <Link href="/aggregations" className="hover:text-indigo-200 text-sm">Insights</Link>
            {isAdmin && (
              <>
                <Link href="/admin/users" className="hover:text-indigo-200 text-sm font-medium">Admin: Users</Link>
                <Link href="/admin/notes" className="hover:text-indigo-200 text-sm font-medium">Admin: Notes</Link>
              </>
            )}
          </>
        )}
      </div>
      <div className="flex items-center gap-4">
        {user ? (
          <>
            <span className="text-sm text-indigo-200">{user.fullName} ({user.role})</span>
            <button onClick={handleLogout} className="bg-indigo-500 hover:bg-indigo-400 px-3 py-1 rounded text-sm">Logout</button>
          </>
        ) : (
          <>
            <Link href="/login" className="hover:text-indigo-200 text-sm">Login</Link>
            <Link href="/register" className="bg-white text-indigo-700 hover:bg-indigo-100 px-3 py-1 rounded text-sm font-medium">Register</Link>
          </>
        )}
      </div>
    </nav>
  );
}
