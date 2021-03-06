using Xunit;
using Domain.Interfaces;
using Domain.Models;
using REST_Api.ApiModels;
using REST_Api.Controllers;
using System.Linq;
using Moq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using REST_Api;

namespace XUnitTests
{
    public class UsersControllerTest
    {
        private readonly UsersController _controller;
        //private readonly ILogger<UsersControllerTest> _logger;
        Mock<ITicketRepo> mockRepo = new Mock<ITicketRepo>();
        private Domain.Models.Users user = new Domain.Models.Users
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "tester@email.com",
            Password = "password",
        };

        List<Domain.Models.Users> users = new List<Domain.Models.Users>();

        public UsersControllerTest(/*ILogger<UsersControllerTest> logger*/)
        {
            //Arrange
            
            users.Add(user);
            users.AsEnumerable();

            mockRepo.Setup(repo => repo.GetUsersAsync("User"))
           .ReturnsAsync(users);
            mockRepo.Setup(repo => repo.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
            mockRepo.Setup(repo => repo.GetUserByLoginAsync(user.Email, user.Password))
            .ReturnsAsync(user);
            mockRepo.Setup(repo => repo.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);
            mockRepo.Setup(repo => repo.AddUserAsync(user))
            .Verifiable("user was not added");
            mockRepo.Setup(repo => repo.UpdateUserAsync(user.Id, user))
            .ReturnsAsync(user);
            mockRepo.Setup(repo => repo.UpdateUserPasswordAsync(user.Id, user.Password))
            .ReturnsAsync(user);
            mockRepo.Setup(repo => repo.SaveAsync())
           .ReturnsAsync(true);
            mockRepo.Setup(repo => repo.DeleteUserAsync(1))
           .Verifiable("item was not removed");
            _controller = new UsersController(mockRepo.Object);
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [Fact]
        public async void Test1()
        {
            var result1 = await _controller.GetByIdAsync(1);
            var goodRequestResult = Assert.IsType<OkObjectResult>(result1);
            Assert.Same(Mapper.MapUsers(user).ToString(), goodRequestResult.Value.ToString());

            var result2 = await _controller.GetByIdAsync(2);
            Assert.IsType<NotFoundObjectResult>(result2);
            
        }


        [Fact]
        public async void Test2()
        {
            var result1 = await _controller.GetByLoginAsync("tester@email.com", "password");
            var goodRequestResult = Assert.IsType<OkObjectResult>(result1);
            Assert.Same(Mapper.MapUsers(user).ToString(), goodRequestResult.Value.ToString());

            var result2 = await _controller.GetByLoginAsync("tester@email.com", "1234");
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async void Test3()
        {
            var user2 = new REST_Api.ApiModels.Users
            {
                Id = 2,
                FirstName = "Test3",
                LastName = "User",
                Email = "tester3@email.com",
                Password = "password",
            };
            var result1 = await _controller.PostAsync(user2);
            Assert.IsType<OkObjectResult>(result1);
            

            user2.Email = "tester@email.com";
            var result2 = await _controller.PostAsync(user2);
            Assert.IsType<BadRequestObjectResult>(result2);
        }

        [Fact]
        public async void Test4()
        {
            var user2 = new REST_Api.ApiModels.Users
            {
                Id = 2,
                FirstName = "Test3",
                LastName = "User",
                Email = "tester3@email.com",
                Password = "password",
            };
            var result1 = await _controller.Put(1, user2);
            var goodRequestResult = Assert.IsType<OkObjectResult>(result1);
            Assert.Same(Mapper.MapUsers(user2).ToString(), goodRequestResult.Value.ToString());

            var result2 = await _controller.Put(4, user2);
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async void Test5()
        {
            var user2 = new REST_Api.ApiModels.Users
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "tester@email.com",
                Password = "new password",
            };
            var result1 = await _controller.PutToChangePasswordAsync(1, user2.Password);
            var goodRequestResult = Assert.IsType<OkObjectResult>(result1);
            //Assert.Same(Mapper.MapUsers(user2).ToString(), goodRequestResult.Value.ToString());


            var result2 = await _controller.PutToChangePasswordAsync(5, "1234");
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async void Test6()
        {
            var result1 = await _controller.DeleteAsync(1);
            Assert.IsType<OkObjectResult>(result1);

            var result2 = await _controller.DeleteAsync(5);
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async void Test7()
        {
            var result1 = await _controller.GetAsync("User");
            var goodRequestResult = Assert.IsType<OkObjectResult>(result1);
            Assert.Same(users.Select(Mapper.MapUsers).ToString(), goodRequestResult.Value.ToString());

            var result2 = await _controller.GetAsync("404");
            Assert.IsType<NotFoundObjectResult>(result2);
        }
    }

}

