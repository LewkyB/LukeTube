using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace luke_site_mvc.Tests
{
    public class ChatroomServiceTests
    {
        //private readonly IChatroomService _chatroomService;

        //private readonly Mock<ILogger<ChatroomService>> _loggerMock;
        //private readonly Mock<IDataRepository> _dataRepositoryMock;

        //public ChatroomServiceTests()
        //{
        //    _loggerMock = new Mock<ILogger<ChatroomService>>();
        //    _dataRepositoryMock = new Mock<IDataRepository>();

        //    _chatroomService = new ChatroomService(
        //        _loggerMock.Object,
        //        _dataRepositoryMock.Object,
        //        new HttpClient());
        //}

        //[Fact]
        //public async Task TesterAsync()
        //{
        //    var result = await _chatroomService.DownloadJSON();

        //    Assert.NotNull(result);
        //}

    }
}
