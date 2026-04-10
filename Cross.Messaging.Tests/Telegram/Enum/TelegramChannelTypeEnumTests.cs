namespace Cross.Messaging.Tests.Telegram.Enum;

public sealed class TelegramChannelTypeEnumTests
{
    [Test]
    public void Values_ShouldMatchDeclaredOrdinals()
    {
        ((int)TelegramChannelTypeEnum.Public).Should().Be(0);
        ((int)TelegramChannelTypeEnum.Private).Should().Be(1);
        ((int)TelegramChannelTypeEnum.Group).Should().Be(2);
        ((int)TelegramChannelTypeEnum.Supergroup).Should().Be(3);
        ((int)TelegramChannelTypeEnum.Bot).Should().Be(4);
    }

    [Test]
    public void Enum_ShouldContainFiveMembers()
    {
        System.Enum.GetValues<TelegramChannelTypeEnum>().Should().HaveCount(5);
    }
}
