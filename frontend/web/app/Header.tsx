'use client';

import Link from 'next/link';
import { useSession, signIn, signOut } from 'next-auth/react';

export default function Header() {
    const { data: session, status } = useSession();

    return (
        <header className="border-b bg-white">
            <div className="mx-auto flex max-w-4xl items-center justify-between px-6 py-4">
                <Link href="/" className="text-x1 font-semibold">
                    triplog
                </Link>
                <nav className="flex items-center gap-4 text-sm">
                    <Link href="/" className="hover:underline">Trips</Link>
                    {session && (
                        <Link href="/trips/new" className="hover:underline">New trip</Link>
                    )}
                    {status === 'loading' ? (
                        <span className="text-xs text-neutral-400">…</span>
                    ) : session ? (
                        <div className="flex items-center gap-3">
                        <span className="text-xs text-neutral-500">{session.user?.email}</span>
                        <button
                            onClick={() => signOut()}
                            className="rounded border px-3 py-1 text-xs hover:bg-neutral-50"
                        >
                            Sign out
                        </button>
                        </div>
                    ) : (
                        <button
                        onClick={() => signIn('google')}
                        className="rounded bg-neutral-900 px-3 py-1 text-xs text-white hover:bg-neutral-800"
                        >
                        Sign in
                        </button>
                    )}
                </nav>
            </div>
        </header>
    );
}