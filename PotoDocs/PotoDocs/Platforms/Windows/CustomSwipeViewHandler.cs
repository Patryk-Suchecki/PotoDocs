using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;


#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace PotoDocs;

public class CustomSwipeViewHandler : ViewHandler<SwipeView, FrameworkElement>
{
    private bool _isSwiping = false;

    public static IPropertyMapper<SwipeView, CustomSwipeViewHandler> Mapper =
        new PropertyMapper<SwipeView, CustomSwipeViewHandler>(ViewHandler.ViewMapper);

    public CustomSwipeViewHandler() : base(Mapper)
    {
    }

    protected override FrameworkElement CreatePlatformView()
    {
        var container = new Microsoft.UI.Xaml.Controls.Grid();

        var content = VirtualView.Content?.ToPlatform(MauiContext);
        if (content != null)
        {
            container.Children.Add(content);
        }

        container.ManipulationMode = ManipulationModes.TranslateX;
        container.ManipulationStarted += OnManipulationStarted;
        container.ManipulationDelta += OnManipulationDelta;
        container.ManipulationCompleted += OnManipulationCompleted;

        return container;
    }

    private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        _isSwiping = false; // Resetujemy flagę
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        if (VirtualView != null)
        {
            // Obliczamy maksymalne przesunięcie (domyślnie 100, jeśli Width == 0)
            var maxSwipeDistance = VirtualView.Width > 0 ? VirtualView.Width / 2 : 100;

            // Aktualizujemy przesunięcie
            VirtualView.TranslationX += e.Delta.Translation.X;

            // Logujemy dla diagnostyki
            Console.WriteLine($"TranslationX: {VirtualView.TranslationX}, MaxSwipeDistance: {maxSwipeDistance}");

            // Ograniczamy przesunięcie
            VirtualView.TranslationX = Math.Clamp(
                VirtualView.TranslationX,
                -maxSwipeDistance, // Minimalna wartość
                maxSwipeDistance   // Maksymalna wartość
            );
        }
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        if (_isSwiping)
        {
            _isSwiping = false;

            if (VirtualView != null)
            {
                var threshold = VirtualView.Width / 4;
                if (Math.Abs(VirtualView.TranslationX) > threshold)
                {
                    if (VirtualView.TranslationX > 0)
                    {
                        VirtualView.Open(OpenSwipeItem.LeftItems);
                    }
                    else
                    {
                        VirtualView.Open(OpenSwipeItem.RightItems);
                    }
                }
                else
                {
                    VirtualView.Close();
                }

                VirtualView.TranslateTo(0, 0, 250, Easing.SpringOut);
            }
        }
    }
}
