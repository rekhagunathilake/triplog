'use client';

import { useRouter } from 'next/navigation';
import { useState, useTransition } from 'react';
import { entriesApi, type TripStatus } from '@/lib/entries-api';
import { ApiError } from '@/lib/api-error';

type Action = 'activate' | 'complete' | 'archive';

export function TripActions({
    tripId,
    status,
}: {
    tripId: string;
    status: TripStatus;
}) {
    const router = useRouter();
    const [pending, startTransition] = useTransition();
    const [error, setError] = useState<string | null>(null);

    async function runAction(action: Action) {
        setError(null);

        try {
            if (action === 'activate') await entriesApi.trips.activate(tripId);
            else if (action === 'complete') await entriesApi.trips.complete(tripId);
            else if (action === 'archive') await entriesApi.trips.archive(tripId);

            // Refresh the Server Component tree so the new status renders
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
const canActivate = status === 'Planning';
const canComplete = status === 'Active';
const canArchive = status !== 'Archived';

return (
    <div className="space-y-3">
        <div className="flex gap-2">
            {canActivate && (
                <ActionButton onClick={() => runAction('activate')} disabled={pending}>
                    Activate
                </ActionButton>
            )}
            {canComplete && (
                <ActionButton onClick={() => runAction('complete')} disabled={pending}>
                    Complete
                </ActionButton>
            )}
            {canArchive && (
                <ActionButton onClick={() => runAction('archive')} disabled={pending}>
                    Archive
                </ActionButton>
            )}
        </div>

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