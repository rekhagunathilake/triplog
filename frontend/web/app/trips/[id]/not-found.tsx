import Link from 'next/link';

export default function TripNotFound() {
  return (
    <div className="text-center">
      <h1 className="text-2xl font-semibold">Trip not found</h1>
      <p className="mt-2 text-neutral-600">Maybe it was archived or the link's wrong.</p>
      <Link href="/" className="mt-4 inline-block text-blue-600 hover:underline">
        Back to trips
      </Link>
    </div>
  );
}