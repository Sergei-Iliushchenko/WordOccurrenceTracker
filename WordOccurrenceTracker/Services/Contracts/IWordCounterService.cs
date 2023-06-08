using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WordOccurrenceTracker.Services.Contracts
{
    public interface IWordCounterService
    {
        Task<Dictionary<string, int>> CountWordsAsync(string filePath, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
