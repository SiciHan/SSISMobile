using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Team8SSISMobile.Models
{
    public class Role
    {

        [Key]
        public int IdRole { get; set; }
        public string Label { get; set; }

        //OneToMany
        public virtual ICollection<Employee> Employees{ get; set; }
    }
}