﻿namespace DataAccess.DataAccess.Models;

public class UserModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Adress { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public List<OrderModel>? Orders { get; set; }
    public string Role { get; set; } = "Customer";
}