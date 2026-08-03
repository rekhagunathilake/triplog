import { notFound } from "next/navigation";
import { entriesApi } from "@/lib/entries-api";
import { ApiError } from "@/lib/api-error";
import { TripActions } from "./TripActions";

export default async function TripPage({
    params,
}: {
    params: Promise<{ id: string }>;
}) {
    const { id } = await params;

    let trip;
    try {
        trip = await entriesApi.trips.getById(id);
    } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
            notFound();
        }
        throw error;
    }

    return (
        <div className="space=y-6">
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
