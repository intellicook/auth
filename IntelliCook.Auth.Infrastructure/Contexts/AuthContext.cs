using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IntelliCook.Auth.Infrastructure.Contexts;

public class AuthContext(DbContextOptions<AuthContext> options) : IdentityDbContext<IntelliCookUser>(options);