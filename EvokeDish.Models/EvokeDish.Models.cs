using EvokeDish.ModelContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acquaint.Models;
using Xamarin.Forms;

namespace EvokeDish.Models
{
    public class Recipe : ObservableEntityData, IRecipe
    {
        public string DataPartitionId { get; set; }

        private string _ImageURL;
        public string ImageURL
        {
            get { return _ImageURL; }

            set { SetProperty(ref _ImageURL, value); }
        }

        //private List<IIngredient> _Ingredients;
        //public List<IIngredient> Ingredients
        //{
        //    get { return _Ingredients; }

        //    set { SetProperty(ref _Ingredients, value); }
        //}

        private string _Name;
        public string Name
        {
            get { return _Name; }

            set { SetProperty(ref _Name, value); }
        }

        private string _OriginalURL;
        public string OriginalURL
        {
            get { return _OriginalURL; }

            set { SetProperty(ref _OriginalURL, value); }
        }

        private string _Instructions;
        public string Instructions
        {
            get { return _Instructions; }

            set { SetProperty(ref _Instructions, value); }
        }
    }

    public class Ingredient : IIngredient
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }

        private string _Quantity;
        public string Quantity
        {
            get { return _Quantity; }

            set { _Quantity = value; }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }

            set { _Type = value; }
        }
    }
}
