import Link from 'next/link';
import './globals.css';

export const metadata = {
  title: 'triplog',
  description: 'A travel journal',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-neutral-50 text-neutral-900 antialiased">
        <header className="border-b bg-white">
          <div className="mx-auto flex max-w-4xl items-center justify-between px-6 py-4">
            <Link href="/" className="text-xl font-semibold">
              triplog
            </Link>
            <nav className="flex gap-4 text-sm">
              <Link href="/" className="hover:underline">Trips</Link>
              <Link href="/trips/new" className="hover:underline">New trip</Link>
            </nav>
          </div>
        </header>
        <main className="mx-auto max-w-4xl px-6 py-8">{children}</main>
      </body>
    </html>
  );
}