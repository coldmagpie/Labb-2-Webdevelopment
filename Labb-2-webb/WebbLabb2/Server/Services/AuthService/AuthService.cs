﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using DataAccess.DataAccess.DataContext;
using DataAccess.DataAccess.Models;
using DataAccess.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebbLabb2.Shared;
using WebbLabb2.Shared.DTOs;

namespace WebbLabb2.Server.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResponse<int>> RegisterUserAsync(UserModel user, string password)
        {
            if (await CheckUserExistsAsync(user.Email))
            {
                return new ServiceResponse<int>()
                {
                    Error = true,
                    Message = "User already exists"
                };
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _unitOfWork.UserRepository.AddUserAsync(user);
            await _unitOfWork.Save();

            return new ServiceResponse<int>()
            {
                Data = user.Id,
                Error = false,
                Message = "Register Succeed!"
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<ServiceResponse<string>> LoginUserAsync(string email, string password)
        {
            if (!await CheckUserExistsAsync(email))
            {
                return new ServiceResponse<string>()
                {
                    Error = true,
                    Message = "Wrong user name or password"
                };
            }

            var user = await _unitOfWork.UserRepository.GetUserAsync(email);

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return new ServiceResponse<string>()
                {
                    Error = true,
                    Message = "Wrong user name or password"
                };
            }

            return new ServiceResponse<string>()
            {
                Data = CreateToken(user),
                Error = false,
                Message = $"Welcome, {user.FirstName}!"
            };

        }

        public async Task<ServiceResponse<bool>> UpdateProfile(int userId, UserProfileDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetUserById(userId);
            if (user is null)
            {
                return new ServiceResponse<bool>
                {
                    Error = true,
                    Message = "User not found."
                };
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Adress = dto.Adress;
            await _unitOfWork.Save();

            return new ServiceResponse<bool> { Data = true, Message = "Profile has been updated." };
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetUserById(userId);
            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Error = true,
                    Message = "User not found."
                };
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _unitOfWork.Save();

            return new ServiceResponse<bool> { Data = true, Message = "Password has been changed." };
        }


        private string CreateToken(UserModel user)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.Role, user.Role),
                new (ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var secret = _configuration.GetSection("AppSecrets:Secret").Value;

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddDays(1),
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPasswordHash(string password, byte[] userPasswordHash, byte[] userPasswordSalt)
        {
            using (var hmac = new HMACSHA512(userPasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(userPasswordHash);
            }
        }

        public async Task<bool> CheckUserExistsAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(email);
            return user is null ? false : true;
        }
    }
}

