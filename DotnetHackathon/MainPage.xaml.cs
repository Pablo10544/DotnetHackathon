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
        string SystemMessage = "continue a história com tema de suspense";
        string EndMessage = "crie um final muito engraçado para terminando com a palavra FIM:";
        string RefinedStoryMessage = "Em no maximo 400 palavras reescreva a historia como se fosse um renomado autor e de um fim engraçado para:";
        string KeepWritingMessage = "Em no maximo de 15 palavras continue:";
        string ImageFromStoryMessage = "Crie uma descrição muito fofa de alguma parte da história em no maximo 50 palavras para ser usada em um modelo de geração de imagem, não use palavras que viole o filtro de palavras seguras";
        string ImageDescription = "";
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
            story += s;
            UpdateSpan(s,Color.FromRgba(250, 177, 160, 255));
            SuggestWords();

        }
        async void ReceiveRedoneTextAsync()
        {
            string s = await client.GetMessage();
            Redone.Text = s;

        }
        async void SuggestWords(int length=3) {

            string s2 = "relacionado a história:" + story + "    sugira 3 palavra para o usuário usar, escreva apenas as palavras separadas por virgulas";
            client2.AddMessage(s2,ChatRole.User);
            string response = await client2.GetMessage();
            foreach(string word in response.Split(','))
            {
                //fazer tratamento para word removendo as palavras sugerindo e "sugira 3 palavra para o usuário usar"
                WordsSuggestion ws = new WordsSuggestion(word,3,true);
            Words.Add(ws);
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
                if (count >=2) {
                FinishWritingStory(senderEntry);
            } else {
                KeepWritingStory(senderEntry);
            }
            Send();

        }
        public void FinishWritingStory(Entry senderEntry) {
            GetPoints(senderEntry.Text);
            client.AddMessage(EndMessage + story, ChatRole.User);
            ReceiveMessageAsync();
            client.AddMessage(RefinedStoryMessage + story, ChatRole.User);
            ReceiveRedoneTextAsync();
            client.AddMessage(ImageFromStoryMessage,ChatRole.User);
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
            ImageDescription = "";
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
