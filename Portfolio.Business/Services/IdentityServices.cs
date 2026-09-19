using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Data.Identity;
namespace Portfolio.Business.Services;
public class UserManagementService(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles) : IUserManagementService
{
    public async Task<List<UserEditDto>> ListAsync(CancellationToken ct=default){var list=new List<UserEditDto>();foreach(var u in await users.Users.AsNoTracking().ToListAsync(ct)){var r=await users.GetRolesAsync(u);list.Add(new UserEditDto{Id=u.Id,Email=u.Email??"",UserName=u.UserName??"",FullName=u.FullName,IsActive=u.IsActive,Role=r.FirstOrDefault()??""});}return list;}
    public async Task<UserEditDto?> GetAsync(Guid id,CancellationToken ct=default){var u=await users.FindByIdAsync(id.ToString());if(u is null)return null;var r=await users.GetRolesAsync(u);return new(){Id=u.Id,Email=u.Email??"",UserName=u.UserName??"",FullName=u.FullName,IsActive=u.IsActive,Role=r.FirstOrDefault()??""};}
    public async Task<(bool Success,string Error)> SaveAsync(UserEditDto dto,CancellationToken ct=default){var u=dto.Id==Guid.Empty?new ApplicationUser{Id=Guid.NewGuid(),CreatedAt=DateTime.UtcNow}:await users.FindByIdAsync(dto.Id.ToString());if(u is null)return(false,"User not found.");u.Email=dto.Email;u.UserName=dto.UserName=string.IsNullOrWhiteSpace(dto.UserName)?dto.Email:dto.UserName;u.FullName=dto.FullName;u.IsActive=dto.IsActive;IdentityResult result=dto.Id==Guid.Empty?await users.CreateAsync(u,dto.Password??"Admin@123456"):await users.UpdateAsync(u);if(!result.Succeeded)return(false,string.Join(" ",result.Errors.Select(e=>e.Description)));if(dto.Id==Guid.Empty && !string.IsNullOrWhiteSpace(dto.Role))await users.AddToRoleAsync(u,dto.Role);if(dto.Id!=Guid.Empty){var current=await users.GetRolesAsync(u);if(current.Any())await users.RemoveFromRolesAsync(u,current);if(!string.IsNullOrWhiteSpace(dto.Role))await users.AddToRoleAsync(u,dto.Role);if(!string.IsNullOrWhiteSpace(dto.Password)){var token=await users.GeneratePasswordResetTokenAsync(u);var pr=await users.ResetPasswordAsync(u,token,dto.Password);if(!pr.Succeeded)return(false,string.Join(" ",pr.Errors.Select(e=>e.Description)));}}return(true,"");}
    public async Task DeleteAsync(Guid id,CancellationToken ct=default){var u=await users.FindByIdAsync(id.ToString());if(u is not null){await users.DeleteAsync(u);}}
}
public class RoleManagementService(PortfolioDbContext db,RoleManager<ApplicationRole> roles) : IRoleManagementService
{
    public async Task<List<RoleEditDto>> ListAsync(CancellationToken ct=default)=>await roles.Roles.AsNoTracking().Select(x=>new RoleEditDto{Id=x.Id,Name=x.Name??"",Description=x.Description}).ToListAsync(ct);
    public async Task<RoleEditDto?> GetAsync(Guid id,CancellationToken ct=default){var r=await roles.FindByIdAsync(id.ToString());if(r is null)return null;var ids=await db.RolePermissions.Where(x=>x.RoleId==id).Select(x=>x.PermissionId).ToListAsync(ct);return new(){Id=r.Id,Name=r.Name??"",Description=r.Description,PermissionIds=ids};}
    public async Task<List<(Guid Id,string Name)>> PermissionsAsync(CancellationToken ct=default) { var items = await db.Permissions.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new { x.Id, x.Name }).ToListAsync(ct); return items.Select(x=>(x.Id,x.Name)).ToList(); }
    public async Task<(bool Success,string Error)> SaveAsync(RoleEditDto dto,CancellationToken ct=default){ApplicationRole? r=dto.Id==Guid.Empty?new ApplicationRole{Id=Guid.NewGuid()}:await roles.FindByIdAsync(dto.Id.ToString());if(r is null)return(false,"Role not found.");r.Name=dto.Name;r.NormalizedName=dto.Name.ToUpperInvariant();r.Description=dto.Description;var result=dto.Id==Guid.Empty?await roles.CreateAsync(r):await roles.UpdateAsync(r);if(!result.Succeeded)return(false,string.Join(" ",result.Errors.Select(e=>e.Description)));var old=await db.RolePermissions.Where(x=>x.RoleId==r.Id).ToListAsync(ct);db.RolePermissions.RemoveRange(old);foreach(var p in dto.PermissionIds.Distinct())db.RolePermissions.Add(new RolePermission{RoleId=r.Id,PermissionId=p});await db.SaveChangesAsync(ct);return(true,"");}
    public async Task DeleteAsync(Guid id,CancellationToken ct=default){var r=await roles.FindByIdAsync(id.ToString());if(r is null)return;if(r.Name==Portfolio.Data.Seed.RoleConstants.SuperAdmin)return;await roles.DeleteAsync(r);}
}
