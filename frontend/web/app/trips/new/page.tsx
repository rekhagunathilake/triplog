import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import NewTripForm from './NewTripForm';

export default async function NewTripPage() {
  const session = await auth();
  if (!session) redirect('/');

  return <NewTripForm />;
}