import Link from 'next/link';
import { entriesApi } from '@/lib/entries-api';
import { getTripHeroUrl } from '@/lib/trip-media';

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

  // Fetch a hero URL for each trip — in parallel
  const heroUrls = await Promise.all(trips.map(t => getTripHeroUrl(t.id)));
  const tripsWithHero = trips.map((trip, i) => ({ ...trip, heroUrl: heroUrls[i] }));

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold">Your trips</h1>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {tripsWithHero.map(trip => (
          <Link
            key={trip.id}
            href={`/trips/${trip.id}`}
            className="group relative aspect-[4/3] overflow-hidden rounded-lg border bg-neutral-100"
          >
            {trip.heroUrl ? (
              <img
                src={trip.heroUrl}
                alt={trip.title}
                className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                loading="lazy"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-neutral-100 to-neutral-200 text-neutral-400">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 4.5h15A1.5 1.5 0 0 1 21 6v12a1.5 1.5 0 0 1-1.5 1.5h-15A1.5 1.5 0 0 1 3 18V6a1.5 1.5 0 0 1 1.5-1.5Zm0 0v13.5m0 0 5.25-5.25a1.5 1.5 0 0 1 2.121 0l4.379 4.379M21 15l-3.879-3.879a1.5 1.5 0 0 0-2.121 0L13.5 12.75" />
                </svg>
              </div>
            )}
            <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/80 via-black/40 to-transparent p-4 text-white">
              <h2 className="font-medium">{trip.title}</h2>
              <p className="mt-0.5 text-xs text-white/80">
                {trip.startDate} → {trip.endDate}
              </p>
              <span className={`${statusBadgeClass(trip.status)} mt-2 inline-block`}>
                {trip.status}
              </span>
            </div>
          </Link>
        ))}
      </div>
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