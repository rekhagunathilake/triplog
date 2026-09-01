'use client'

import { useState, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useSession } from 'next-auth/react';
import { useUpload } from '@/lib/use-upload';

export function MediaUpload({ entryId }: { entryId: string }) {
    const router = useRouter();
    const inputRef = useRef<HTMLInputElement>(null);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const { state, progress, error, upload, reset, uploading } = useUpload();
    const { data: session } = useSession();

    async function handleUpload() {
        if (!selectedFile) return;
        if (!session?.apiToken) {
            return (
                <div className="rounded border bg-neutral-50 p-4 text-sm text-neutral-600">
                    Sign in to upload media.
                </div>
            );
            //return;
        }

        try {
            await upload(selectedFile, entryId, session.apiToken);
            // Refresh the Server Component so the new media appears in the list
            router.refresh();
            setSelectedFile(null);
            if (inputRef.current) inputRef.current.value = '';
            // Give the "done" state a moment to show, then reset
            setTimeout(() => reset(), 800);
        } catch {
            // useUpload already captured the error into its state
        }
    }

    return (
        <div className="rounded border bg-white p-4">
            <div className="flex items-center gap-3">
                <input
                    ref={inputRef}
                    type="file"
                    accept='image/jpeg,image/png,image/webp'
                    onChange={e => setSelectedFile(e.target.files?.[0] ?? null)}
                    disabled={uploading}
                    className="rounded bg-neutral-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-neutral-800 disabled:opacity-50"
                />
                <button
                    onClick={handleUpload}
                    disabled={!selectedFile || uploading}
                    className='rounded bg-neutral-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-neutral-800 disabled: opacity-50'
                >
                    {stateLabel(state)}
                </button>
            </div>

            {uploading && (
                <div className='mt-3'>
                    <div className='h-2 overflow-hidden rounded bg-neutral-100'>
                        <div className='h-full bg-neutral-900 transition-all duration-100' style={{ width: `${progress}%`}}
                        />
                    </div>
                    <p className='mt-1 text-xs text-neutral-600'>
                        {state === 'requesting' && 'Requesting upload URL...'}
                        {state === 'uploading' && `Uploading... ${progress}%`}
                        {state === 'attaching' && 'Attaching to entry...'}
                    </p>
                </div>
            )}

            {state === 'done' && (
                <p className='mt-3 text-sm text-green-700'>Uploaded and attached.</p>
            )}

            {error && (
                <div className='mt-3 rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700'>
                    {error}
                </div>
            )}
        </div>
    );
}

function stateLabel(state: string) {
    switch (state) {
        case 'requesting': return 'Starting...';
        case 'uploading': return 'Uploading...';
        case 'attaching': return 'Attaching...';
        case 'done': return 'Done';
        default: return 'Upload';
    }
}