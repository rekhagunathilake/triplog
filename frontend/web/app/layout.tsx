import Link from 'next/link';
import { Fraunces, Inter } from 'next/font/google';
import './globals.css';

const fraunces = Fraunces({
  subsets: ['latin'],
  variable: '--font-serif',
  display: 'swap',
  axes: ['SOFT', 'opsz'],   // enable soft-serif variant + optical-size
});

const inter = Inter({
  subsets: ['latin'],
  variable: '--font-sans',
  display: 'swap',
});

export const metadata = {
  title: 'triplog',
  description: 'A travel journal',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${fraunces.variable} ${inter.variable}`}>
      <body className="min-h-screen bg-background text-foreground antialiased">
        <header className="mx-auto max-w-7xl px-8 pt-10 pb-6">
          <div className="flex items-baseline justify-between">
            <Link href="/" className="font-serif text-3xl tracking-tight">
              triplog
            </Link>
            <nav className="flex gap-8 text-xs uppercase tracking-widest text-muted-foreground">
              <Link href="/" className="hover:text-foreground transition-colors">
                Journeys
              </Link>
              <Link href="/trips/new" className="hover:text-foreground transition-colors">
                New trip
              </Link>
            </nav>
          </div>
        </header>
        <main className="mx-auto max-w-7xl px-8 pb-16">{children}</main>
        <footer className="mx-auto max-w-7xl px-8 py-10 text-xs uppercase tracking-widest text-muted-foreground">
          <div className="flex items-baseline justify-between border-t pt-6">
            <span>© 2026 Triplog</span>
            <div className="flex gap-6">
              <Link href="/about" className="hover:text-foreground">About</Link>
              <Link href="/contact" className="hover:text-foreground">Contact</Link>
            </div>
          </div>
        </footer>
      </body>
    </html>
  );
}