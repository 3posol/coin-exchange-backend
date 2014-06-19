﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Common.Tests;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;
using NUnit.Framework;

namespace CoinExchange.IdentityAccess.Infrastructure.IntegrationTests
{
    [TestFixture]
    public class PermisssionRepositoryTests:AbstractConfiguration
    {
        private IPermissionRepository _permissionRepository;

        //properties will be injected based on type
        public IPermissionRepository PermissionRepository
        {
            set { _permissionRepository = value; }
        }

        private DatabaseUtility _databaseUtility;

        [SetUp]
        public void Setup()
        {
            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
        }

        [Test]
        [Category("Integration")]
        public void ReadPermissionMasterData_IfPermissionDataExists_ThereShouldBeSevenPermissions()
        {
            IList<Permission> getPermissions = _permissionRepository.GetAllPermissions();
            Assert.NotNull(getPermissions);
            Assert.AreEqual(8,getPermissions.Count);
            VerifyPermssionValues(getPermissions);
        }

        /// <summary>
        /// Verify permission master data
        /// </summary>
        /// <param name="permissions"></param>
        private void VerifyPermssionValues(IList<Permission> permissions)
        {
            for (int i = 0; i < 8; i++)
            {
                switch (permissions[i].PermissionId)
                {
                    case "QF":
                        Assert.AreEqual("Query Funds", permissions[i].PermissionName);
                        break;
                    case "WF":
                        Assert.AreEqual("Withdraw Funds", permissions[i].PermissionName);
                        break;
                    case "QOOT":
                        Assert.AreEqual("Query Open Orders and Trades", permissions[i].PermissionName);
                        break;
                    case "QCOT":
                        Assert.AreEqual("Query Closed Orders and Trades", permissions[i].PermissionName);
                        break;
                    case "PO":
                        Assert.AreEqual("Place Order", permissions[i].PermissionName);
                        break;
                    case "CO":
                        Assert.AreEqual("Cancel Order", permissions[i].PermissionName);
                        break;
                    case "QLT":
                        Assert.AreEqual("Query Ledger Entries", permissions[i].PermissionName);
                        break;
                    case "DF":
                        Assert.AreEqual("Deposit Funds", permissions[i].PermissionName);
                        break;
                    default:
                        Assert.Fail("Does not exist in permissions:",permissions[i].PermissionId,permissions[i].PermissionName);
                        break;
                }
            }
        }
    }
}
