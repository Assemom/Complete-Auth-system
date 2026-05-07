namespace App.Business.Models;

public record AccessTokenResult(string AccessToken, DateTime ExpiresAt, string[] Roles);
