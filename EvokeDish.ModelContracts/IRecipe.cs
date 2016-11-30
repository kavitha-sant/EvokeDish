using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.ModelContracts
{
    // This interface is shared between the backend and the client code to enforce the model contract.

    public interface IRecipe
    {
        string Name { get; set; }
        //List<IIngredient> Ingredients { get; set; }
        string Instructions { get; set; }
        string ImageURL { get; set; }
        string OriginalURL { get; set; }
    }

    public interface IIngredient
    {
        string Quantity { get; set; }
        string Name { get; set; }
        string Type { get; set; }
    }
}
