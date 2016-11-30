using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvokeDish.ViewModels;
using Xamarin.Forms;

namespace EvokeDish.Pages
{
    public partial class RecipeListPage : ContentPage
    {
        protected RecipeListViewModel ViewModel => BindingContext as RecipeListViewModel;
        public RecipeListPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The action to take when a list item is tapped.
        /// </summary>
        /// <param name="sender"> The sender.</param>
        /// <param name="e">The ItemTappedEventArgs</param>
        void ItemTapped(object sender, ItemTappedEventArgs e)
        {
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ViewModel.ExecuteLoadRecipesCommand();

            ListView recipesLV = this.FindByName<ListView>("RecipesLV");

            NavigationPage.SetHasNavigationBar(this, true);
        }
    }
}
