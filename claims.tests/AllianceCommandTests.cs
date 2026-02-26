using claims.src.part;
using claims.src.part.structure;
using claims.src.perms;
using Moq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace claims.tests
{
    public class AllianceCommandTests
    {
        private Mock<IServerPlayer> _mockPlayer;
        private Mock<TextCommandCallingArgs> _mockArgs;
        private Mock<IPlayer> _mockIPlayer;
        private Mock<PlayerInfo> _mockPlayerInfo;
        private Mock<Plot> _mockPlot;
        private Mock<City> _mockCity;
        private Mock<PermsHandler> _mockPermsHandler;
        public AllianceCommandTests()
        {
            _mockPlayer = new Mock<IServerPlayer>();
            _mockArgs = new Mock<TextCommandCallingArgs>();
            _mockIPlayer = new Mock<IPlayer>();
            _mockPlayerInfo = new Mock<PlayerInfo>();
            _mockPlot = new Mock<Plot>();
            _mockCity = new Mock<City>();
            _mockPermsHandler = new Mock<PermsHandler>();
        }
        [Fact]
        public void PlotClaim_PlayerInfoNull_ReturnsError()
        {

        }
    }
}
