using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvokeDish.Abstractions;
using EvokeDish.Constants;
using MvvmHelpers;
using Xamarin.Forms;
using EvokeDish.Models;
using FormsToolkit;
using Microsoft.Practices.ServiceLocation;
using EvokeDish.Util;

namespace EvokeDish.ViewModels
{
    public class RecipeListViewModel : BaseNavigationViewModel
    {
        public RecipeListViewModel()
        {
            SubscribeToAddRecipeMessages();

            SubscribeToUpdateRecipeMessages();

            SubscribeToDeleteRecipeMessages();

            SetDataSource();
        }

        IDataSource<Recipe> _DataSource;

        ObservableRangeCollection<Recipe> _Recipes;

        Command _LoadRecipesCommand;

        Command _RefreshRecipesCommand;

        Command _NewRecipeCommand;

        Command _ShowSettingsCommand;

        void SetDataSource()
        {
            _DataSource = ServiceLocator.Current.GetInstance<IDataSource<Recipe>>();
        }

        public ObservableRangeCollection<Recipe> Recipes
        {
            get { return _Recipes ?? (_Recipes = new ObservableRangeCollection<Recipe>()); }
            set
            {
                _Recipes = value;
                OnPropertyChanged("Recipes");
            }
        }

        /// <summary>
        /// Command to load Recipes
        /// </summary>
        public Command LoadRecipesCommand
        {
            get { return _LoadRecipesCommand ?? (_LoadRecipesCommand = new Command(async () => await ExecuteLoadRecipesCommand())); }
        }

        public async Task ExecuteLoadRecipesCommand()
        {
            LoadRecipesCommand.ChangeCanExecute();

            // set the data source on each load, because we don't know if the data source may have been updated between page loads
            SetDataSource();

            if (Settings.LocalDataResetIsRequested)
                _Recipes.Clear();

            if (Recipes.Count < 1 || !Settings.DataIsSeeded || Settings.ClearImageCacheIsRequested)
                await FetchRecipes();

            LoadRecipesCommand.ChangeCanExecute();
        }

        public Command RefreshRecipesCommand
        {
            get
            {
                return _RefreshRecipesCommand ?? (_RefreshRecipesCommand = new Command(async () => await ExecuteRefreshRecipesCommandCommand()));
            }
        }

        async Task ExecuteRefreshRecipesCommandCommand()
        {
            RefreshRecipesCommand.ChangeCanExecute();

            await FetchRecipes();

            RefreshRecipesCommand.ChangeCanExecute();
        }

        async Task FetchRecipes()
        {
            IsBusy = true;

            Recipes = new ObservableRangeCollection<Recipe>(await _DataSource.GetItems());

            // ensuring that this flag is reset
            Settings.ClearImageCacheIsRequested = false;

            IsBusy = false;
        }

        /// <summary>
        /// Command to create new Recipe
        /// </summary>
        public Command NewRecipeCommand
        {
            get
            {
                return _NewRecipeCommand ??
                    (_NewRecipeCommand = new Command(async () => await ExecuteNewRecipeCommand()));
            }
        }

        async Task ExecuteNewRecipeCommand()
        {
            //await PushAsync(new RecipeEditPage() { BindingContext = new RecipeEditViewModel() });
        }

        /// <summary>
        /// Command to show settings
        /// </summary>
        public Command ShowSettingsCommand
        {
            get
            {
                return _ShowSettingsCommand ??
                    (_ShowSettingsCommand = new Command(async () => await ExecuteShowSettingsCommand()));
            }
        }

        async Task ExecuteShowSettingsCommand()
        {
            //var navPage = new NavigationPage(
            //    new SettingsPage() { BindingContext = new SettingsViewModel() })
            //{
            //    BarBackgroundColor = Color.FromHex("547799")
            //};

            //navPage.BarTextColor = Color.White;

            //await PushModalAsync(navPage);
        }

        /// <summary>
        /// Subscribes to "AddRecipe" messages
        /// </summary>
        void SubscribeToAddRecipeMessages()
        {
            MessagingService.Current.Subscribe<Recipe>(MessageKeys.AddRecipe, async (service, recipe) =>
            {
                IsBusy = true;

                await _DataSource.AddItem(recipe);

                await FetchRecipes();

                IsBusy = false;
            });
        }

        /// <summary>
        /// Subscribes to "UpdateRecipe" messages
        /// </summary>
        void SubscribeToUpdateRecipeMessages()
        {
            MessagingService.Current.Subscribe<Recipe>(MessageKeys.UpdateRecipe, async (service, recipe) =>
            {
                IsBusy = true;

                await _DataSource.UpdateItem(recipe);

                await FetchRecipes();

                IsBusy = false;
            });
        }

        /// <summary>
        /// Subscribes to "DeleteRecipe" messages
        /// </summary>
        void SubscribeToDeleteRecipeMessages()
        {
            MessagingService.Current.Subscribe<Recipe>(MessageKeys.DeleteRecipe, async (service, recipe) =>
            {
                IsBusy = true;

                await _DataSource.RemoveItem(recipe);

                await FetchRecipes();

                IsBusy = false;
            });
        }
    }
}
