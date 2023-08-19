using System.Threading.Tasks;

namespace BuildTimeHistory
{
    public class SponsorDetector
    {
        // This might be the code you see, but it's not what I compile into the extension when built ;)
        public static async Task<bool> IsSponsorAsync()
        {
            return await Task.FromResult(false);
        }
    }
}
