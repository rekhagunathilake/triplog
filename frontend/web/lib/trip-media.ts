import { entriesApi } from "./entries-api";
import { mediaApi } from "./media-api";

export interface TripMediaItem {
    mediaId: string;
    url: string;
    entryId: string;
    entryTitle: string;
    visitedOn: string;
    displayOrder: number;
}

/**
 * Fetches media URLs for every entry in a trip, sorted newest-first.
 * Optionally limits to the first N items for hero/preview use cases.
 */
export async function getTripMediaUrls(
    tripId: string,
    limit?: number
): Promise<TripMediaItem[]> {
    // 1. List entries in the trip
    const summaries = await entriesApi.entries.listByTrip(tripId);

    // 2. Load full entries (need mediaReference) - in parallel
    const entries = await Promise.all(
        summaries.map(e => entriesApi.entries.getById(e.id))
    );

    // 3. Flatten to (mediaRef + parent entry) records
    const refs = entries.flatMap(entry =>
        entry.mediaReferences.map(m => ({
            mediaId: m.id,
            displayOrder: m.displayOrder,
            entryId: entry.id,
            entryTitle: entry.title,
            visitedOn: entry.visitedOn,
        }))
    );

    // 4. Sort - newest entry first, then by display order within the entry
    const sorted = refs.sort((a,b) => {
        if (a.visitedOn !== b.visitedOn) return b.visitedOn.localeCompare(a.visitedOn);
        return a.displayOrder - b.displayOrder;
    });

    // 5. Optionally cap before making the download-url calls (bandwidth win)
    const capped = limit ? sorted.slice(0, limit) : sorted;

    // 6. Fetch presigned download URLs in parallel
    const items = await Promise.all(
        capped.map(async ref => {
            const { url } = await mediaApi.getDownloadUrl(ref.mediaId);
            return { ...ref, url };
        })
    );

    return items;
}

/**
 * Convenience: just the first media URL for a trip, for hero/thumbnail use.
 * Returns null if the trip has no media yet.
 */
export async function getTripHeroUrl(tripId: string): Promise<string | null> {
    const [first] = await getTripMediaUrls(tripId, 1);
    return first?.url ?? null;
}