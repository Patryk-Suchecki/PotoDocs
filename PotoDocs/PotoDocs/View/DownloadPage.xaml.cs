namespace PotoDocs.View;

public partial class DownloadPage : ContentPage
{
    public DownloadPage(DownloadViewModel downloadViewModel)
    {
        InitializeComponent();
        BindingContext = downloadViewModel;
    }
}

