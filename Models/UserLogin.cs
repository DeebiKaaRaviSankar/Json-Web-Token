using System;
using System.ComponentModel.DataAnnotations;
namespace Users.Models{
    public class UserLogin{
        [Key]
        public int Id{get;set;}
        public string? Username{get;set;}
        public string? Password{get;set;}
        public byte[]? PasswordSalt{get;set;}
        public byte[]? PasswordHash{get;set;}


        
        
    }
}