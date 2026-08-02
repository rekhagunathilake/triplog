import { apiRequest } from "./api-client";

const BASE_URL = process.env.NEXT_PUBLIC_ENTRIES_API_URL;

// DTOs

export type TripStatus = 'Planning' | 'Active' | 'Completed' | 'Archived';
export type EntryStatus = 'Draft' | 'Publishing' | 'Published' | 'Archived';

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

// Client

export const entriesApi = {
    trips: {
        list: () =>
            apiRequest<TripSummary[]>({ baseUrl: BASE_URL!, path: '/trips' }),

        getById: (id: string) =>
            apiRequest<Trip>({ baseUrl: BASE_URL!, path: `/trips/${id}` }),

        create: (input: CreateTripInput) =>
            apiRequest<{ id: string }>({
                baseUrl: BASE_URL!,
                path: '/trips',
                json: input,
            }),

            activate: (id: string) =>
                apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/activate` , method: 'POST' }),

            complete: (id: string) =>
                apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/complete` , method: 'POST' }),

            archive: (id: string) =>
                apiRequest({ baseUrl: BASE_URL!, path: `/trips/${id}/archive` , method: 'POST' }),
    },
};