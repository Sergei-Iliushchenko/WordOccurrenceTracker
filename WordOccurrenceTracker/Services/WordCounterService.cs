using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WordOccurrenceTracker.Services.Contracts;

namespace WordOccurrenceTracker.Services
{
    public class WordCounterService : IWordCounterService
    {
        private const int ProgressStep = 5;
        public async Task<Dictionary<string, int>> CountWordsAsync(string filePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var wordCounts = new Dictionary<string, int>();
            using var reader = new StreamReader(filePath);
            long fileSize = new FileInfo(filePath).Length;
            long bytesRead = 0;
            int borderStep = ProgressStep;
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string line = (await reader.ReadLineAsync(cancellationToken))!;

                string[] words = line.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in words)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts[word] = 1;
                }

                bytesRead += line.Length + Environment.NewLine.Length;
                var progressPercentage = (double)bytesRead / fileSize * 100;
                if (progressPercentage > borderStep)
                {
                    progress.Report((int)progressPercentage);
                    borderStep += ProgressStep;
                }
            }

            return wordCounts;
        }
    }
}
