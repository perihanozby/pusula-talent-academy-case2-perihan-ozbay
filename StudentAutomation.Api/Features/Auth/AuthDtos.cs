namespace StudentAutomation.Api.Features.Auth;

public record RegisterRequest(string Email, string Password, string FullName, string Role);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string FullName, string Role);
