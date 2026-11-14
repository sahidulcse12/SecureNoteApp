'use client';

import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import Link from 'next/link';

export default function HomePage() {
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && user) router.replace('/notes');
  }, [user, loading, router]);

  return (
    <div className="flex flex-col items-center justify-center min-h-[70vh] text-center">
      <h1 className="text-5xl font-bold text-indigo-700 mb-4">SecureNote</h1>
      <p className="text-gray-600 text-xl mb-8 max-w-lg">
        A secure note-taking app with role-based access, public posts, and smart insights.
      </p>
      <div className="flex gap-4">
        <Link href="/login" className="bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 font-medium">
          Sign In
        </Link>
        <Link href="/register" className="border border-indigo-600 text-indigo-600 px-6 py-3 rounded-lg hover:bg-indigo-50 font-medium">
          Register
        </Link>
      </div>
    </div>
  );
}
