﻿using System;
using System.Management.Instrumentation;
using CoinExchange.IdentityAccess.Application.AccessControlServices.Commands;
using CoinExchange.IdentityAccess.Application.SecurityKeysServices;
using CoinExchange.IdentityAccess.Domain.Model.Repositories;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;
using CoinExchange.IdentityAccess.Domain.Model.UserAggregate;

namespace CoinExchange.IdentityAccess.Application.AccessControlServices
{
    /// <summary>
    /// Serves the login operation(s)
    /// </summary>
    public class LoginApplicationService : ILoginApplicationService
    {
        // Get the Current Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IUserRepository _userRepository;
        private IPasswordEncryptionService _passwordEncryptionService;
        private ISecurityKeysApplicationService _securityKeysApplicationService;

        /// <summary>
        /// Initializes with the UserRepository and PasswordEncryption service 
        /// </summary>
        public LoginApplicationService(IUserRepository userRepository, IPasswordEncryptionService passwordEncryptionService,
                                       ISecurityKeysApplicationService securityKeysApplicationService)
        {
            _userRepository = userRepository;
            _passwordEncryptionService = passwordEncryptionService;
            _securityKeysApplicationService = securityKeysApplicationService;
        }

        /// <summary>
        /// Login call by the user, logs user in if username and password are correct
        /// </summary>
        /// <returns></returns>
        public UserValidationEssentials Login(LoginCommand loginCommand)
        {
            User user = _userRepository.GetUserByUserName(loginCommand.Username);
            if (user != null)
            {
                if(_passwordEncryptionService.VerifyPassword(loginCommand.Password, user.Password))
                {
                    Tuple<ApiKey, SecretKey> securityKeys = _securityKeysApplicationService.CreateSystemGeneratedKey(loginCommand.Username);
                    return new UserValidationEssentials(securityKeys, user.AutoLogout);
                }
                else
                {
                    Log.Error(string.Format("Incorrect password for username: {0}", loginCommand.Username));
                }
            }
            else
            {
                Log.Error(string.Format("Invalid username: {0}", loginCommand.Username));
            }
            return null;
        }
    }
}
