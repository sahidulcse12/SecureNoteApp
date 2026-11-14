'use client';

import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export default function RequireAuth({
  children,
  adminOnly = false,
}: {
  children: React.ReactNode;
  adminOnly?: boolean;
}) {
  const { user, loading, isAdmin } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading) {
      if (!user) router.replace('/login');
      else if (adminOnly && !isAdmin) router.replace('/notes');
    }
  }, [user, loading, isAdmin, adminOnly, router]);

  if (loading || !user) return (
    <div className="flex justify-center items-center min-h-[200px]">
      <div className="text-gray-500">Loading...</div>
    </div>
  );
  if (adminOnly && !isAdmin) return null;
  return <>{children}</>;
}
