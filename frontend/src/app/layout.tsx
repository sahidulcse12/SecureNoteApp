import type { Metadata } from 'next';
import './globals.css';
import { AuthProvider } from '@/context/AuthContext';
import Navbar from '@/components/Navbar';

export const metadata: Metadata = {
  title: 'SecureNote App',
  description: 'A secure note-taking application with role-based access control',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: 'Arial, Helvetica, sans-serif' }}>
        <AuthProvider>
          <Navbar />
          <main className="max-w-6xl mx-auto px-4 py-8">{children}</main>
        </AuthProvider>
      </body>
    </html>
  );
}
