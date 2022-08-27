namespace StripeDemo.Services
{
    public class UserInfosService : IUserInfosService

    {
        public UserInfos GetCurrentUser()
        {
            return new UserInfos()
            {
                Email = "mboumeden@gmail.com",
                Name = "Mboumeden Giscard"
            };
        }
    }

    public interface IUserInfosService
    {
        UserInfos GetCurrentUser();
    }

    public class UserInfos
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string StripeCustumerId { get; set; }
    }
}
