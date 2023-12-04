using Azure;
using Azure.AI.OpenAI;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DotnetHackathon
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        Client client;
        Client client2;
        int count = 0;
        int Points { get; set; }   
        string story="";
        string SystemMessage = "continue the story with a suspense theme";
        string EndMessage = "\n create an ending to the story above, ending with the word END";
        string RefinedStoryMessage = "\n rewrite the story above as if you were a renowned author,keep the ending, In no more than 200 words and two paragraphs.";
        string KeepWritingMessage = "In no more than 15 words continue the story:";
        string PreSugestionWord = "related to the story:";
        string PosSugestionWord = "\n suggest 3 words for the user to use, write only the words separated by commas";
        public ObservableCollection<WordsSuggestion> Words { get; set; }
        public MainPage()
        {
            InitializeComponent();
            CreateClient();
            Words = new ObservableCollection<WordsSuggestion>();
            Points = 0;
            BindingContext = this;
        }
        public void CreateClient() {
            client = new Client();
            client.AddMessage(SystemMessage, ChatRole.System);
            client2 = new Client();
            client2.AddMessage(SystemMessage, ChatRole.System);
        }
        async void ReceiveMessageAsync() {
            string s = await client.GetMessage();
            story += RemoveWordsFromMessage(s);
            UpdateSpan(RemoveWordsFromMessage(s), Color.FromRgba(250, 177, 160, 255));
            SuggestWords();

        }
        public string RemoveWordsFromMessage(string message) {
           string messageCorrected= message.Replace(KeepWritingMessage,"").Replace("In no more than 15 words continue the story","");
            return messageCorrected;
        }
        async void ReceiveRedoneTextAsync()
        {
            string s = await client.GetMessage();
            Redone.Text = s;

        }
        public string RemoveWordsFromSuggestion(string word) {
           string wordFixed= word.Replace("suggest 3 words for the user to use", "").Replace(".", "").Replace(PreSugestionWord,"").ToLower();
            return wordFixed;
        }
        async void SuggestWords(int length=3) {
            if (Words.Count < 4)
            {
                string s2 = PreSugestionWord + story + PosSugestionWord;
                client2.AddMessage(s2, ChatRole.User);
                string response = await client2.GetMessage();
                foreach (string word in response.Split(','))
                {
                    WordsSuggestion ws = new WordsSuggestion(RemoveWordsFromSuggestion(word), 3, true);
                    Words.Add(ws);
                }
            }
            
        }
        void UpdateSpan(string message,Color color) {
            var s = new Span { Text = " "+message,TextColor=color };
            var formattedString = Answer.FormattedText;
            formattedString.Spans.Add(s);

            Answer.FormattedText = formattedString;
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
            Entry senderEntry = Entrada;
            if (senderEntry.Text == null || senderEntry.Text == " ") {
                return;
            }
                if (count >=3) {
                FinishWritingStory(senderEntry);
            } else {
                KeepWritingStory(senderEntry);
            }
            Send();

        }
        public void FinishWritingStory(Entry senderEntry) {
            GetPoints(senderEntry.Text);
            client.AddMessage( story+ EndMessage, ChatRole.User);
            ReceiveMessageAsync();
            client.AddMessage(story+ RefinedStoryMessage, ChatRole.User);
            ReceiveRedoneTextAsync();
            senderEntry.IsEnabled = false;
            senderEntry.IsVisible = false;
            ButtonSend.IsVisible = false;
            ButtonRestart.IsVisible = true;
            SuggestedWords.IsVisible = false;


        }
        public void KeepWritingStory(Entry senderEntry) {
            client.AddMessage(KeepWritingMessage + senderEntry.Text, ChatRole.User);
            story += senderEntry.Text;
            GetPoints(senderEntry.Text);
            ReceiveMessageAsync();
        }
        void GetPoints(string s) {
            var list= Words.Where(x => s.Contains(x.Word)).ToArray();
            foreach (var point in list) { 
                Points++;
                PointsLabel.Text = "Points: " + Points;
                Words.Remove(point);
            }
                
            

            
        }
        public void Send() {

            UpdateSpan(Entrada.Text, Color.FromRgba(129, 236, 236, 255));
            Entrada.Text = "";
            count++;
        }
        public void Restart() {
            count = 0;
            Entrada.IsEnabled = true;
            Entrada.IsVisible = true;
            SuggestedWords.IsVisible = true;
            Words.Clear();
            story = "";
            Redone.Text = "";
            Points = 0;
            PointsLabel.Text = "Pontos: "+Points;
            var formattedString = new FormattedString();
            Answer.FormattedText = formattedString;
            ButtonSend.IsVisible = true;
            ButtonRestart.IsVisible = false;
            CreateClient();

        }

        private void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            Restart();
        }
    }

}
