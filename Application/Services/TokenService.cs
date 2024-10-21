using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateToken(string email)
    {
        // Get the issuer from configuration or throw an exception
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidConfigurationException("Invalid jwt issuer configuration");
    
        // Get the audience from configuration or throw an exception
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidConfigurationException("Invalid jwt audience configuration");
    
        // Get the key from configuration or throw an exception
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidConfigurationException("Invalid jwt key configuration")));
    
        // Create the token descriptor with claims, issuer, audience, and expiration time
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>
            {
                {"email", email}
            },
            
            Expires = DateTime.UtcNow.Add(GetTokenLifetime()),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = issuer,
            Audience = audience
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // Create the token
        var token = tokenHandler.CreateToken(tokenDescriptor); 
        return tokenHandler.WriteToken(token);
    }
    
    private TimeSpan GetTokenLifetime()
    {
        // Get the token lifetime string from the configuration
        string tokenLifetimeString = _configuration["Jwt:TokenLifetime"] ?? 
                                     throw new InvalidConfigurationException("Invalid token lifetime configuration");
    
        // Define the format for the token lifetime string
        string format = @"hh\:mm\:ss";
    
        // Set the format provider for the token lifetime string
        IFormatProvider formatProvider = CultureInfo.InvariantCulture; 
    
        // Convert the token lifetime string to a TimeSpan
        if (TimeSpan.TryParseExact(tokenLifetimeString, format, formatProvider, out TimeSpan tokenLifetime))
        {
            return tokenLifetime;
        }
    
        throw new InvalidConfigurationException("Invalid token lifetime configuration");
    }

}