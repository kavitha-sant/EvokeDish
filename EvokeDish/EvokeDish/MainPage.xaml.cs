using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvokeDish.Pages;
using EvokeDish.ViewModels;
using Xamarin.Forms;

namespace EvokeDish
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;
        public MainPage()
        {
            InitializeComponent();
        }
        public void OnRecipesButtonClicked(object sender, EventArgs args)
        {
            // create a new NavigationPage, with a new RecipeListPage set as the Root
            var navPage = new NavigationPage(new RecipeListPage
            {
                Title = "Recipes",
                BindingContext = new RecipeListViewModel()
            });

            navPage.BarTextColor = Color.White;

            // set the MainPage of the app to the navPage
            Application.Current.MainPage = navPage;
        }
    }
}
