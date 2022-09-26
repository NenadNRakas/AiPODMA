﻿//using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
namespace AiPODMA;
public partial class MainPage : ContentPage
{
    internal class BitmapImage
    {
        private Uri photoURI;
        public BitmapImage(Uri photoURI)
        {
            this.photoURI = photoURI;
        }
    }
    const string SettingDateToday = "date today";
    const string SettingShowOnStartup = "show on startup";
    const string SettingImageCountToday = "image count today";
    const string SettingLimitRange = "limit range";
    // Declare a container for the local settings.
    //ApplicationDataContainer localSettings;
    // The objective of the NASA API portal is to make NASA data, including imagery, eminently accessible to application developers. 
    const string EndpointURL = "https://api.nasa.gov/planetary/apod";
    // The objective of the NASA API portal is to make NASA data, including imagery, eminently accessible to application developers. 
    const string DesignerURL = "https://aicloudptyltd.business.site";
    // June 16, 1995  : the APOD launch date.
    DateTime launchDate = new DateTime(1995, 6, 16);
    // A count of images downloaded today.
    private int imageCountToday;
    private int count = 0;
    // Application settings status
    //private string imageAutoLoad = "Yes";
    // To support the Timeline, we need to record user activity, and create an Adaptive Card.
    //UserActivitySession _currentActivity;
    //AdaptiveCard apodTimelineCard;
    public MainPage()
    {
        InitializeComponent();
        if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Minor >= 0 && Environment.OSVersion.Version.Build >= 19041)
        {
            FeedBackButton.IsVisible = true;
            FeedBackButton.Text = "Feedback?";
        }
        else 
        { 
            FeedBackButton.IsVisible = true; 
            FeedBackButton.Text = "by [@.i.]™"; 
        }
        if (AutoLoadImageCheckBox.IsChecked == true)
        {
            _ = LoadPhoto();
        }
        // 10 minute animation
        uint duration = 10 * 60 * 1000;
        Task.WhenAll
        (
          BotImage.RotateTo(307 * 360, duration),
          BotImage.RotateXTo(251 * 360, duration),
          BotImage.RotateYTo(199 * 360, duration)
        );
        //WebView1.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().EnableZoomControls(true);
        //WebView1.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().DisplayZoomControls(true);
    }
    private void OnContentReload(object sender, EventArgs args)
    {
        WebView1.WidthRequest = ContentScrollView.Width;
        WebView1.HeightRequest = ContentScrollView.Height;
    }
        private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Loaded {count} image!";
        else
            CounterBtn.Text = $"Loaded {count} images!";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    private void OnLaunchClicked(object sender, EventArgs e)
    {
        // Return thee full date range
        DateLimitCheckeBox.IsChecked = false;
        //Set the date to the begining
        DatePck.Date = launchDate;
    }
    private void DatePck_DateChanged(object sender, EventArgs e)
    {
        _ = LoadPhoto();
        //OnContentReload(sender, e);
        //ImagePictureBox.IsVisible = true;
    }
    private void AutoLoadImageCheckBox_OnChecked(object sender, EventArgs args)
    {
        //AutoLoadImageCheckBox.IsChecked = true;
        // T.B.D.
    }
    private void AutoLoadImageChecked_OnUnchacked(object sender, EventArgs args)
    {
        //AutoLoadImageCheckBox.IsChecked = false;
        // T.B.D.
    }
    private void LimitRangeCheckBox_OnChecked(object sender, CheckedChangedEventArgs args)
    {
        //AutoLoadImageCheckBox.IsChecked = true;
        if (DateLimitCheckeBox.IsChecked == true)
        {
            DateTime firstDayOfThisYear = new DateTime(DateTime.Today.Year, 1, 1);
            DatePck.MinimumDate = firstDayOfThisYear;

        }
        else
        {
            DatePck.MinimumDate = launchDate;
        }
    }
    private void LimitRangeCheckBox_OnUnchecked(object sender, CheckedChangedEventArgs arg)
    {
        //AutoLoadImageCheckBox.IsChecked = false;
        // T.B.D.
    }
    private async void FeedBackButton_Clicked(object sender, EventArgs args)
    {
        // T.B.D.
        /*var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
        await launcher.LaunchAsync();*/
        if (Email.Default.IsComposeSupported)
        {
            string subject = "A.i.POD Feedback";
            string body = "This is our new multi-platform APOD application upgrade non Microsoft feedback option. I've " +
                          "attached a photo of our adventures together.";
            string[] recipients = new[] { "info@aicloudptyltd.com", "NenadRakas@outlook.com" };
            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string>(recipients)
            };
            string picturePath = Path.Combine(FileSystem.CacheDirectory, "memories.jpg");
            message.Attachments.Add(new EmailAttachment(picturePath));
            await Email.Default.ComposeAsync(message);
        }
    }
    private bool IsSupportedFormat(string photoUrl)
    {
        //throw new NotImplementedException();
        // Extract the extension, force to lower case and compare
        string ext = Path.GetExtension(photoUrl).ToLower();
        // Check the UWP formats
        return (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".svg" ||
                ext == ".bmp" || ext == ".tif" || ext == ".ico" || ext == ".gif");
    }
    private async Task LoadPhoto()
    {
        var client = new HttpClient();
        string description = null;
        string photoUrl = null;
        string copyright = null;
        // Set the UI elements to defaults
        ImageCopyrightTextBox.Text = "© " + "NASA";
        DescriptionLabel.Text = " ";
        // Build the date parameter string for the date selected, or the last date if a range is specified.
        DateTimeOffset dt = (DateTimeOffset)DatePck.Date;
        string dateSelected = $"{dt.Year.ToString()}-{dt.Month.ToString("00")}-{dt.Day.ToString("00")}";
        string apiNasa = $"TZ6ay3nXkgGqVPMlWbrxYArpggcdyqSCjR7ZVeim";
        string URLParams = $"?date={dateSelected}&api_key={apiNasa}";
        // Populate the Http client appropriately.
        client.BaseAddress = new Uri(EndpointURL);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // The critical call: sends a GET request with the appropriate parameters.
        HttpResponseMessage response = client.GetAsync(URLParams).Result;
        if (response.IsSuccessStatusCode)
        {
            // Be ready to catch any data/server errors.
            try
            {
                // Parse response using Newtonsoft APIs.
                string responseContent = await response.Content.ReadAsStringAsync();
                // Parse the response string for the details we need.
                JObject jResult = JObject.Parse(responseContent);
                // Now get the image.
                photoUrl = (string)jResult["url"];
                var photoURI = new Uri(photoUrl);
                var bmi = new BitmapImage(photoURI);
                description = (string)jResult["explanation"];
                copyright = (string)jResult["copyright"];
                // Set the variable
                if (IsSupportedFormat(photoUrl))
                {
                    //ImagePictureBox.Source = (new Uri(photoUrl, UriKind.Absolute));  //photoURI;
                    WebView1.Source = (new Uri(photoUrl, UriKind.Absolute));
                    // Get the copyright message, but fill with "NASA" if no name is provided.
                    //copyright = (string)jResult["copyright"];
                    if (copyright != null && copyright.Length > 0)
                    {
                        ImageCopyrightTextBox.Text = "© " + copyright;
                    }
                    // Populate the description text box.
                    DescriptionLabel.Text = description;
                    // Switch the visibility back
                    await Task.Delay(TimeSpan.FromSeconds(3.3f));
                    WebView1.IsVisible = true;
                    ImagePictureBox.IsVisible = false;                    
                    WebView1.HeightRequest = 789;
                    WebView1.WidthRequest = TopVerticalLayout.WidthRequest;
                }
                else
                {
                    WebView1.Source = new Uri(photoUrl, UriKind.Absolute);
                    //WebView1.Navigate(new Uri(photoUrl));
                    ImageCopyrightTextBox.Text = "© " + copyright;
                    DescriptionLabel.Text = description + $"Url is: {photoUrl}";
                    await Task.Delay(TimeSpan.FromSeconds(3.3f));
                    ImagePictureBox.IsVisible = false;
                    WebView1.IsVisible = true;
                    WebView1.HeightRequest = 789;
                    WebView1.WidthRequest = TopVerticalLayout.WidthRequest;
                }
            }
            catch (Exception ex)
            {
                //WebView1.IsVisible = false;
                //ImagePictureBox.IsVisible = false;
                WebView1.Source = (new Uri(photoUrl, UriKind.Absolute));
                //WebView1.Navigate(new Uri(photoUrl));
                if (copyright != null && copyright.Length > 0)
                {
                    ImageCopyrightTextBox.Text = "© " + copyright;
                }
                DescriptionLabel.Text = description + $" Msg: {ex.Message}";
            }
            // Keep track of our downloads, in case we reach the limit.
            ++imageCountToday;
            ImagesTodayTextBox.Text = imageCountToday.ToString();
        }
        else
        {
            DescriptionLabel.Text = "We were unable to retrieve the NASA picture for that day: " +
                $"{response.StatusCode.ToString()} {response.ReasonPhrase}";
        }
        //SetupForTimelineAsync();
    }
}

