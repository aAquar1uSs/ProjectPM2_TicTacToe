using TicTacToe.Server.DTO;

namespace TicTacToe.Server.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Update information about users form list, if login and password valid,
        /// sends the response to the client and log-in users in account.
        /// </summary>
        /// <param name="account">User account. <see cref="UserAccountDto"/></param>
        /// <returns>
        /// A task that represents the asynchronous operation. <see cref="Task"/>
        /// </returns>
        /// <exception cref="Exceptions.AccountException">Throws if input login, password incorrect or
        /// user has already entered into account.</exception>
        /// <exception cref="TimeoutException">Throws if user try log-in account three times .</exception>
        Task InvokeLoginAsync(UserAccountDto account);

        /// <summary>
        /// Update information about users form list, if login and password valid,
        /// sends the response to the client and registered new account.
        /// </summary>
        /// <param name="account">User account. <see cref="UserAccountDto"/>.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. <see cref="Task"/>
        /// </returns>
        /// <exception cref="Exceptions.AccountException">Throws if input login,
        /// password are already taken</exception>
        Task InvokeRegistrationAsync(UserAccountDto account);

        /// <summary>
        /// Method remove active player from list.
        /// </summary>
        /// <param name="login"></param>
        void RemoveActiveAccountByLogin(string login);
    }
}
