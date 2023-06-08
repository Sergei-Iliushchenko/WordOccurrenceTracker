using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WordOccurrenceTracker.Models;
using WordOccurrenceTracker.Services.Contracts;
using WordOccurrenceTracker.Utilities;

namespace WordOccurrenceTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IWordCounterService _wordCounterService;
        private CancellationTokenSource _cancellationTokenSource = null!;
        private int _progressPercentage;
        private bool _isProcessing;
        private string _filePath = null!;

        public MainViewModel(IWordCounterService wordCounter)
        {
            _wordCounterService = wordCounter ?? throw new ArgumentNullException(nameof(wordCounter));
            ProcessCommand = new AsyncRelayCommand(ProcessAsync);
            CancelCommand = new RelayCommand(_ => Cancel());
            BrowseCommand = new RelayCommand(_ => Browse());
        }

        public ObservableCollection<WordOccurrence> WordOccurrences { get; } = new();

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ProcessCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand BrowseCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    OnPropertyChanged();
                }
            }
        }


        private async Task ProcessAsync()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                IsProcessing = true;
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    var progress = new Progress<int>(percentage =>
                    {
                        ProgressPercentage = percentage;
                    });
                    var wordCounts = await _wordCounterService.CountWordsAsync(FilePath, progress, _cancellationTokenSource.Token);
                    WordOccurrences.Clear();
                    foreach (var wordCount in wordCounts.OrderBy(kv => kv.Value))
                    {
                        WordOccurrences.Add(new WordOccurrence(wordCount.Key, wordCount.Value));
                    }
                }
                catch (OperationCanceledException)
                {
                   
                }
                finally
                {
                    IsProcessing = false;
                    _cancellationTokenSource.Dispose();
                }
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private void Browse()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                FilePath = filePath;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
