using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public interface ICategoryDAO
    {
        IEnumerable<Category> FindAllCategories();
        Category Create(Category category);
        Category Update(Category category);
        Category Delete (int id);
    }
}