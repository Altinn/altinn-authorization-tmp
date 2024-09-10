using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;

namespace Altinn.Platform.Authorization.Services.Interface
{
    /// <summary>
    /// Interface for actions related to profile
    /// </summary>
    public interface IProfile
    {
        /// <summary>
        /// Method that fetches the user profile for a given user id
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>The user profile</returns>
        Task<UserProfile> GetUserProfile(int userId);

        /// <summary>
        /// Method that fetches the user profile for a given ssn
        /// </summary>
        Task<UserProfile> GetUserProfileBySSN(string ssn);
    }
}
