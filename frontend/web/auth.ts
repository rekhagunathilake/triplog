import NextAuth from 'next-auth';
import Google from 'next-auth/providers/google';
import { SignJWT } from 'jose';

async function signApiToken(email: string): Promise<string> {
    const secret = new TextEncoder().encode(process.env.AUTH_SECRET);
    return await new SignJWT({ email })
        .setProtectedHeader({ alg: 'HS256' })
        .setIssuedAt()
        .setExpirationTime('7d')
        .sign(secret);
}

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
    async jwt({ token, user }) {
        // Runs at sign-in AND on subsequent auth() calls
        // On sign-in, `user` is populated, Persist a fresh signed API token in the JWT payload.
        if (user?.email) {
            token.apiToken = await signApiToken(user.email);
        }
        return token;
    },
    async session({ session, token }) {
        // Expose the signed API token on the session object so components can read it.
        session.apiToken = token.apiToken as string;
        return session;
    },
  },
});