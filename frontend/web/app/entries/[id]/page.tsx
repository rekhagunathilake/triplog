import Link from 'next/link';
import { notFound } from 'next/navigation';
import { entriesApi } from '@/lib/entries-api';
import { ApiError } from '@/lib/api-error';
import { MediaUpload } from './MediaUpload';
import { EntryActions } from './EntryActions';

export default async function EntryPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  let entry;
  try {
    entry = await entriesApi.entries.getById(id);
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) {
      notFound();
    }
    throw err;
  }

  return (
    <div className="space-y-6">
      <header>
        <Link
          href={`/trips/${entry.tripId}`}
          className="text-sm text-blue-600 hover:underline"
        >
          ← Back to trip
        </Link>
        <h1 className="mt-2 text-2xl font-semibold">{entry.title}</h1>
        <p className="mt-1 text-sm text-neutral-600">
          {entry.visitedOn}
          {entry.location && ` · ${entry.location.name}`}
        </p>
        <span className={entryStatusBadge(entry.status)}>{entry.status}</span>
      </header>

      <article className="prose whitespace-pre-wrap text-neutral-800">
        {entry.body}
      </article>

      <section>
        <h2 className="mb-2 text-lg font-medium">Media ({entry.mediaReferences.length})</h2>

        {entry.mediaReferences.length === 0 ? (
          <p className="text-sm text-neutral-500">No media attached yet.</p>
        ) : (
          <ul className="grid gap-2">
            {entry.mediaReferences
              .sort((a, b) => a.displayOrder - b.displayOrder)
              .map(m => (
                <li key={m.id} className="rounded border bg-white p-3 text-sm">
                  <code className="text-xs text-neutral-500">{m.id}</code>
                  <span className="ml-2 text-neutral-400">·</span>
                  <span className="ml-2 text-neutral-500">order {m.displayOrder}</span>
                </li>
              ))}
          </ul>
        )}

        {entry.status === 'Draft' && (
          <div className="mt-4">
            <MediaUpload entryId={entry.id} />
          </div>
        )}

        <EntryActions entryId={entry.id} status={entry.status} />
      </section>
    </div>
  );
}

function entryStatusBadge(status: string) {
  const base = 'mt-2 inline-block rounded-full px-3 py-1 text-xs font-medium';
  switch (status) {
    case 'Draft':      return `${base} bg-neutral-100 text-neutral-700`;
    case 'Publishing': return `${base} bg-amber-100 text-amber-700`;
    case 'Published':  return `${base} bg-green-100 text-green-700`;
    case 'Archived':   return `${base} bg-neutral-100 text-neutral-500`;
    default:           return `${base} bg-neutral-100 text-neutral-700`;
  }
}