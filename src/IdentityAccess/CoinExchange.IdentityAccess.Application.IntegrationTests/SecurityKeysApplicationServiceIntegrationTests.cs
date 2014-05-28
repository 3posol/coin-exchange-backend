﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using CoinExchange.Common.Tests;
using CoinExchange.IdentityAccess.Application.SecurityKeysServices;
using CoinExchange.IdentityAccess.Application.SecurityKeysServices.Commands;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

namespace CoinExchange.IdentityAccess.Application.IntegrationTests
{
    [TestFixture]
    class SecurityKeysApplicationServiceIntegrationTests
    {
        private IApplicationContext _applicationContext;
        private DatabaseUtility _databaseUtility;
        private ISecurityKeysRepository _securityKeysRepository;
        private IPermissionRepository _permissionRepository;

        [SetUp]
        public void Setup()
        {
            _applicationContext = ContextRegistry.GetContext();
            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _securityKeysRepository = _applicationContext["SecurityKeysPairRepository"] as ISecurityKeysRepository;
            _permissionRepository = _applicationContext["PermissionRespository"] as IPermissionRepository;
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
        }

        [TearDown]
        public void TearDown()
        {
            ContextRegistry.Clear();
            _databaseUtility.Create();
        }

        [Test]
        public void ServiceStartuptest_TestsIfTheServiceStartsAsExpectedThroughSpring_VerifiesThroughTheStateOfTheObject()
        {
            ISecurityKeysApplicationService registrationService =
                (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];
            Assert.IsNotNull(registrationService);
        }

        [Test]
        public void SecurityKeyGenerationService_TestsIfTheSecurityKeysAreGenerated_VerifiesThroughTheReturnedValues()
        {
            ISecurityKeysApplicationService registrationService =
                (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];

            Tuple<ApiKey, SecretKey> securityKeys = registrationService.CreateSystemGeneratedKey("Bob");

            Assert.IsNotNull(securityKeys);
            Assert.IsNotNull(securityKeys.Item1);
            Assert.IsNotNull(securityKeys.Item2);
        }

        [Test]
        [Category("Integration")]
        public void CreateSystemGeneratedSecurityPair_IfUserNameIsProvided_VerifyKeyPairIsReturnedAndPersistedSuccessfully()
        {
             ISecurityKeysApplicationService registrationService =
                (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];
            var keys = registrationService.CreateSystemGeneratedKey("user1");
            Assert.NotNull(keys);
            Assert.IsNotNullOrEmpty(keys.Item1.Value);
            Assert.IsNotNullOrEmpty(keys.Item2.Value);
            SecurityKeysPair persistedKeysPair = _securityKeysRepository.GetByApiKey(keys.Item1.Value);
            Assert.NotNull(persistedKeysPair);
            Assert.AreEqual(persistedKeysPair.UserName, "user1");
            Assert.AreEqual(persistedKeysPair.SystemGenerated, true);
            Assert.AreEqual(persistedKeysPair.ApiKey, keys.Item1.Value);
            Assert.AreEqual(persistedKeysPair.SecretKey,keys.Item2.Value);
            Assert.IsNotNullOrEmpty(persistedKeysPair.KeyDescription);
            Assert.IsNotNullOrEmpty(persistedKeysPair.CreationDateTime.ToString());
        }

        [Test]
        [Category("Integration")]
        public void CreateUserGeneratedSecurityPair_IfAllRequiredParametersAreProvided_VerifyKeyPairIsReturnedAndPersistedSuccessfully()
        {
            List<SecurityKeyPermissionsRepresentation> securityKeyPermissions=new List<SecurityKeyPermissionsRepresentation>();
            IList<Permission> permissions = _permissionRepository.GetAllPermissions();
            for (int i = 0; i < permissions.Count; i++)
            {
                securityKeyPermissions.Add(new SecurityKeyPermissionsRepresentation(true,permissions[i]));
            }
            CreateUserGeneratedSecurityKeyPair command =
                new CreateUserGeneratedSecurityKeyPair(securityKeyPermissions.ToArray(),
                    DateTime.Today.AddDays(1).ToString(), DateTime.Today.AddDays(-2).ToString(),
                    DateTime.Today.AddDays(-1).ToString(), true, true, true, "123", "user1");
            ISecurityKeysApplicationService registrationService =
               (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];
            var keys = registrationService.CreateUserGeneratedKey(command);
            Assert.NotNull(keys);
            Assert.IsNotNullOrEmpty(keys.Item1);
            Assert.IsNotNullOrEmpty(keys.Item2);
            SecurityKeysPair persistedKeysPair = _securityKeysRepository.GetByApiKey(keys.Item1);
            Assert.NotNull(persistedKeysPair);
            Assert.AreEqual(persistedKeysPair.UserName, "user1");
            Assert.AreEqual(persistedKeysPair.SystemGenerated, false);
            Assert.AreEqual(persistedKeysPair.ApiKey, keys.Item1);
            Assert.AreEqual(persistedKeysPair.SecretKey, keys.Item2);
            Assert.IsNotNullOrEmpty(persistedKeysPair.KeyDescription);
            Assert.IsNotNullOrEmpty(persistedKeysPair.CreationDateTime.ToString());
            Assert.AreEqual(persistedKeysPair.EnableStartDate,true);
            Assert.AreEqual(persistedKeysPair.EnableEndDate, true);
            Assert.AreEqual(persistedKeysPair.EnableExpirationDate, true);
            Assert.AreEqual(persistedKeysPair.ExpirationDate, DateTime.Today.AddDays(1));
            Assert.AreEqual(persistedKeysPair.StartDate, DateTime.Today.AddDays(-2));
            Assert.AreEqual(persistedKeysPair.EndDate, DateTime.Today.AddDays(-1));
            ValidatePermissions(persistedKeysPair,securityKeyPermissions.ToArray());
        }

        [Test]
        [Category("Integration")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUserGeneratedSecurityPair_IfNoPermissionIsAssigned_ArgumentNullExceptionShouldBeRaised()
        {
            List<SecurityKeyPermissionsRepresentation> securityKeyPermissions = new List<SecurityKeyPermissionsRepresentation>();
            IList<Permission> permissions = _permissionRepository.GetAllPermissions();
            for (int i = 0; i < permissions.Count; i++)
            {
                securityKeyPermissions.Add(new SecurityKeyPermissionsRepresentation(false, permissions[i]));
            }
            CreateUserGeneratedSecurityKeyPair command =
                new CreateUserGeneratedSecurityKeyPair(securityKeyPermissions.ToArray(),
                    DateTime.Today.AddDays(1).ToString(), DateTime.Today.AddDays(-2).ToString(),
                    DateTime.Today.AddDays(-1).ToString(), true, true, true, "123", "user1");
            ISecurityKeysApplicationService registrationService =
               (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];
            var keys = registrationService.CreateUserGeneratedKey(command);
        }

        [Test]
        [Category("Integration")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUserGeneratedSecurityPair_IfKeyDescriptionAlreadyExists_ArgumentExceptionShouldBeRaised()
        {
            List<SecurityKeyPermissionsRepresentation> securityKeyPermissions = new List<SecurityKeyPermissionsRepresentation>();
            IList<Permission> permissions = _permissionRepository.GetAllPermissions();
            for (int i = 0; i < permissions.Count; i++)
            {
                securityKeyPermissions.Add(new SecurityKeyPermissionsRepresentation(true, permissions[i]));
            }
            CreateUserGeneratedSecurityKeyPair command =
                new CreateUserGeneratedSecurityKeyPair(securityKeyPermissions.ToArray(),
                    DateTime.Today.AddDays(1).ToString(), DateTime.Today.AddDays(-2).ToString(),
                    DateTime.Today.AddDays(-1).ToString(), true, true, true, "123", "user1");
            ISecurityKeysApplicationService registrationService =
               (ISecurityKeysApplicationService)_applicationContext["SecurityKeysApplicationService"];
            var keys = registrationService.CreateUserGeneratedKey(command);
            command=
               new CreateUserGeneratedSecurityKeyPair(securityKeyPermissions.ToArray(),
                   DateTime.Today.AddDays(1).ToString(), DateTime.Today.AddDays(-2).ToString(),
                   DateTime.Today.AddDays(-1).ToString(), true, true, true, "123", "user1");
            var keys1 = registrationService.CreateUserGeneratedKey(command);
        }
        /// <summary>
        /// Validate permissions
        /// </summary>
        /// <param name="keysPair"></param>
        /// <param name="permissions"></param>
        private void ValidatePermissions(SecurityKeysPair keysPair, SecurityKeyPermissionsRepresentation[] permissions)
        {
            for (int i = 0; i < permissions.Length; i++)
            {
                Assert.AreEqual(permissions[i].Allowed,
                    keysPair.ValidatePermission(permissions[i].Permission.PermissionId));
            }
        }
    }
}
