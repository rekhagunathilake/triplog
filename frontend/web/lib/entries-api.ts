import { apiRequest } from "./api-client";

const BASE_URL = process.env.NEXT_PUBLIC_ENTRIES_API_URL;

// DTOs

export type TripStatus = 'Planning' | 'Active' | 'Completed' | 'Archived';

export interface TripSummary {
    id: string;
    title: string;
    startDate: string;
    endDate: string;
    status: TripStatus;
}

export interface Trip {
    id: string;
    ownerId: string;
    title: string;
    description: string | null;
    startDate: string;
    endDate: string;
    status: TripStatus;
    createdAtUtc: string;
    archivedAtUtc: string | null;
}

export interface CreateTripInput {
    title: string;
    description?: string;
    startDate: string;
    endDate: string;
}

export type EntryStatus = 'Draft' | 'Publishing' | 'Published' | 'Archived';

export interface EntrySummary {
    id: string;
    tripId: string;
    title: string;
    visitedOn: string;
    status: EntryStatus;
    mediaCount: number;
}

export interface Location {
    name: string;
    latitude: number;
    longitude: number;
}

export interface MediaReference {
    id: string;
    displayOrder: number;
}

export interface Entry {
    id: string;
    tripId: string;
    ownerId: string;
    title: string;
    body: string;
    location: Location | null;
    visitedOn: string;
    status: EntryStatus;
    mediaReferences: MediaReference[];
    createdAtUtc: string;
    publishedAtUtc: string | null;
    archivedAtUtc: string | null;
    lastPublishFailReason: string | null;
}

export interface CreateEntryInput {
    title: string;
    body: string;
    visitedOn: string;
    locationName?: string;
    latitude?: number;
    longitude?: number;
}

// Client

export const entriesApi = {
    trips: {
        list: () =>
            apiRequest<TripSummary[]>({ baseUrl: BASE_URL!, path: '/trips' }),

        getById: (id: string) =>
            apiRequest<Trip>({ baseUrl: BASE_URL!, path: `/trips/${id}` }),

        create: (input: CreateTripInput, token: string) =>
            apiRequest<{ id: string }>({
                baseUrl: BASE_URL!,
                path: '/trips',
                json: input,
                token,
            }),

        activate: (id: string, token: string) =>
            apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/activate` , method: 'POST', token }),
        complete: (id: string, token: string) =>
            apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/complete` , method: 'POST', token }),

        archive: (id: string, token: string) =>
            apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/archive` , method: 'POST', token }),
    },
    entries: {
        listByTrip: (tripId: string) =>
            apiRequest<EntrySummary[]>({ baseUrl: BASE_URL!, path: `/trips/${tripId}/entries`}),
        
        getById: (id: string) =>
            apiRequest<Entry>({ baseUrl: BASE_URL!, path: `/entries/${id}` }),

        create: (tripId: string, input: CreateEntryInput, token: string) =>
            apiRequest<{ id: string }>({
                baseUrl: BASE_URL!,
                path: `/trips/${tripId}/entries`,
                json: input,
                token,
            }),

        attachMedia: (entryId: string, mediaId: string, token: string) =>
            apiRequest({
                baseUrl: BASE_URL!,
                path: `/entries/${entryId}/media/${mediaId}`,
                method: 'POST',
                token,
            }),

        removeMedia: (entryId: string, mediaId: string, token: string) =>
            apiRequest({
                baseUrl: BASE_URL!,
                path: `/entries/${entryId}/media/${mediaId}`,
                method: 'DELETE',
                token,
            }),

        publish: (id: string, token: string) =>
            apiRequest({ baseUrl: BASE_URL!, path: `/entries/${id}/publish` , method: 'POST', token }),

        archive: (id: string, token: string) =>
            apiRequest({ baseUrl: BASE_URL!, path: `/entries/${id}/archive` , method: 'POST', token }),
    },
};