using SesameApi;

public static class SesameUtil
{
    public static async Task<Result<SesameCheckInOutInfo, Error>> CheckInOutInfo(Sesame s, string? siteName = null)
    {
        return await s.Login()
            .Bind(s.CurrentUserInfo)
            .Map(userInfoList => userInfoList.First().id)
            .Bind(async userId => await s.SignInSites(userId)
                .Map(
                    siteInfoList => siteInfoList.Where(site => site.name.ToLowerInvariant() == siteName?.ToLowerInvariant())
                        .FirstOrDefault()
                        .AsMaybe()
                        .Map(siteInfo => siteInfo.id)
                ).Map(siteId => new SesameCheckInOutInfo(userId, siteId))
            );
    }
}

public record class SesameCheckInOutInfo(string UserId, Maybe<string> SiteId) { }