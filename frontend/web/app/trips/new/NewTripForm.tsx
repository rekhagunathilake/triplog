'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useSession } from 'next-auth/react';
import { entriesApi } from '@/lib/entries-api';
import { ApiError } from '@/lib/api-error';

export default function NewTripForm() {
    const router = useRouter();
    const { data: session } = useSession();

    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!session?.apiToken) {
            setError('You must be signed in to create a trip.');
            return;
        }

        setSubmitting(true);
        setError(null);
        setFieldErrors({});

        try {
            const { id } = await entriesApi.trips.create(
                {
                    title,
                    description: description || undefined,
                    startDate,
                    endDate,
                },
                session.apiToken 
            );
            router.push(`/trips/${id}`);
        } catch (error) {
            if (error instanceof ApiError) {
                if (error.problem.errors) {
                    setFieldErrors(error.problem.errors);
                }
                else{
                    setError(error.problem.detail || error.problem.title || 'Something went wrong.');
                }
            } else {
                setError('An unexpected error occurred - check the console.');
                console.error(error);
            }
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="max-w-lg">
            <h1 className="text-2xl font-semibold">New Trip</h1>

            <form onSubmit={handleSubmit} className="mt-6 space-y-4">
                <Field label="Title" errors={fieldErrors.Title}>
                    <input
                        type="text"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        required
                        className="w-full border rounded px-3 py-2"
                    />
                </Field>

                <Field label="Description">
                    <textarea
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        rows={3}
                        className="w-full border rounded px-3 py-2"
                    />
                </Field>

                <div className="grid grid-cols-2 gap-4">
                    <Field label="Start Date" errors={fieldErrors.StartDate}>
                        <input
                            type="date"
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                            required
                            className="w-full border rounded px-3 py-2"
                        />
                    </Field>
                    <Field label="End Date" errors={fieldErrors.EndDate}>
                        <input
                            type="date"
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                            required
                            className="w-full border rounded px-3 py-2"
                        />
                    </Field>
                </div>

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
                        {submitting ? 'Creating...' : 'Create Trip'}
                    </button>
                    <button
                        type="button"
                        onClick={() => router.back()}
                        className="rounded border px-4 py-2 text-sm hover:bg-neutral-50">
                            Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}

// Small helper component for keeping the form JSX readable
function Field({
    label,
    errors,
    children,
}:{
    label: string;
    errors?: string[];
    children: React.ReactNode;
}) {
    return (
        <label className="block text-sm">
            <span className="mb-1 block font-medium">{label}</span>
            {children}
            {errors && errors.length > 0 && (
                <span className="mt-1 block text-xs text-red-600">{errors.join(', ')}</span>
            )}
        </label>
    );
}

