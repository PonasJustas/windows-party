namespace TestTesonet.Infrastructure.Events
{
    public class BusyEvent
    {
        public BusyEvent(string eventKey)
        {
            Event = eventKey;
            IsBusy = true;
        }

        public BusyEvent(string eventKey, string title, string text) : this(eventKey)
        {
            Title = title;
            Text = text;
        }

        public string Event { get; }
        public string Title { get; set; }
        public string Text { get; set; }
        public bool IsBusy { get; set; }
    }
}
