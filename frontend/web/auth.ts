import NextAuth from 'next-auth';
import Google from 'next-auth/providers/google';

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [Google],
  session: { strategy: 'jwt' },
  callbacks: {
    async signIn({ profile }) {
      // Only your own Google account can sign in.
      // The backend also enforces this via OwnerOnly policy, but blocking at
      // sign-in gives a nicer UX than letting anyone in and then failing writes.
      const ownerEmail = process.env.NEXT_PUBLIC_OWNER_EMAIL;
      if (!ownerEmail) return true; // fail-open in local dev if not set
      return profile?.email?.toLowerCase() === ownerEmail.toLowerCase();
    },
  },
});