﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Client.Tests;
using CoinExchange.Common.Tests;
using Spring.Context;
using Spring.Context.Support;

namespace CoinExchange.Client.Console
{
    /// <summary>
    /// Contains tests that test all the bounded contexts running mutually
    /// </summary>
    public class AllBoundedContextsIntegrationTests
    {
        private AccessControl _accessControl;
        private IdentityAccessClient _identityAccessClient;
        private IApplicationContext _applicationContext;
        private DatabaseUtility _databaseUtility;
        
        private string _baseUrlCloud = "http://rockblanc.cloudapp.net/test/v1";
        private string _baseUrlLocalhost = "http://localhost:51780/v1";
        private FundsClient _fundsClient;
        private string _username = "rodholt";
        private string _password = "mclaren";
        private string _email = "rodholt@mclaren.com";
        private string _baseCurrency = "BTC";
        private string _quoteCurrency = "USD";

        public AllBoundedContextsIntegrationTests()
        {
            _fundsClient = new FundsClient(_baseUrlLocalhost);
            _identityAccessClient = new IdentityAccessClient(_baseUrlLocalhost);
            
            _accessControl = new AccessControl(_identityAccessClient, _password, _username);
        }

        public void Initialization()
        {
            //ClearDatabase();
            UserLogin();

            _fundsClient.key = _identityAccessClient.key;
            _fundsClient.secretkey = _identityAccessClient.secretkey;

            MakeDeposit();
            //SendOrders();
            ApplyForTier1();
            GetLimits();
            //TradeExecuted();
        }

        private  void ClearDatabase()
        {
            //_applicationContext = ContextRegistry.GetContext();
            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
            System.Console.WriteLine("Database cleared and initialized");
        }

        private void UserLogin()
        {
            System.Console.WriteLine("User sign up start");
            _accessControl.CreateAndActivateUser(_username, _password, _email);
            System.Console.WriteLine("User sign up end");

            System.Console.WriteLine("User log in start");
            _accessControl.Login(_username, _password);
            System.Console.WriteLine("User log in end");
        }

        private void MakeDeposit()
        {
            System.Console.WriteLine("Deposit start");
            _fundsClient.MakeDeposit(_baseCurrency, 1000, true);
            _fundsClient.MakeDeposit(_quoteCurrency, 100000, false);
            System.Console.WriteLine("Deposit end");
        }

        

        private void ApplyForTier1()
        {
            System.Console.WriteLine("Tier apply start");
            System.Console.WriteLine(_identityAccessClient.ApplyForTierLevel1("Rod Holt", DateTime.Now.AddYears(57).ToShortDateString(), 
                "+1244322222"));

            System.Console.WriteLine("Tier Apply end");
        }

        private void GetLimits()
        {
            System.Console.WriteLine("Get Limits start");
            System.Console.WriteLine(_fundsClient.GetDepositLimits(_baseCurrency));
            System.Console.WriteLine(_fundsClient.GetWithdrawalLimits(_baseCurrency));
            System.Console.WriteLine("Get Limits end");
        }

        private void SendOrders()
        {
            System.Console.WriteLine("Send orders start");
            System.Console.WriteLine(_identityAccessClient.CreateOrder(_baseCurrency + _quoteCurrency, "limit", "buy", 2, 250));
            System.Console.WriteLine(_identityAccessClient.CreateOrder(_baseCurrency + _quoteCurrency, "limit", "sell", 2, 260));
            System.Console.WriteLine(_identityAccessClient.CreateOrder(_baseCurrency + _quoteCurrency, "limit", "buy", 2, 251));
            System.Console.WriteLine(_identityAccessClient.CreateOrder(_baseCurrency + _quoteCurrency, "limit", "sell", 2, 262));
            System.Console.WriteLine("Send orders end");
        }

        private void TradeExecuted()
        {
            System.Console.WriteLine("Trade execution start");
            System.Console.WriteLine(_identityAccessClient.CreateOrder(_baseCurrency + _quoteCurrency, "limit", "buy", 2, 260));
            System.Console.WriteLine("Trade execution end");
        }
    }
}
