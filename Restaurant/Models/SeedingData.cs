using AliEns.Data;
using AliEns.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class SeedingData
    {
        private ApplicationDbContext _context;

        public SeedingData(ApplicationDbContext context)
        {
            _context = context;
        }

        public async void SeedAdminUser()
        {
            var user = new ApplicationUser
            {
                Id = "86f344cd-5168-4c2f-8dfb-0eef5bebc743",
                UserName = "ali.nasser.9.1997@gmail.com",
                NormalizedUserName = "ALI.NASSER.9.1997@GMAIL.COM",
                Email = "ali.nasser.9.1997@gmail.com",
                NormalizedEmail = "ALI.NASSER.9.1997@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAELH/5/Lc/L9C2NRodOWyyacXumgzwWHnqBCkzsboQ52TtT2j5SgpbkYt+Cx6QrvbCw==",
                SecurityStamp = "FBX4JWID5VXCSPGFVFRGG2X7KZC2OFND",
                ConcurrencyStamp = "8d8d1c60-048d-484e-9e9a-4e46a87d3a6f",
                LockoutEnabled = true
            };

            var roleStore = new RoleStore<IdentityRole>(_context);

            if (!_context.Roles.Any(e => e.Name == SD.Admin))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = SD.Admin, NormalizedName = SD.Admin.ToUpper() });
            }

            if (!_context.Roles.Any(e => e.Name == SD.ManagerUser))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = SD.ManagerUser, NormalizedName = SD.ManagerUser.ToUpper() });
            }

            if (!_context.Roles.Any(e => e.Name == SD.KitchenUser))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = SD.KitchenUser, NormalizedName = SD.KitchenUser.ToUpper() });
            }

            if (!_context.Roles.Any(e => e.Name == SD.FrontDeskUser))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = SD.FrontDeskUser, NormalizedName = SD.FrontDeskUser.ToUpper() });
            }

            if (!_context.Roles.Any(e => e.Name == SD.EndCustomerUser))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = SD.EndCustomerUser, NormalizedName = SD.EndCustomerUser.ToUpper() });
            }

            if (!_context.Users.Any(e => e.UserName == user.UserName))
            {
                var userStore = new UserStore<IdentityUser>(_context);
                await userStore.CreateAsync(user);
                await userStore.AddToRoleAsync(user, SD.Admin);
            }

            await _context.SaveChangesAsync();
        }
    }
}
