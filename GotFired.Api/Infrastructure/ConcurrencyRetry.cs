using GotFired.DAL;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// Commands without a client-supplied version (answer, reply, notes, tags, appeal state) are "apply this change to
    /// the current case". If another request saved the case between our load and save (RowVersion check), reload and
    /// apply the change again instead of failing. Edits that carry the client's version (If-Match) must NOT use this.
    /// </summary>
    public static class ConcurrencyRetry
    {
        private const int Attempts = 6;

        public static async Task<T> RunAsync<T>(GotFiredDbContext db, Func<Task<T>> loadChangeAndSave)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    return await loadChangeAndSave();
                }
                catch (DbUpdateConcurrencyException) when (attempt < Attempts)
                {
                    db.ChangeTracker.Clear();   // forget the stale entities; the next attempt reloads them
                    // Randomized backoff so requests that collided do not retry in lockstep and collide again.
                    await Task.Delay(Random.Shared.Next(5, 20 * attempt));
                }
            }
        }
    }
}
