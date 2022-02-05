using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Infrastructure
{
    [HtmlTargetElement("td",Attributes = "identity-role")]
    public class RoleUsersTagHelper:TagHelper
    {
        private UserManager<AppUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;
        public RoleUsersTagHelper(UserManager<AppUser> UserManager, RoleManager<IdentityRole> RoleManager)
        {
            this.UserManager = UserManager;
            this.RoleManager = RoleManager;
        }
        [HtmlAttributeName("identity-role")]
        public string RoleId { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> names = new List<string>();
            IdentityRole RoleName =await RoleManager.FindByIdAsync(RoleId);
            if (RoleName !=null)
            {
                foreach (var user in UserManager.Users)
                {
                    if (user!=null && await UserManager.IsInRoleAsync(user,RoleName.Name))
                    {
                        names.Add(user.UserName);
                    }
                }
            }
            output.Content.SetContent(names.Count == 0 ? "No Users" : string.Join(", ", names));
            
        }
    }
}
