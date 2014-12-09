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
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Memory
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private const string JSONFILENAME = "AppData.json";

        public AppManager appManager = new AppManager();

        // My random
        Random random = new Random();

        // my timer
        DispatcherTimer gameTimer;

        public bool imageHasBeenTapped = false;
        int gameTimeLeft = 60;
        bool gameFinished = false;
        public int index, lastIndex, pairCount;
        public int imagesTapped = 0;
        public bool ready = true;

        // list of images for the game
        public List<GameImage> gameImages;

        // to keep track of the last image that was tapped
        public List<GameImage> lastTappedImages = new List<GameImage>();

        public string questionMark = "Assets/question_mark.png";
        public int[] imageRecord = new int[6];

        public string[] allImages = {   "Assets/Beach_Images/image1.jpg",
                                        "Assets/Beach_Images/image2.jpg",
                                        "Assets/Beach_Images/image3.jpg",
                                        "Assets/Beach_Images/image4.jpg",
                                        "Assets/Beach_Images/image5.jpg",
                                        "Assets/Beach_Images/image6.jpg",
                                        "Assets/Dessert_Images/image1.jpg",
                                        "Assets/Dessert_Images/image2.jpg",
                                        "Assets/Dessert_Images/image3.jpg",
                                        "Assets/Dessert_Images/image4.jpg",
                                        "Assets/Dessert_Images/image5.jpg",
                                        "Assets/Dessert_Images/image6.jpg",
                                        "Assets/Hat_Images/image1.jpg",
                                        "Assets/Hat_Images/image2.jpg",
                                        "Assets/Hat_Images/image3.jpg",
                                        "Assets/Hat_Images/image4.jpg",
                                        "Assets/Hat_Images/image5.jpg",
                                        "Assets/Hat_Images/image6.jpg"};

        public string[] numberSymbolImages = {  "Assets/Number_Images/image1.png",
                                                "Assets/Number_Images/image2.png",
                                                "Assets/Number_Images/image3.png",
                                                "Assets/Number_Images/image4.png",
                                                "Assets/Number_Images/image5.png",
                                                "Assets/Number_Images/image6.png",
                                                "Assets/Symbol_Images/image1.png",
                                                "Assets/Symbol_Images/image2.png",
                                                "Assets/Symbol_Images/image3.png",
                                                "Assets/Symbol_Images/image4.png",
                                                "Assets/Symbol_Images/image5.png",
                                                "Assets/Symbol_Images/image6.png"};

        public string[] hardNumberSymbolImages = {  "Assets/Hard_Number_Images/image1.png",
                                                    "Assets/Hard_Number_Images/image2.png",
                                                    "Assets/Hard_Number_Images/image3.png",
                                                    "Assets/Hard_Number_Images/image4.png",
                                                    "Assets/Hard_Number_Images/image5.png",
                                                    "Assets/Hard_Number_Images/image6.png",
                                                    "Assets/Hard_Symbol_Images/image1.png",
                                                    "Assets/Hard_Symbol_Images/image2.png",
                                                    "Assets/Hard_Symbol_Images/image3.png",
                                                    "Assets/Hard_Symbol_Images/image4.png",
                                                    "Assets/Hard_Symbol_Images/image5.png",
                                                    "Assets/Hard_Symbol_Images/image6.png"};

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public GamePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += gameTimer_Tick;

            gameTimer.Start();

            headingTB.Text = "Time Left: ";
            outputTB.Text = "60";
            appManager.Player = "Player";
            appManager.GameMode = "Normal";
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // load the score from Json file
            try
            {
                await deserializeJsonAsync();
            }
            catch
            {

            }

            // check if the images list is created
            // if not get images.
            if (gameImages == null)
            {
                gameImages = new List<GameImage>();
                getImages();
                imageSetup();
            } // if

            // must tell the listview what the source
            // of information is.
            imagesGV.ItemsSource = gameImages;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void getImages()
        {
            int value;
            int j = 0;
            GameImage tempImage;

            switch (appManager.GameMode)
            {
                case "Normal":
                    // adds images to the game
                    for (int i = 0; i < 6; i++)
                    {
                        tempImage = new GameImage();

                        do
                        {
                            value = random.Next(0, allImages.Length);
                        } while (checkForDoubles(value) != false);

                        imageRecord[j] = value;
                        j++; // moves image record to next 
                        tempImage.ImageSource = questionMark;
                        tempImage.ActualImage = allImages[value];

                        gameImages.Add(tempImage);
                    } // for    
                    break;
                case "EasyNumberSymbol":
                    // adds images to the game
                    for (int i = 0; i < 6; i++)
                    {
                        tempImage = new GameImage();

                        do
                        {
                            value = random.Next(0, numberSymbolImages.Length);
                        } while (checkForDoubles(value) != false);

                        imageRecord[j] = value;
                        j++; // moves image record to next 
                        tempImage.ImageSource = questionMark;
                        tempImage.ActualImage = numberSymbolImages[value];

                        gameImages.Add(tempImage);
                    } // for    
                    break;
                case "HardNumberSymbol":
                    // adds images to the game
                    for (int i = 0; i < 6; i++)
                    {
                        tempImage = new GameImage();

                        do
                        {
                            value = random.Next(0, hardNumberSymbolImages.Length);
                        } while (checkForDoubles(value) != false);

                        imageRecord[j] = value;
                        j++; // moves image record to next 
                        tempImage.ImageSource = questionMark;
                        tempImage.ActualImage = hardNumberSymbolImages[value];

                        gameImages.Add(tempImage);
                    } // for
                    break;
            } // switch()

        } // getImages()

        private bool checkForDoubles(int value)
        {
            // to make sure there isn't any doubles
            bool isDouble = false;

            for (int i = 0; i < 6; i++)
            {
                if (imageRecord[i] == value)
                {
                    return true;
                }
                else
                {
                    isDouble = false;
                }
            } // for

            return isDouble;
        } // checkForDoubles()

        private void imageSetup()
        {
            int listSize = gameImages.Count;
            GameImage tempImage;

            // gets all images and makes a double of them
            for (int i = 0; i < listSize; i++)
            {
                tempImage = new GameImage();

                tempImage.ImageSource = gameImages[i].ImageSource;
                tempImage.ActualImage = gameImages[i].ActualImage;

                gameImages.Add(tempImage);
            } // for

            // shuffles gameRows list to get random placement of images
            shuffle(gameImages);

            // adds index to objects
            for (int i = 0; i < gameImages.Count; i++)
            {
                gameImages[i].ImageTag = i.ToString();
            } // for
        } // imageSetup()

        private void shuffle(List<GameImage> list)
        {
            // A reworked version of the FisherYates shuffle Algorithm

            int i, j;
            GameImage temp;
            int listSize = list.Count;

            // Start from the last element and swap one by one
            for (i = (listSize - 1); i > 0; i--)
            {
                // Pick a random index from 0 to i
                j = random.Next(0, i + 1);

                // Swap gameImages[i] with the element at random index
                temp = new GameImage();

                temp = list[i];
                gameImages[i] = gameImages[j];
                gameImages[j] = temp;
            } // for

        } // shuffle()

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // ready is used to avoid a bug when player selects
            // more then 2 images in a row very fast
            if (ready == true)
            {
                Image img = (Image)sender;
                GameImage lastTapped;

                index = 0;
                Int32.TryParse(img.Tag.ToString(), out index);

                if (gameImages[index].pairFound == true)
                {
                    // do nothing
                }
                else
                {
                    if (imageHasBeenTapped == false)
                    {
                        imageHasBeenTapped = true;
                        gameImages[index].ImageSource = gameImages[index].ActualImage;

                        // gets the image that was tapped
                        lastTapped = new GameImage();

                        lastTapped.ImageTag = index.ToString();
                        lastTapped.ActualImage = gameImages[index].ActualImage;

                        lastTappedImages.Add(lastTapped);
                    } // if

                    // so if a second image was tapped but not the same one twice
                    if (imageHasBeenTapped = true && gameImages[index].ImageTag != lastTappedImages[0].ImageTag)
                    {
                        ready = false;
                        gameImages[index].ImageSource = gameImages[index].ActualImage;

                        if (gameImages[index].ActualImage != lastTappedImages[0].ActualImage) // if pair doesn't match
                        {
                            //starts a timer before hiding images again
                            lastIndex = 0;
                            Int32.TryParse(lastTappedImages[0].ImageTag.ToString(), out lastIndex);

                            await Task.Delay(TimeSpan.FromMilliseconds(600));

                            gameImages[index].ImageSource = questionMark;
                            gameImages[lastIndex].ImageSource = questionMark;
                            gameImages[index].pairFound = false;
                            gameImages[lastIndex].pairFound = false;
                            lastTappedImages.Clear();
                            imageHasBeenTapped = false;
                            ready = true;
                        }
                        else if (gameImages[index].ActualImage == lastTappedImages[0].ActualImage) // if pair does match
                        {

                            lastIndex = 0;
                            Int32.TryParse(lastTappedImages[0].ImageTag.ToString(), out lastIndex);

                            gameImages[index].pairFound = true;
                            gameImages[lastIndex].pairFound = true;
                            lastTappedImages.Clear();
                            imageHasBeenTapped = false;
                            pairCount++;
                            ready = true;
                        } // if
                    } // if
                } //if
            } // if
        } // Image_Tapped()

        void gameTimer_Tick(object sender, object e)
        {
            gameTimeLeft--;

            outputTB.Text = gameTimeLeft.ToString();

            if (pairCount == 6)
            {
                gameFinished = true;
            }

            // when time runs out
            if (gameTimeLeft == 0)
            {
                gameTimer.Stop();
                gameFinished = true;
                showScore();
            }

            // if the game is finished before time runs out
            if (gameFinished == true)
            {
                gameTimer.Stop();
                showScore();
            }
        } // gameTimer_Tick()

        public async void showScore()
        {
            appManager.addGameScore(gameTimeLeft, appManager.Player);
            // shows the top 5 scores
            var dialog = new MessageDialog("Your Score is: " + gameTimeLeft + "\n\n" +
                                            "1: Name: " + appManager.Name1 + "\n    Score: " + appManager.Score1 + "\n\n" +
                                            "2: Name: " + appManager.Name2 + "\n    Score: " + appManager.Score2 + "\n\n" +
                                            "3: Name: " + appManager.Name3 + "\n    Score: " + appManager.Score3 + "\n\n" +
                                            "4: Name: " + appManager.Name4 + "\n    Score: " + appManager.Score4 + "\n\n" +
                                            "5: Name: " + appManager.Name5 + "\n    Score: " + appManager.Score5);
            await dialog.ShowAsync();

            // saves scores to Json file
            await writeJsonAsync();

            Frame.Navigate(typeof(MainPage));
        } // showScore

        private async Task writeJsonAsync()
        {
            var serializer = new DataContractJsonSerializer(typeof(AppManager));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                                JSONFILENAME,
                                CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(stream, appManager);
            }
        } // writeJsonAsync()

        private async Task deserializeJsonAsync()
        {
            AppManager theManager;
            var jsonSerializer = new DataContractJsonSerializer(typeof(AppManager));

            var myStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(JSONFILENAME);

            theManager = (AppManager)jsonSerializer.ReadObject(myStream);

            appManager.Player = theManager.Player;
            appManager.GameMode = theManager.GameMode;

            appManager.Name1 = theManager.Name1;
            appManager.Score1 = theManager.Score1;
            appManager.Name2 = theManager.Name2;
            appManager.Score2 = theManager.Score2;
            appManager.Name3 = theManager.Name3;
            appManager.Score3 = theManager.Score3;
            appManager.Name4 = theManager.Name4;
            appManager.Score4 = theManager.Score4;
            appManager.Name5 = theManager.Name5;
            appManager.Score5 = theManager.Score5;
        } // deserializeJsonAsync()
    }
}
