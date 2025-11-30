using Domain.Users;
using System;

namespace Api.Dtos
{
    public record UserDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string FullName,
        UserEnums.UserRole Role,
        string? PhoneNumber,
        string? ProfileImage,
        DateTime RegistrationDate)
    {
        public static UserDto FromDomainModel(User user) =>
            new(
                Id: user.Id.Value,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role,
                PhoneNumber: user.PhoneNumber,
                ProfileImage: user.ProfileImage,
                RegistrationDate: user.RegistrationDate
            );
    }

    public record CreateUserDto(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber,
        string? ProfileImage
    );

    public record UpdateUserDto(
        string FirstName,
        string LastName,
        string? PhoneNumber,
        string? ProfileImage,
        UserEnums.UserRole? Role
    );

    public record AdminCreateUserDto(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber,
        string? ProfileImage,
        UserEnums.UserRole Role
    );
}