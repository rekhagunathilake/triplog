'use client'

import { useRouter } from 'next/navigation';
import { useState, type FormEvent, use } from 'react';
import { useSession } from 'next-auth/react';
import { entriesApi } from '@/lib/entries-api';
import { ApiError } from '@/lib/api-error';

export default function NewEntryPage({
    params,
}: {
    params: Promise<{ id: string }>;
}) {
    const { id: tripId } = use(params);
    const router = useRouter();
    const { data: session } = useSession();

    const [title, setTitle] = useState('');
    const [body, setBody] = useState('');
    const [visitedOn, setVisitedOn] = useState('');

    // Location group - all or nothing per the backend validator
    const [locationName, setLocationName] = useState('');
    const [latitude, setLatitude] = useState('');
    const [longitude, setLongitude] = useState('');

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        if (!session?.apiToken) {
            setError('You must be signed in to create an entry.');
            return;
        }
        setSubmitting(false);
        setError(null);
        setFieldErrors({});

        const hasAnyLocation = locationName || latitude || longitude;
        const hasFullLocation = locationName && latitude && longitude;

        if (hasAnyLocation && !hasFullLocation) {
            setError('Location requires name, latitude, and longitude - or leave all blank.');
            setSubmitting(false);
            return;
        }

        try {
            const { id } = await entriesApi.entries.create(tripId, {
                title,
                body,
                visitedOn,
                locationName: hasFullLocation ? locationName : undefined,
                latitude: hasFullLocation ? Number(latitude) : undefined,
                longitude: hasFullLocation ? Number(longitude) : undefined,
            }, session.apiToken);
            router.push(`/entries/${id}`);
        }
        catch (error) {
            if (error instanceof ApiError) {
                if (error.problem.errors) {
                    setFieldErrors(error.problem.errors);
                } else {
                    setError(error.problem.detail || error.problem.title || 'Somthing went wrong.');
                }
            } else {
                setError('Unexpected error - check the console.');
                console.error(error);
            }
            setSubmitting(false);
        }
    }

    return (
        <div className="max-w-lg">
            <h1 className="text-2xl font-semibold">New entry</h1>

            <form onSubmit={handleSubmit} className='mt-6 space-y-4'>
                <Field label="Title" errors={fieldErrors.Title}>
                    <input type="text" value={title} onChange={e => setTitle(e.target.value)}
                    required
                    className='w-full rounded border px-3 py-2'
                    />
                </Field>

                <Field label="Body" errors={fieldErrors.Body}>
                <textarea
                    value={body}
                    onChange={e => setBody(e.target.value)}
                    required
                    rows={6}
                    className="w-full rounded border px-3 py-2"
                />
                </Field>

                <Field label="Visited on" errors={fieldErrors.VisitedOn}>
                <input
                    type="date"
                    value={visitedOn}
                    onChange={e => setVisitedOn(e.target.value)}
                    required
                    className="w-full rounded border px-3 py-2"
                />
                </Field>

                <fieldset className="rounded border p-4">
                <legend className="px-1 text-sm font-medium">Location (optional)</legend>

                <div className="mt-2 space-y-3">
                    <Field label="Place name" errors={fieldErrors.LocationName}>
                    <input
                        type="text"
                        value={locationName}
                        onChange={e => setLocationName(e.target.value)}
                        placeholder="Paris, France"
                        className="w-full rounded border px-3 py-2"
                    />
                    </Field>

                    <div className="grid grid-cols-2 gap-3">
                    <Field label="Latitude" errors={fieldErrors.Latitude}>
                        <input
                        type="number"
                        step="any"
                        value={latitude}
                        onChange={e => setLatitude(e.target.value)}
                        placeholder="48.8566"
                        className="w-full rounded border px-3 py-2"
                        />
                    </Field>
                    <Field label="Longitude" errors={fieldErrors.Longitude}>
                        <input
                        type="number"
                        step="any"
                        value={longitude}
                        onChange={e => setLongitude(e.target.value)}
                        placeholder="2.3522"
                        className="w-full rounded border px-3 py-2"
                        />
                    </Field>
                    </div>
                </div>
                </fieldset>

                {error && (
                <div className="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                    {error}
                </div>
                )}

                <div className="flex gap-2 pt-2">
                <button
                    type="submit"
                    disabled={submitting}
                    className="rounded bg-neutral-900 px-4 py-2 text-sm font-medium text-white hover:bg-neutral-800 disabled:opacity-50"
                >
                    {submitting ? 'Creating…' : 'Create entry'}
                </button>
                <button
                    type="button"
                    onClick={() => router.back()}
                    className="rounded border px-4 py-2 text-sm hover:bg-neutral-50"
                >
                    Cancel
                </button>
                </div>
            </form>
        </div>
    );
}

function Field({
    label,
    errors,
    children
}: {
    label: string;
    errors?: string[];
    children: React.ReactNode;
}) {
    return (
        <label className="block text-sm">
            <span className='mb-1 block font-medium'>{label}</span>
            {children}
            {errors && errors.length > 0 && (
                <span className='mt-1 block text-xs text-red-600'>{errors.join(' ')}</span>
            )}
        </label>
    );
}