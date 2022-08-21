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
	int count = 0;
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
    // Application settings status
    //private string imageAutoLoad = "Yes";
    // To support the Timeline, we need to record user activity, and create an Adaptive Card.
    //UserActivitySession _currentActivity;
    //AdaptiveCard apodTimelineCard;

    public MainPage()
	{
        InitializeComponent();
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
	private void DatePck_DateChanged(object sender, EventArgs e)
	{
        _ = LoadPhoto();
        //ImagePictureBox.IsVisible = true;
	}
    private void AutoLoadImageCheckBox_OnChecked(object sender, EventArgs args)
    {
        AutoLoadImageCheckBox.IsChecked = true;
    }
    private void AutoLoadImageChecked_OnUnchacked(object sender, EventArgs args)
    {
        AutoLoadImageCheckBox.IsChecked = false;
    }
    private bool IsSupportedFormat(string photoUrl)
    {
        //throw new NotImplementedException();
        // Extract the extension, force to lower case and compare
        string ext = Path.GetExtension(photoUrl).ToLower();
        // Check the UWP formats
        return (ext == ".jpg" || ext == "jpeg" || ext == ".png" || ext == ".svg" ||
                ext == ".bmp" || ext == ".tif" || ext == ".ico" || ext == ".gif");
    }
    private async Task LoadPhoto()
    {
        var client = new HttpClient();
        string description = null;
        string photoUrl = null;
        string copyright = null;
        // Set the UI elements to defaults
        ImageCopyrightTextBox.BindingContext = "© " + "NASA";
        DescriptionTextBox.BindingContext = " ";
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
                ImagePictureBox.Source = photoURI;
                if (IsSupportedFormat(photoUrl))
                {
                    // Get the copyright message, but fill with "NASA" if no name is provided.
                    //copyright = (string)jResult["copyright"];
                    if (copyright != null && copyright.Length > 0)
                    {
                        ImageCopyrightTextBox.BindingContext = "© " + copyright;
                    }
                    // Populate the description text box.
                    DescriptionTextBox.BindingContext = description;
                    // Switch the visibility back
                    await Task.Delay(TimeSpan.FromSeconds(1.3f));
                    WebView1.IsVisible = false;
                    ImagePictureBox.IsVisible = true;
                }
                else
                {
                    WebView1.Source = (new Uri(photoUrl, UriKind.Absolute));
                    //WebView1.Navigate(new Uri(photoUrl));
                    ImageCopyrightTextBox.BindingContext = "© " + copyright;
                    DescriptionTextBox.BindingContext = description + $"Url is: {photoUrl}";
                    await Task.Delay(TimeSpan.FromSeconds(1.3f));
                    ImagePictureBox.IsVisible = false;
                    WebView1.IsVisible = true;
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
                    ImageCopyrightTextBox.BindingContext = "© " + copyright;
                }
                DescriptionTextBox.BindingContext = description + $" Msg: {ex.Message}";
            }
            // Keep track of our downloads, in case we reach the limit.
            ++imageCountToday;
            ImagesTodayTextBox.BindingContext = imageCountToday.ToString();
        }
        else
        {
            DescriptionTextBox.BindingContext = "We were unable to retrieve the NASA picture for that day: " +
                $"{response.StatusCode.ToString()} {response.ReasonPhrase}";
        }
        //SetupForTimelineAsync();
    }
}

