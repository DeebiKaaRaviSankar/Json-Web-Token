using System;
using Microsoft.EntityFrameworkCore;
namespace Users.Models{
    public class userDbcontext : DbContext{
        public userDbcontext(DbContextOptions<userDbcontext> options) : base(options){

        }

        public virtual DbSet<UserLogin> Users{get;set;}
        // public virtual DbSet<UserDTO> userDTO{get;set;}

    }
}