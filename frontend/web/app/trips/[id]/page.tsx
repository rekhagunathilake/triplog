import Link from "next/link";
import { auth } from '@/auth';
import { notFound } from "next/navigation";
import { entriesApi, type EntrySummary } from "@/lib/entries-api";
import { ApiError } from "@/lib/api-error";
import { TripActions } from "./TripActions";
import { getTripMediaUrls, type TripMediaItem } from "@/lib/trip-media";

export default async function TripPage({
    params,
}: {
    params: Promise<{ id: string }>;
}) {
    const { id } = await params;
    const session = await auth();

    let trip;
    let entries: EntrySummary[];
    let galleryItems: TripMediaItem[];

    try {
        [trip, entries, galleryItems] = await Promise.all([
            entriesApi.trips.getById(id),
            entriesApi.entries.listByTrip(id),
            await getTripMediaUrls(id)
        ]);

    } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
            notFound();
        }
        throw error;
    }

    return (
        <div className="space=y-8">
            <header className="flex items-start justify-between">
                <div>
                    <h1 className="text-2xl font-semibold">{trip.title}</h1>
                    <p className="mt-1 text-sm text-neutral-600">
                        {trip.startDate} → {trip.endDate}
                    </p>
                    {trip.description && (
                        <p className="mt-3 text-neutral-700">{trip.description}</p>
                    )}
                </div>
                <span className={statusBadgeClass(trip.status)}>{trip.status}</span>
            </header>

            <TripActions tripId={trip.id} status={trip.status} />

            <section>
                <div className="mb-3 flex items-center justify-between">
                    <h2 className="text-lg font-medium">Entries</h2>
                    {session && (
                        <Link
                            href={`/trips/${trip.id}/entries/new`}
                            className="text-sm text-blue-600 hover:underline"
                        >
                            New Entry
                        </Link>
                    )}
                </div>

                {entries.length === 0 ? (
                    <p className="text-sm text-neutral-500">No entries yet.</p>
                ) : (
                    <ul className="grid gap-2">
                        {entries.map((entry) => (
                            <li key={entry.id}>
                                <Link
                                    href={`/entries/${entry.id}`}
                                    className="block rounded border bg-white p-3 hover:border-neutral-400"
                                    >
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <p className="font-medium">{entry.title}</p>
                                                <p className="text-xs text-neutral-500">
                                                    {entry.visitedOn} · {entry.mediaCount} media
                                                </p>
                                            </div>
                                            <span className={entryBadgeClass(entry.status)}>
                                                {entry.status}
                                            </span>
                                        </div>
                                </Link>
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            {galleryItems.length > 0 &&  (
                <section>
                    <h2 className="mb-3 text-lg font-medium">
                        Gallery <span className="text-sm font-normal text-neutral-500">({galleryItems.length})</span>
                    </h2>
                    <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-4">
                        {galleryItems.map(item => (
                            <Link
                            key={item.mediaId}
                            href={`/entries/${item.entryId}`}
                            className="group relative aspect-square overflow-hidden rounded-lg border bg-neutral-100"
                            >
                                <img
                                    src={item.url}
                                    alt={item.entryTitle}
                                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                                    loading="lazy"
                                />
                                <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 to-transparent p-3 opacity-0 transition-opacity group-hover:opacity-100">
                                    <p className="text-xs font-medium text-white">{item.entryTitle}</p>
                                    <p className="text-xs text-white/80">{item.visitedOn}</p>
                                </div>
                            </Link>
                        ))}
                    </div>
                </section>
            )}
        </div>
    );
}

function statusBadgeClass(status: string) {
    const base = 'rounded-full px-3 py-1 text-xs font-medium';
    switch (status) {
        case 'Planning': return `${base} bg-blue-100 text-blue-700`;
        case 'Active': return `${base} bg-green-100 text-green-700`;
        case 'Completed': return `${base} bg-purple-100 text-purple-700`;
        case 'Archived': return `${base} bg-neutral-100 text-neutral-600`;
        default: return `${base} bg-neutral-100 text-neutral-700`;
    }
}

function entryBadgeClass(status: string) {
    const base = 'rounded-full px-2 py-0.5 text-xs font-medium';
    switch (status) {
        case 'Draft': return `${base} bg-neutral-100 text-neutral-700`;
        case 'Publishing': return `${base} bg-amber-100 text-amber-700`;
        case 'Published': return `${base} bg-green-100 text-green-700`;
        case 'Archived': return `${base} bg-neutral-100 text-neutral-500`;
        default: return `${base} bg-neutral-100 text-neutral-700`;
    }
}
