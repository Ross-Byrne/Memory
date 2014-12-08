using Memory.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Memory
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        ScoreManager scoreManager = new ScoreManager();

        private const string JSONFILENAME = "ScoreData.json";

        string gameMode;

        public SettingsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (localSettings.Values.ContainsKey("GameMode"))
            {
                gameMode = localSettings.Values["GameMode"].ToString();
            }

            // gets game mode when navigating to page
            string value = e.NavigationParameter as string;

            if (!string.IsNullOrWhiteSpace(value))
            {
                gameMode = value;
            }

            // load selected game mode into radio buttons
            if (gameMode == "Normal")
            {
                myFirstRadioButton.IsChecked = true;
            }
            else if (gameMode == "EasyNumberSymbol")
            {
                mySecondRadioButton.IsChecked = true;
            }
            else if (gameMode == "HardNumberSymbol")
            {
                myThirdRadioButton.IsChecked = true;
            } // if else

            // load the score from Json file
            try
            {
                await deserializeJsonAsync();
            }
            catch
            {

            }

            // keeps the players name updated
            playerNameTBX.Text = scoreManager.Player;

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            localSettings.Values["GameMode"] = gameMode;

            // saves scores to Json file
            await writeJsonAsync();
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void saveGameModeBt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (myFirstRadioButton.IsChecked == true)
            {
                gameMode = "Normal";
            }
            else if (mySecondRadioButton.IsChecked == true)
            {
                gameMode = "EasyNumberSymbol";
            }
            else if (myThirdRadioButton.IsChecked == true)
            {
                gameMode = "HardNumberSymbol";
            } // if else

            Frame.Navigate(typeof(MainPage), gameMode);

        } // saveGameModeBt_Tapped()

        private async void topScoreBt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // shows the top 5 scores
            var dialog = new MessageDialog("1: Name: " + scoreManager.Name1 + "\n    Score: " + scoreManager.Score1 + "\n\n" +
                                            "2: Name: " + scoreManager.Name2 + "\n    Score: " + scoreManager.Score2 + "\n\n" +
                                            "3: Name: " + scoreManager.Name3 + "\n    Score: " + scoreManager.Score3 + "\n\n" +
                                            "4: Name: " + scoreManager.Name4 + "\n    Score: " + scoreManager.Score4 + "\n\n" +
                                            "5: Name: " + scoreManager.Name5 + "\n    Score: " + scoreManager.Score5);
            await dialog.ShowAsync();
        } // // topScoreBt_Tapped()

        private void playerNameTBX_TextChanged(object sender, TextChangedEventArgs e)
        {
            scoreManager.Player = playerNameTBX.Text;
        } // playerNameTBX_TextChanged()

        private void playerNameTBX_GotFocus(object sender, RoutedEventArgs e)
        {
            playerNameTBX.SelectAll();
        } // playerNameTBX_GotFocus()

        private async Task writeJsonAsync()
        {
            var serializer = new DataContractJsonSerializer(typeof(ScoreManager));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                                JSONFILENAME,
                                CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(stream, scoreManager);
            }

        } // writeJsonAsync()

        private async Task deserializeJsonAsync()
        {
            string content = String.Empty;

            ScoreManager theScore;
            var jsonSerializer = new DataContractJsonSerializer(typeof(ScoreManager));

            var myStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(JSONFILENAME);

            theScore = (ScoreManager)jsonSerializer.ReadObject(myStream);

            scoreManager.Player = theScore.Player;

            scoreManager.Name1 = theScore.Name1;
            scoreManager.Score1 = theScore.Score1;
            scoreManager.Name2 = theScore.Name2;
            scoreManager.Score2 = theScore.Score2;
            scoreManager.Name3 = theScore.Name3;
            scoreManager.Score3 = theScore.Score3;
            scoreManager.Name4 = theScore.Name4;
            scoreManager.Score4 = theScore.Score4;
            scoreManager.Name5 = theScore.Name5;
            scoreManager.Score5 = theScore.Score5;
        } // deserializeJsonAsync()
    }
}
