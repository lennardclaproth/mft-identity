using Microsoft.AspNetCore.Identity;

namespace LClaproth.MyFinancialTracker.Identity;

internal class RegistrationException : Exception{
    public RegistrationException() { }
    public RegistrationException(string message) : base($"{typeof(RegistrationException)}: {message}") { }
    public RegistrationException(string message, Exception inner) : base($"{typeof(RegistrationException)}: {message}", inner) { }
}

public class RegistrationService<T> where T : IdentityUser
{
    private readonly UserManager<T> _userManager;

    public RegistrationService(UserManager<T> userManager){
        _userManager = userManager;
    }

    public async Task<string> Register(AccountCreationParams userParams){
        // Here the code will go to send an email and so on.
        var user = new IdentityUser(){
            UserName = userParams.Email,
            Email = userParams.Email,
        };

        if(!userParams.Password.Equals(userParams.ConfirmPassword)){
            throw new RegistrationException("Passwords do not match, account could not be created.");
        }

        try {
            var result = await _userManager.CreateAsync((T) user, userParams.Password);
            if(!result.Succeeded){  
                throw new RegistrationException($"An error occurred while trying to create an account. code: {result.Errors.First().Code}. {result.Errors.First().Description}");
            }
            return user.UserName;
        } catch (RegistrationException e){
            throw e;
        } 
        catch (Exception e){
            throw new RegistrationException("An error occurred while trying to register.", e);
        }
    }
}
