using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentFin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoviesPage : Page
    {
        public MoviesPage()
        {
            this.InitializeComponent();
            LoadMoviesAsync();
        }

        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();

        private async void LoadMoviesAsync()
        {
            try
            {
                JellyfinClient clientKeys = new JellyfinClient("", "");
                string jellyfinUrl = clientKeys.RetrieveString("serverUrl_Key");
                var apiKey = clientKeys.RetrieveToken();
                string libraryId = "f137a2dd21bbc1b99aa5c0f6bf02a805";
                JellyfinClient client = new JellyfinClient(jellyfinUrl, apiKey);
                var movies = await client.GetMoviesInLibraryAsync(libraryId);

                foreach (var movie in movies)
                {
                    Console.WriteLine($"{movie.Title} {movie.Year} {movie.PosterUrl}");
                    Movies.Add(new Movie { Title = movie.Title, Year = movie.Year, PosterUrl = movie.PosterUrl });
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading libraries: {ex.Message}");
            }
        }
    }

    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string PosterUrl { get; set; }
    }
}
