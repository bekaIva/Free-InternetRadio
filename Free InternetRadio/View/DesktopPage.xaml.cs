using iRadio_Free.Model;
using iRadio_Free.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace iRadio_Free.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DesktopPage : Page
    {
        MainViewModel viewmodel;
        SystemMediaTransportControls systemControls;
        public DesktopPage()
        {
            this.InitializeComponent();
            viewmodel = (MainViewModel)DataContext;
            CurrentNavigationText.Text = "Featured";
            systemControls = SystemMediaTransportControls.GetForCurrentView();
            systemControls.IsEnabled = true;
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.ButtonPressed += (arg1, arg2) =>
            {
                Player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {

                    switch (arg2.Button)
                    {
                        case SystemMediaTransportControlsButton.Next:
                            {
                                try
                                {
                                    var channel = Player.Tag as Channel;
                                    if (channel != null && channel.NextChannel != null)
                                    {
                                        Player.Stop();
                                        channel.IsPlaying = false;
                                        Player.Tag = channel.NextChannel;
                                        Player.Source = new Uri(channel.NextChannel.ChannelAddress);
                                    }
                                }
                                catch
                                {

                                }
                                break;
                            }
                        case SystemMediaTransportControlsButton.Previous:
                            {
                                try
                                {
                                    var channel = Player.Tag as Channel;
                                    if (channel != null && channel.PreviousChannel != null)
                                    {
                                        Player.Stop();
                                        channel.IsPlaying = false;
                                        Player.Tag = channel.PreviousChannel;
                                        Player.Source = new Uri(channel.PreviousChannel.ChannelAddress);
                                    }
                                }
                                catch
                                {

                                }
                                break;
                            }
                        case SystemMediaTransportControlsButton.Pause:
                            {
                                Player.Pause();
                                break;
                            }
                        case SystemMediaTransportControlsButton.Play:
                            {
                                Player.Play();
                                break;

                            }
                    }
                    UpdateSystemControls();
                });

            };
            UpdateSystemControls();
            if (ResourceContext.GetForCurrentView().QualifierValues["DeviceFamily"] != "Mobile")
            {
                BottomAd.Visibility = Visibility.Collapsed;
                LeftAd.Visibility = Visibility.Visible;
                RightAd.Visibility = Visibility.Visible;
            }
            else
            {

                if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
                {
                    BottomAd.Visibility = Visibility.Visible;
                    LeftAd.Visibility = Visibility.Collapsed;
                    RightAd.Visibility = Visibility.Collapsed;
                    StatusBar.GetForCurrentView().HideAsync();
                }
            }
            try
            {
                DispatcherTimer dt = new DispatcherTimer();
                dt.Tick += Dt_Tick;
                dt.Interval = TimeSpan.FromSeconds(30);
                dt.Start();
            }
            catch
            {
                
            }
            SearchFavorites();
        }

        private void Dt_Tick(object sender, object e)
        {
            try
            {
                if (Player.Tag != null && Player.CurrentState == MediaElementState.Playing)
                {
                    (Player.Tag as Channel)?.UpdateCurrentStatus();
                }
            }
            catch
            {

            }
        }

        async Task UpdateSystemControls()
        {
            try
            {
                if (systemControls != null)
                {
                    var channel = Player.Tag as Channel;
                    if (channel != null)
                    {
                        if (channel.NextChannel != null) systemControls.IsNextEnabled = true;
                        else systemControls.IsNextEnabled = false;

                        if (channel.PreviousChannel != null) systemControls.IsPreviousEnabled = true;
                        else systemControls.IsPreviousEnabled = false;

                        if (channel.IsPlaying) systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        else systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    }
                }
            }
            catch
            {

            }
        }
     
        private void PaneOpenCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Splitview.IsPaneOpen = !Splitview.IsPaneOpen;
        }

        private async void FeaturedClick(object sender, TappedRoutedEventArgs e)
        {
            await SearchFavorites();
        }
        async Task SearchFavorites()
        {
            bool thrw = false;
            try
            {

                viewmodel.context = new SearchContext();
                if (viewmodel.IsSearching)
                {
                    thrw = true;
                    throw new Exception("Please wait while current search proccess ends..");
                }
                viewmodel.IsSearching = true;
                var res = await viewmodel.SearchAsync();
                if (res.Count > 0)
                {
                    CurrentNavigationText.Text = "Featured";
                    viewmodel.Results.Clear();
                    foreach (var r in res)
                    {
                        viewmodel.Results.Add(r);
                    }
                    await UpdatePlayerTag();
                }
                else
                {
                    throw new Exception("No results.");
                }
            }
            catch (TaskCanceledException te)
            {

            }
            catch (COMException ce)
            {
                (new MessageDialog("Could not connect!")).ShowAsync();
            }
            catch (Exception ee)
            {
                (new MessageDialog(ee.Message)).ShowAsync();
            }
            finally
            {
                if (!thrw) viewmodel.IsSearching = false;
            }
        }
        private async void Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            VolumeBar.Visibility = Visibility.Collapsed;
            try
            {
                if (GenresComboBox.SelectedIndex == -1) return;
                
                Genre genre = (Genre)GenresComboBox.SelectedItem;
                bool thrw = false;
                try
                {
                    viewmodel.context = new SearchContext(genre);

                    if (viewmodel.IsSearching)
                    {
                        thrw = true;
                        throw new Exception("Please wait while current search proccess ends..");
                    }
                    viewmodel.IsSearching = true;
                    var res = await viewmodel.SearchAsync();
                    if (res.Count > 0)
                    {
                        CurrentNavigationText.Text = "Genres";
                        viewmodel.Results.Clear();
                        foreach (var r in res)
                        {
                            viewmodel.Results.Add(r);
                        }
                        await UpdatePlayerTag();
                    }
                    else
                    {
                        throw new Exception("No results.");
                    }
                }
                catch (TaskCanceledException te)
                {

                }
                catch (Exception ee)
                {
                    (new MessageDialog(ee.Message)).ShowAsync();
                }
                finally
                {
                    if (!thrw) viewmodel.IsSearching = false;
                }



            }
            catch (Exception ee)
            {

            }
        }
        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                Genre genre = (Genre)GenresComboBox.SelectedItem;
                
                bool thrw = false;
                try
                {
                    viewmodel.context = new SearchContext(genre);

                    if (viewmodel.IsSearching)
                    {
                        thrw = true;
                        throw new Exception("Please wait while current search proccess ends..");
                    }
                    viewmodel.IsSearching = true;
                    var res = await viewmodel.SearchAsync();
                    if (res.Count > 0)
                    {
                        viewmodel.Results.Clear();
                        CurrentNavigationText.Text = "Genres";
                        foreach (var r in res)
                        {
                            viewmodel.Results.Add(r);
                        }
                        await UpdatePlayerTag();
                    }
                    else
                    {
                        throw new Exception("No results.");
                    }
                }
                catch (TaskCanceledException te)
                {

                }
                catch (COMException ce)
                {
                    (new MessageDialog("Could not connect!")).ShowAsync();
                }
                catch (Exception ee)
                {
                    (new MessageDialog(ee.Message)).ShowAsync();
                }
                finally
                {
                    if (!thrw) viewmodel.IsSearching = false;
                }



            }
            catch(Exception ee)
            {

            }
        }
        private async void Favorites_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool thrw = false;
            try
            {
                viewmodel.context = new SearchContext(Mode.Favorites);
                viewmodel.context.IsMoreAvailable = false;
                if (viewmodel.IsSearching)
                {
                    thrw = true;
                    throw new Exception("Please wait while current search proccess ends..");
                }
                viewmodel.IsSearching = true;
                var res = await viewmodel.SearchAsync();
                CurrentNavigationText.Text = "Favorites";
                viewmodel.Results.Clear();
                if (res.Count > 0)
                {
                    
                    for (int i=0;i<res.Count; i++)
                    {
                        viewmodel.Results.Add(res[i]);                      
                    }
                    await UpdatePlayerTag();
                }
                else
                {
                    throw new Exception("Favorites are empty.");
                }
            }
            catch (TaskCanceledException te)
            {

            }
            catch (Exception ee)
            {
                (new MessageDialog(ee.Message)).ShowAsync();
            }
            finally
            {
                if (!thrw) viewmodel.IsSearching = false;
            }
        }
        private void ContentGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
        private async void ItemTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            VolumeBar.Visibility = Visibility.Collapsed;

        }
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            bool thrw = false;
            try
            {
                viewmodel.context = new SearchContext(args.QueryText);
               
                if (viewmodel.IsSearching)
                {
                    thrw = true;
                    throw new Exception("Please wait while current search proccess ends..");
                }
                viewmodel.IsSearching = true;
                var res = await viewmodel.SearchAsync();
                if (res.Count > 0)
                {
                    CurrentNavigationText.Text = "Search";
                    viewmodel.Results.Clear();
                    foreach (var r in res)
                    {
                        viewmodel.Results.Add(r);
                    }
                    await UpdatePlayerTag();
                }
                else
                {
                    throw new Exception("No results.");
                }
            }
            catch (COMException ce)
            {
                (new MessageDialog("Could not connect!")).ShowAsync();
            }
            catch (TaskCanceledException te)
            {

            }
            catch (Exception ee)
            {
                (new MessageDialog(ee.Message)).ShowAsync();
            }
            finally
            {
               if(!thrw) viewmodel.IsSearching = false;
            }
        }
        async Task LoadMore()
        {
            try
            {
                var res = await viewmodel.SearchAsync();
                if (res.Count > 0)
                {
                    try
                    {
                        if (viewmodel.Results.Count > 0)
                        {
                            viewmodel.Results.Last().NextChannel = res.First();
                            res.First().PreviousChannel = viewmodel.Results.Last();
                        }
                    }
                    catch
                    {
                        
                    }
                    foreach (var r in res)
                    {
                        viewmodel.Results.Add(r);
                    }
                }

            }
            catch
            {
                viewmodel.context.IsMoreAvailable = false;
            }
            finally
            {
                viewmodel.IsSearching = false;
            }
        }
        private void ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {

                ScrollViewer scrollViewer = (ScrollViewer)sender;
                if (scrollViewer.VerticalOffset / scrollViewer.ScrollableHeight > 0.7 )
                {
                    if (!viewmodel.IsSearching&&viewmodel.context.IsMoreAvailable)
                    {
                        viewmodel.IsSearching = true;
                        LoadMore();
                    }
                }
                else
                {

                }
            }
            catch (Exception)
            {

            }
        }

        private void Player_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Channel tagitem = (Channel)Player.Tag;
                switch (Player.CurrentState)
                {
                    case MediaElementState.Paused:
                        {
                            tagitem.IsPlaying = false;
                            break;
                        }
                    case MediaElementState.Playing:
                        {
                            tagitem.IsPlaying = true;
                            try
                            {
                                viewmodel.ShowRateDialog();
                            }
                            catch
                            {

                            }
                            break;
                        }
                    case MediaElementState.Stopped:
                        {
                            tagitem.IsPlaying = false;
                            break;
                        }
                    case MediaElementState.Opening:
                        {
                            tagitem.IsPlaying = true;
                            break;
                        }
                    case MediaElementState.Closed:
                        {
                            tagitem.IsPlaying = false;
                            break;
                        }

                }
                UpdateSystemControls();
            }
            catch
            {

            }
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            Channel ch = ((Button)sender).DataContext as Channel;
            if (Player.Tag == null)
            {
                Player.Tag = ch;
                Player.Source = new Uri(ch.ChannelAddress);
            }
            else
            {
                Channel tagchannel = (Channel)Player.Tag;
                if (tagchannel.ChannelAddress == ch.ChannelAddress)
                {
                    switch (Player.CurrentState)
                    {
                        case MediaElementState.Playing:
                            {
                                Player.Pause();
                                break;
                            }
                        default:
                            {
                                Player.Play();
                                break;
                            }
                    }
                }
                else
                {
                    var currenttag = Player.Tag as Channel;
                    if (currenttag != null) currenttag.IsPlaying = false;

                    Player.Tag = ch;
                    Player.Source = new Uri(ch.ChannelAddress);
                }
            }
        }

      

        private void NavPlayPauseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var chanel = Player.Tag as Channel;
                if(chanel!=null)
                {
                    if (chanel.IsPlaying) Player.Pause();
                    else Player.Play();
                }
            }
            catch
            {

            }
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var channel = Player.Tag as Channel;
                if (channel != null && channel.NextChannel != null)
                {
                    Player.Stop();
                    channel.IsPlaying = false;
                    Player.Tag = channel.NextChannel;
                    Player.Source = new Uri(channel.NextChannel.ChannelAddress);
                }
            }
            catch
            {

            }
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var channel = Player.Tag as Channel;
                if (channel != null && channel.PreviousChannel != null)
                {
                    Player.Stop();
                    channel.IsPlaying = false;
                    Player.Tag = channel.PreviousChannel;
                    Player.Source = new Uri(channel.PreviousChannel.ChannelAddress);
                }
            }
            catch
            {

            }
        }  

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Channel channel = ((Button)sender).DataContext as Channel;
                if(channel.IsInFavorites)
                {
                   await viewmodel.RemoveFromFavorites(channel);
                    if(viewmodel.context.SearchMode== Mode.Favorites && viewmodel.Results.Contains(channel))
                    {
                        viewmodel.Results.Remove(channel);
                    }
                }
                else
                {
                    
                   await viewmodel.AddToFavorite(channel);
                }
               
            }
            catch
            {

            }
        }

        private void BottomAdRefreshed(object sender, RoutedEventArgs e)
        {
            if (BottomAd.Visibility == Visibility.Visible)
            {
                BottomAdRow.Height = new GridLength(55, GridUnitType.Pixel);
            }
        }

        private void RightAdRefreshed(object sender, RoutedEventArgs e)
        {
            if (RightAd.Visibility == Visibility.Visible)
            {
                RightAdColumn.Width = new GridLength(160, GridUnitType.Pixel);
            }
        }

        private void LeftAdRefreshed(object sender, RoutedEventArgs e)
        {
            if (LeftAd.Visibility == Visibility.Visible)
            {
                LeftAdColumn.Width = new GridLength(160, GridUnitType.Pixel);
            }
        }

        private void LeftAdError(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            if(LeftAdColumn.Width.Value==160)
            {
                LeftAdColumn.Width = new GridLength(1, GridUnitType.Pixel);
            }
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
             
                (new MessageDialog("Failed to open this stream.")).ShowAsync();
            
        }

        private void RightAdError(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            if (RightAdColumn.Width.Value == 160)
            {
                RightAdColumn.Width = new GridLength(1, GridUnitType.Pixel);
            }
        }

        private void BottomAdError(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
           
            if (BottomAdRow.Height.Value == 55)
            {
                BottomAdRow.Height = new GridLength(1, GridUnitType.Pixel);
            }
        }
        public async Task UpdatePlayerTag()
        {
            try
            {
                if (Player.Tag != null)
                {
                   var tagEquals = viewmodel.Results.Where((arg) => 
                    {
                        if ((Player.Tag as Channel)?.ChannelName == arg.ChannelName) return true;
                        else return false;
                    });
                    if(tagEquals.Count()>0)
                    {
                        var oldTag = Player.Tag as Channel;
                        var newTag = tagEquals.First();
                        if(Player.CurrentState== MediaElementState.Playing)
                        {
                            newTag.IsPlaying = true;
                        }
                        Player.Tag = newTag;
                    }
                }
            }
            catch
            {
                Player.Tag = null;
                Player.Source = null;
            }
        }
        private async void GenreTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                Genre genre = (Genre)((Button)sender).DataContext;
                bool thrw = false;
                try
                {
                    viewmodel.context = new SearchContext(genre);

                    if (viewmodel.IsSearching)
                    {
                        thrw = true;
                        throw new Exception("Please wait while current search proccess ends..");
                    }
                    viewmodel.IsSearching = true;
                    var res = await viewmodel.SearchAsync();
                    if (res.Count > 0)
                    {
                        viewmodel.Results.Clear();
                        foreach (var r in res)
                        {
                            viewmodel.Results.Add(r);
                        }
                       await UpdatePlayerTag();
                    }
                    else
                    {
                        throw new Exception("No results.");
                    }
                }
                catch (TaskCanceledException te)
                {

                }
                catch (Exception ee)
                {
                    (new MessageDialog(ee.Message)).ShowAsync();
                }
                finally
                {
                    if (!thrw) viewmodel.IsSearching = false;
                }



            }
            catch (Exception ee)
            {

            }
        }

        private void VolumeIconClick(object sender, RoutedEventArgs e)
        {
        
            VolumeBar.Visibility = VolumeBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RootGridTapped(object sender, TappedRoutedEventArgs e)
        {
            VolumeBar.Visibility = Visibility.Collapsed;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
