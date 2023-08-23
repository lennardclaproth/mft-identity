using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LClaproth.MyFinancialTracker.Identity;

internal class AuthenticationException : Exception {
    public AuthenticationException() : base() { }
    public AuthenticationException(string message) : base($"{typeof(AuthenticationException)}: {message}") { }
    public AuthenticationException(string message, Exception inner) : base($"{typeof(AuthenticationException)}: {message}", inner) { }
}

public class AuthenticationService<T> where T : IdentityUser
{
    private readonly  SignInManager<T> _signInManager;
    private readonly  UserManager<T> _userManager;
    private readonly  PasswordHasher<T> _passwordHasher;
    private readonly  IConfiguration _config;

    public AuthenticationService(SignInManager<T> signInManager, UserManager<T> userManager,  IConfiguration config){
        _signInManager = signInManager;
        _userManager = userManager;
        _passwordHasher = new PasswordHasher<T>();
        _config = config;
    }

    public async Task<string> Authenticate(AuthenticationParams parameters){
        var user = await _userManager.FindByEmailAsync(parameters.Email);
        var canSignInAsync = _signInManager.CanSignInAsync(user);
        var checkPasswordSignInAsync = _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);

        if(user == null){
            throw new AuthenticationException("Invalid credentials");
        }

        if(!await canSignInAsync){
            if(!user.EmailConfirmed){
                throw new AuthenticationException("Email has not been validated yet.");
            }

            if(user.LockoutEnd!=null){
                throw new AuthenticationException($"This account has been locked until [INSERT DATE]");
            }
        }
        var passwordResult = await checkPasswordSignInAsync;
        if(!passwordResult.Succeeded){
            throw new AuthenticationException("Invalid credentials");
        }

        try {
            await _signInManager.SignInAsync(user, false);
        } catch (Exception e){
            throw new AuthenticationException("An error occurred while trying to sign in.", e);
        }

        var token = await GetTokenAsync(user);

        return await Task.FromResult(token);
    } 

    private async Task<string> GetTokenAsync(T user) {
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var signingCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresIn"]));

        var subject = new ClaimsIdentity(new[]{
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName)
        });

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = subject,
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
