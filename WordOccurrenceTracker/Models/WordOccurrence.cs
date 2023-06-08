namespace WordOccurrenceTracker.Models
{
    public class WordOccurrence
    { 
        public WordOccurrence(string word, int occurrence)
        {
            Word = word;
            Occurrence = occurrence;
        }

        public string Word { get; set; }
        public int Occurrence { get; set; }
    }
}
