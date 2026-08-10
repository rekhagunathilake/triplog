'use client'

import { useState, useEffect, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { entriesApi, type EntryStatus } from '@/lib/entries-api';
import { ApiError } from '@/lib/api-error';

type Action = 'publish' | 'archive';

export function EntryActions({
    entryId,
    status
 }: {
    entryId: string, 
    status: EntryStatus
}) {
    const router = useRouter();
    const [pending, startTransition] = useTransition();
    const [error, setError] = useState<string | null>(null);

    // Auto-refresh while the saga runs
    useEffect(() => {
        if (status !== 'Publishing') return;

        const interval = setInterval(() => {
            router.refresh();
        }, 2000);

        return () => clearInterval(interval);
    }, [status, router]);

    async function runAction(action: Action) {
            setError(null);           
                   
            try { 
                if (action === 'publish') entriesApi.entries.publish(entryId);
                else if (action === 'archive') entriesApi.entries.archive(entryId);

                startTransition(() => {
                router.refresh();
                });
            } catch (error) {
                if (error instanceof ApiError) {
                    setError(error.problem.detail || error.problem.title || 'Action failed.');
                } else {
                    setError('An unexpected error occurred - check the console.');
                    console.error(error);
                }
        }
    }
    
    // Choose which buttons to show based on the current state
    const canPublish = status === 'Draft';
    const canArchive = status !== 'Archived';
    
    return (
        <div className="space-y-3">
            <div className="flex gap-2">
                {canPublish && (
                    <ActionButton onClick={() => runAction('publish')} disabled={!canPublish}>
                        Publish
                    </ActionButton>
                )}
                {canArchive && (
                    <ActionButton onClick={() => runAction('archive')} disabled={!canArchive}>
                        Archive
                    </ActionButton>
                )}
            </div>

            {status === 'Publishing' && (
                <p className="text-sm text-amber-700">
                    Publishing… waiting for media to finalize.
                </p>
            )}

            {status === 'Published' && (
                <p className="text-sm text-green-700">Published successfully.</p>
            )}
    
            {error && (
                <div className="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                    {error}
                </div>
            )}
            </div>
    );
    }
    
    function ActionButton({
        onClick,
        disabled,
        variant = 'default',
        children,
    }: {
        onClick: () => void;
        disabled?: boolean;
        variant?: 'default' | 'danger';
        children: React.ReactNode;
    }) {
        const base = 'rounded px-3 py-1.5 text-sm font-medium disabled:opacity-50';
        const style =
            variant === 'danger'
                ? 'border border-red-200 text-red-700 hover:bg-red-50'
                : 'bg-neutral-900 text-white hover:bg-neutral-800';
        return (
            <button onClick={onClick} disabled={disabled} className={`${base} ${style}`}>
                {children}
            </button>
        );
    }