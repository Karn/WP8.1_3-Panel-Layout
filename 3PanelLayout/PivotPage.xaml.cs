/**
 *  3 Panel Facebook Style Layout
 *
 *  This is a sample three panel layout designed for Windows Phone 8 Silverlight.
 *  It is a rework of an example by Dan Ardelean (https://www.developer.nokia.com/Profile/?u=dan.ardelean)
 *  taken from the following page;
 *  http://sviluppomobile.blogspot.in/2013/08/add-lateral-menus-to-windows-phone.html
 *
 *  The changes that are implemented are as follows;
 *  - Change from Manipulation events to Pointer events
 *  - Adaptation to various phone screen sizes
 *  - Code cleanup
 *
 *  I'm not too good with the license stuff so it is free to use for anything you want. 
 *  As a result this application is provided as is.
 *
 *  Cheers,
 *  Karn.
 */

using _3PanelLayout.Common;
using _3PanelLayout.Data;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace _3PanelLayout {
    public sealed partial class PivotPage : Page {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        #region Panel Definitions
        private static double PageWidth = Window.Current.Bounds.Width;
        private static double PageHeight = Window.Current.Bounds.Height;

        private static double LeftPanelPosition = 0;
        private static double CenterPanelPosition = -1 * Window.Current.Bounds.Width;
        private static double RightPanelPosition = -1 * Window.Current.Bounds.Width * 2;

        private static double InitialXPosition = 0;
        #endregion

        public PivotPage() {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            #region Panel components
            //Initialize components
            LeftPanel.Width = PageWidth;
            LeftPanel.Margin = new Thickness(0);

            CenterPanel.Width = PageWidth;
            CenterPanel.Margin = new Thickness(PageWidth, 0, 0, 0);

            RightPanel.Width = PageWidth;
            RightPanel.Margin = new Thickness(PageWidth * 2, 0, 0, 0);

            //Set initial view
            Canvas.SetLeft(LayoutRoot, CenterPanelPosition);
            #endregion

            //Make the statusbar sit over the app (comment out to see change in statusbar
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            // TODO: Save the unique state of the page here.
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
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        #region PANEL MANIPULATION HANDLER
        /// <summary>
        /// Sets InitialXPosition to the x position of the input relative to the the canvas element.
        /// </summary>
        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e) {
            InitialXPosition = e.GetCurrentPoint(canvas).Position.X;
        }

        /// <summary>
        /// Compares InitialXPosition to the x position of the pointer after it released and determines if the
        /// gesture is to the left or to the right. If the gesture is enough to be considered a swipe in the
        /// horizontal direction the appropriate panel is opened.
        /// </summary>
        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e) {
            if (e.GetCurrentPoint(canvas).Position.X - InitialXPosition < -50) {
                MoveRight(null, null);
            } else if (e.GetCurrentPoint(canvas).Position.X - InitialXPosition > 50) {
                MoveLeft(null, null);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Get the current position of the canvas and move in the appropriate direction by passing the 
        /// final position of the view to the MoveView function.
        /// </summary>
        private void MoveLeft(object sender, RoutedEventArgs e) {
            var currentPosition = Canvas.GetLeft(LayoutRoot);
            if (currentPosition == CenterPanelPosition) {
                MoveView(LeftPanelPosition);
            } else if (currentPosition == RightPanelPosition) {
                MoveView(CenterPanelPosition);
            }
        }

        /// <summary>
        /// Get the current position of the canvas and move in the appropriate direction by passing the 
        /// final position of the view to the MoveView function.
        /// </summary>
        private void MoveRight(object sender, RoutedEventArgs e) {
            var currentPosition = Canvas.GetLeft(LayoutRoot);
            if (currentPosition == LeftPanelPosition) {
                MoveView(CenterPanelPosition);
            } else if (currentPosition == CenterPanelPosition) {
                MoveView(RightPanelPosition);
            }
        }

        /// <summary>
        /// Method that handles the movement of the cavas via a storyboard.
        /// </summary>
        /// <param name="left">Position that the panel is to be moved to.</param>
        void MoveView(double left) {
            ((Storyboard)canvas.Resources["moveAnimation"]).SkipToFill();
            ((DoubleAnimation)((Storyboard)canvas.Resources["moveAnimation"]).Children[0]).To = left;
            ((Storyboard)canvas.Resources["moveAnimation"]).Begin();
        }

        #endregion
    }
}
