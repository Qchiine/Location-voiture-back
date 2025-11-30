using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocationVoituresAPI.DTOs;
using LocationVoituresAPI.Services;
using LocationVoituresAPI.Models;

namespace LocationVoituresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
            return Unauthorized("Email ou mot de passe incorrect");

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        // Forcer le type à CLIENT pour l'inscription publique
        registerDto.TypeUtilisateur = TypeUtilisateur.CLIENT;

        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
            return BadRequest("Erreur lors de l'inscription. Vérifiez vos données.");

        return Ok(result);
    }

    [HttpPost("register-employe")]
    [Authorize(Roles = "ADMINISTRATEUR")]
    public async Task<ActionResult<AuthResponseDto>> RegisterEmploye([FromBody] RegisterDto registerDto)
    {
        // Forcer le type à EMPLOYE pour la création par admin
        registerDto.TypeUtilisateur = TypeUtilisateur.EMPLOYE;

        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
            return BadRequest("Erreur lors de la création de l'employé. Vérifiez vos données.");

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
        if (result == null)
            return Unauthorized("Token invalide");

        return Ok(result);
    }
}