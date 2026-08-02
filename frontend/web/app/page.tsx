import Link from 'next/link';
import { entriesApi } from '@/lib/entries-api';

export default async function HomePage() {
  let trips;
  try {
    trips = await entriesApi.trips.list();
  } catch (error) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
        <p className="font-medium">Couldn't load trips.</p>
        <p className="mt-1 text-sm">
          Check that entries-api is running at{' '}
          <code className="text-xs">{process.env.NEXT_PUBLIC_ENTRIES_API_URL}</code>.
        </p>
      </div>
    );
  }

  if (trips.length === 0) {
    return (
      <div className="text-center">
        <h1 className="text-2xl font-semibold">No trips yet</h1>
        <p className="mt-2 text-neutral-600">Plan your first one.</p>
        <Link
          href="/trips/new"
          className="mt-4 inline-block rounded bg-neutral-900 px-4 py-2 text-white hover:bg-neutral-800"
        >
          Create trip
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Your trips</h1>
      <ul className="grid gap-3">
        {trips.map((trip) => (
          <li key={trip.id}>
            <Link
              href={`/trips/${trip.id}`}
              className="block rounded-lg border bg-white p-4 hover:border-neutral-400"
            >
              <div className="flex items-center justify-between">
                <h2 className="font-medium">{trip.title}</h2>
                <span className={statusBadgeClass(trip.status)}>{trip.status}</span>
              </div>
              <p className="mt-1 text-sm text-neutral-600">
                {trip.startDate} → {trip.endDate}
              </p>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}

function statusBadgeClass(status: string) {
  const base = 'rounded-full px-2 py-0.5 text-xs font-medium';
  switch (status) {
    case 'Planning': return `${base} bg-blue-100 text-blue-700`;
    case 'Active': return `${base} bg-green-100 text-green-700`;
    case 'Completed': return `${base} bg-purple-100 text-purple-700`;
    case 'Archived': return `${base} bg-neutral-100 text-neutral-600`;
    default: return `${base} bg-neutral-100 text-neutral-700`;
  }
}