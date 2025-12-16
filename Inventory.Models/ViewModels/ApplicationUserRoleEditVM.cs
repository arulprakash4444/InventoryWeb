using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModels
{
    public class ApplicationUserRoleEditVM
    {
        public string UserId { get; set; }

        [ValidateNever]
        public string UserName { get; set; }   // Display only

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }

        [RegularExpression("^(Admin|Employee)$", ErrorMessage = "Role must be either Admin or Employee.")]
        public string SelectedRole { get; set; }
    }
}
