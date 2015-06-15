using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WannaSwapWebRole.Models
{

    // The DbContext derived class serves as the link between database and application
    // used by Entity Framework, and LINQ to query/update the database
    // more here: http://stackoverflow.com/questions/13627829/about-dbset-and-dbcontext
    // and here https://msdn.microsoft.com/en-ie/data/jj729737.aspx
    public class WannaSwapContext : DbContext
    {
        // default constructor 
        //The "name=WannaSwapContext" part will tell DbContext to look in Web.config for the attribute named "WannaSwapContext" => the contents are passed to the constructor
        public WannaSwapContext(): base("name=WannaSwapContext")
        {

        }

        // used to perform CRUD operations against a specific type from the model
        // more info here: http://mvc4beginner.com/Tutorial/Introducing-DBContext-&-DBSet.html
        public DbSet<Advert> Adverts { get; set; }
    }
}